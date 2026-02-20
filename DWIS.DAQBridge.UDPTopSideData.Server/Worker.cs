using DWIS.Client.ReferenceImplementation.OPCFoundation;
using DWIS.RigOS.Common.Worker;
using OSDC.DotnetLibraries.General.Common;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using DWIS.DAQBridge.UDPTopSideData.Model;

namespace DWIS.DAQBridge.UDPTopSideData.Server
{
    public class Worker : DWISWorker<ConfigurationForUDP>
    {
        public static string DefaultCulture = "nb-NO";
        private Model.UDPTopSideData UDPTopSideData { get; set; } = new Model.UDPTopSideData();

        private int UDPPort { get; set; } = 1502;

        private CultureInfo Culture { get; set; } = new CultureInfo(DefaultCulture);

        private UdpClient? UDPClient { get; set; } = null;

        public Worker(ILogger<IDWISWorker<ConfigurationForUDP>> logger, ILogger<DWISClientOPCF>? loggerDWISClient) : base(logger, loggerDWISClient)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (Configuration is not null && Configuration is ConfigurationForUDP confUDP && confUDP.UDPPort is not null)
            {
                UDPPort = confUDP.UDPPort.Value;
                Culture = new CultureInfo((!string.IsNullOrEmpty(confUDP.Culture)) ? confUDP.Culture : DefaultCulture);
            }
            ConnectToUDP();
            ConnectToBlackboard();
            if (Configuration is not null && _DWISClient != null && _DWISClient.Connected)
            {
                await RegisterToBlackboard(UDPTopSideData);
                await Loop(stoppingToken);
            }
        }

        protected override async Task Loop(CancellationToken stoppingToken)
        {
            PeriodicTimer timer = new PeriodicTimer(LoopSpan);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ReadUDP();
                    DateTime d1 = DateTime.UtcNow;
                    await PublishBlackboardAsync(UDPTopSideData, stoppingToken);
                    DateTime d2 = DateTime.UtcNow;
                    double elapsed = (d2 - d1).TotalSeconds;
                    lock (_lock)
                    {
                        if (Logger is not null && Logger.IsEnabled(LogLevel.Information) &&
                            UDPTopSideData.StandPipePressure is not null &&
                            UDPTopSideData.StandPipePressure.Value is not null)
                        {
                            Logger.LogInformation("UDP Data SPP: " + UDPTopSideData.StandPipePressure.Value.Value.ToString("F3"));
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger?.LogError(e.ToString());
                }
                ConfigurationUpdater<ConfigurationForUDP>.Instance.UpdateConfiguration(this);
            }
        }

        protected virtual async Task ReadUDP()
        {
            if (UDPClient is not null)
            {
                EmptyData();
                try
                {
                    UdpReceiveResult result = await UDPClient.ReceiveAsync();
                    if (result.Buffer is not null)
                    {
                        string str = Encoding.UTF8.GetString(result.Buffer);
                        if (!string.IsNullOrEmpty(str))
                        {
                            if (Configuration is not null && Configuration.UseJson)
                            {
                                string[]? r = JsonSerializer.Deserialize<string[]>(str);
                                if (r is not null)
                                {
                                    foreach (string s in r)
                                    {
                                        ParseContent(s);
                                    }
                                }
                            }
                            else
                            {
                                string[] lines = str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                if (lines is not null && lines.Length > 2 && lines[0] == "&&" && lines[lines.Length - 1] == "!!")
                                {
                                    for (int i = 1; i < lines.Length - 1; i++)
                                    {
                                        string line = lines[i];
                                        if (!string.IsNullOrEmpty(line))
                                        {
                                            ParseWITS0Content(line);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger?.LogError(e.ToString());
                }
            }
        }

        protected virtual void EmptyData()
        {
            foreach (var prop in typeof(Model.UDPTopSideData).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType == typeof(ScalarProperty))
                {
                    var propVal = prop.GetValue(UDPTopSideData);
                    if (propVal is null)
                    {
                        UDPTopSideData.CreateProperty(prop);
                        propVal = prop.GetValue(UDPTopSideData);
                    }
                    if (propVal is not null && propVal is ScalarProperty scalProp)
                    {
                        scalProp.Value = null;
                    }
                }
            }
        }

        protected virtual void ConnectToUDP()
        {
            if (UDPClient is null)
            {
                UDPClient = new UdpClient(UDPPort);
                UDPClient.EnableBroadcast = true;
            }
        }

        protected virtual void ParseWITS0Content(string line)
        {
            if (!string.IsNullOrEmpty(line) && line.Length >= 4 && UDPTopSideData is not null)
            {
                string code = line.Substring(0, 4);
                string sval = line.Substring(4);
                if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(sval))
                {
                    string srecord = code.Substring(0, 2);
                    string srow = code.Substring(2);
                    if (int.TryParse(srecord, out int record) && int.TryParse(srow, out int row) && double.TryParse(sval, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                    {
                        if (record == 1)
                        {
                            switch (row)
                            {
                                case 8: // bit depth (m)
                                    if (UDPTopSideData.BottomOfStringDepth is not null)
                                    {
                                        // convert m
                                        val *= 1.0;
                                        UDPTopSideData.BottomOfStringDepth.Value = val;
                                    }
                                    break;
                                case 12: // block position (m)
                                    if (UDPTopSideData.BlockPosition is not null)
                                    {
                                        // convert m
                                        val *= 1.0;
                                        UDPTopSideData.BlockPosition.Value = val;
                                    }
                                    break;
                                case 13: // ROP (m/h)
                                    if (UDPTopSideData.SurfaceRateOfPenetration is not null)
                                    {
                                        // convert m/hr
                                        val /= 3600.0;
                                        UDPTopSideData.SurfaceRateOfPenetration.Value = val;
                                    }
                                    break;
                                case 14: // Hookload (KDaN)
                                    if (UDPTopSideData.HookLoadAtAnchor is not null)
                                    {
                                        // convert ton
                                        val *= 1000 * Constants.EarthStandardSurfaceGravitationalAcceleration;
                                        UDPTopSideData.HookLoadAtAnchor.Value = val;
                                    }
                                    break;
                                case 16: // SWOB (KDaN)
                                    if (UDPTopSideData.SurfaceWeightOnBit is not null)
                                    {
                                        // convert ton
                                        val *= 1000 * Constants.EarthStandardSurfaceGravitationalAcceleration;
                                        UDPTopSideData.SurfaceWeightOnBit.Value = val;
                                    }
                                    break;
                                case 21: // SPP (kPa)
                                    if (UDPTopSideData.StandPipePressure is not null)
                                    {
                                        // convert bar
                                        val *= 1e3;
                                        val += Constants.EarthStandardAtmosphericPressure;
                                        UDPTopSideData.StandPipePressure.Value = val;
                                    }
                                    break;
                                case 23: // SPM #1
                                    break;
                                case 24: // SPM #2
                                    break;
                                case 26: // Active Vol (m3)
                                    if (UDPTopSideData.ActiveVolume is not null)
                                    {
                                        // convert m3
                                        val *= 1.0;
                                        UDPTopSideData.ActiveVolume.Value = val;
                                    }
                                    break;
                                case 33: // Mud Temp In (degC)
                                    if (UDPTopSideData.DrillingFluidTemperatureIn is not null)
                                    {
                                        // convert degC
                                        val += Constants.ZeroCelsius;
                                        UDPTopSideData.DrillingFluidTemperatureIn.Value = val;
                                    }
                                    break;
                                case 34: // Mud Temp Out (degC)
                                    if (UDPTopSideData.DrillingFluidTemperatureOut is not null)
                                    {
                                        // convert degC
                                        val += Constants.ZeroCelsius;
                                        UDPTopSideData.DrillingFluidTemperatureOut.Value = val;
                                    }
                                    break;
                                case 41: // Top-drive RPM
                                    if (UDPTopSideData.RotatingDriveSystemRotationalSpeed is not null)
                                    {
                                        // convert RPM
                                        val = 2.0 * Math.PI * val / 60.0;
                                        UDPTopSideData.RotatingDriveSystemRotationalSpeed.Value = val;
                                    }
                                    break;
                                case 42: // Top-drive torque (kN.m)
                                    if (UDPTopSideData.RotatingDriveSystemTorque is not null)
                                    {
                                        // convert kNm
                                        val *= 1000.0;
                                        UDPTopSideData.RotatingDriveSystemTorque.Value = val;
                                    }
                                    break;
                                case 43: // MP1 flow (L/min)
                                    if (UDPTopSideData.FlowrateIn is not null)
                                    {
                                        // convert L/min
                                        val /= 60000.0;
                                        if (UDPTopSideData.FlowrateIn.Value is null)
                                        {
                                            UDPTopSideData.FlowrateIn.Value = 0.0;
                                        }
                                        UDPTopSideData.FlowrateIn.Value += val;
                                    }
                                    break;
                                case 44: // MP2 flow (L/min)
                                    if (UDPTopSideData.FlowrateIn is not null)
                                    {
                                        // convert L/min
                                        val /= 60000.0;
                                        if (UDPTopSideData.FlowrateIn.Value is null)
                                        {
                                            UDPTopSideData.FlowrateIn.Value = 0.0;
                                        }
                                        UDPTopSideData.FlowrateIn.Value += val;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void ParseContent(string signal)
        {
            if (!string.IsNullOrEmpty(signal))
            {
                string[] tokens = signal.Split(';');
                if (tokens is not null && tokens.Length >= 3)
                {
                    try
                    {
                        string prop = tokens[0];
                        DateTime timeStamp = DateTime.ParseExact(tokens[1], "dd.MM.yyyy HH:mm:ss", Culture);
                        double val = double.Parse(tokens[2], Culture);
                        if (prop == "TD RPM")
                        {
                            if (UDPTopSideData.RotatingDriveSystemRotationalSpeed is not null)
                            {
                                // convert RPM
                                val = 2.0 * Math.PI * val / 60.0;
                                UDPTopSideData.RotatingDriveSystemRotationalSpeed.Value = val;
                            }
                        }
                        else if (prop == "TD Torque")
                        {
                            if (UDPTopSideData.RotatingDriveSystemTorque is not null)
                            {
                                // convert kNm
                                val *= 1000.0;
                                UDPTopSideData.RotatingDriveSystemTorque.Value = val;
                            }
                        }
                        else if (prop == "WOB")
                        {
                            if (UDPTopSideData.SurfaceWeightOnBit is not null)
                            {
                                // convert ton
                                val *= 1000 * Constants.EarthStandardSurfaceGravitationalAcceleration;
                                UDPTopSideData.SurfaceWeightOnBit.Value = val;
                            }
                        }
                        else if (prop == "ROP")
                        {
                            if (UDPTopSideData.SurfaceRateOfPenetration is not null)
                            {
                                // convert m/hr
                                val /= 3600.0;
                                UDPTopSideData.SurfaceRateOfPenetration.Value = val;
                            }
                        }
                        else if (prop == "Hook pos")
                        {
                            if (UDPTopSideData.BlockPosition is not null)
                            {
                                // convert m
                                val *= 1.0;
                                UDPTopSideData.BlockPosition.Value = val;
                            }
                        }
                        else if (prop == "Hook load")
                        {
                            if (UDPTopSideData.HookLoadAtAnchor is not null)
                            {
                                // convert ton
                                val *= 1000 * Constants.EarthStandardSurfaceGravitationalAcceleration;
                                UDPTopSideData.HookLoadAtAnchor.Value = val;
                            }
                        }
                        else if (prop == "Standpipe press")
                        {
                            if (UDPTopSideData.StandPipePressure is not null)
                            {
                                // convert bar
                                val *= 1e5;
                                val += Constants.EarthStandardAtmosphericPressure;
                                UDPTopSideData.StandPipePressure.Value = val;
                            }
                        }
                        else if (prop == "Flow in MP1")
                        {
                            if (UDPTopSideData.FlowrateIn is not null)
                            {
                                // convert L/min
                                val /= 60000.0;
                                if (UDPTopSideData.FlowrateIn.Value is null)
                                {
                                    UDPTopSideData.FlowrateIn.Value = 0.0;
                                }
                                UDPTopSideData.FlowrateIn.Value += val;
                            }
                        }
                        else if (prop == "Flow in MP2")
                        {
                            if (UDPTopSideData.FlowrateIn is not null)
                            {
                                // convert L/min
                                val /= 60000.0;
                                if (UDPTopSideData.FlowrateIn.Value is null)
                                {
                                    UDPTopSideData.FlowrateIn.Value = 0.0;
                                }
                                UDPTopSideData.FlowrateIn.Value += val;
                            }

                        }
                        else if (prop == "Active tank")
                        {
                            if (UDPTopSideData.ActiveVolume is not null)
                            {
                                // convert m3
                                val *= 1.0;
                                UDPTopSideData.ActiveVolume.Value = val;
                            }
                        }
                        else if (prop == "Temp in")
                        {
                            if (UDPTopSideData.DrillingFluidTemperatureIn is not null)
                            {
                                // convert degC
                                val += Constants.ZeroCelsius;
                                UDPTopSideData.DrillingFluidTemperatureIn.Value = val;
                            }
                        }
                        else if (prop == "Temp out")
                        {
                            if (UDPTopSideData.DrillingFluidTemperatureOut is not null)
                            {
                                // convert degC
                                val += Constants.ZeroCelsius;
                                UDPTopSideData.DrillingFluidTemperatureOut.Value = val;
                            }
                        }
                        else if (prop == "TD load pin")
                        {
                            if (UDPTopSideData.HookLoadAtTopDrive is not null)
                            {
                                // convert ton
                                val *= 1000.0 * Constants.EarthStandardSurfaceGravitationalAcceleration;
                                UDPTopSideData.HookLoadAtTopDrive.Value = val;
                            }
                        }
                        else if (prop == "Bit Depth")
                        {
                            if (UDPTopSideData.BottomOfStringDepth is not null)
                            {
                                // convert m
                                val *= 1.0;
                                UDPTopSideData.BottomOfStringDepth.Value = val;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger?.LogError(e.ToString());
                    }
                }
            }
        }
    }
}
