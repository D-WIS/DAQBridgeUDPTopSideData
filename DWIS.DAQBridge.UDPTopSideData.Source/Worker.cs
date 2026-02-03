using DWIS.Client.ReferenceImplementation.OPCFoundation;
using DWIS.DAQBridge.UDPTopSideData.Model;
using DWIS.RigOS.Common.Worker;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace DWIS.DAQBridge.UDPTopSideData.Source
{
    public class Worker : DWISWorker<ConfigurationForUDP>
    {
        public static string DefaultCulture = "nb-NO";
        private int UDPPort { get; set; } = 1502;

        private IPEndPoint? Destination { get; set; } = null;

        private string json = @"[""TD RPM;14.10.2024 07:35:10;-0,138"",
""TD Torque;14.10.2024 07:35:10;-0,163"",
""WOB;14.10.2024 07:35:10;19,636"",
""ROP;14.10.2024 07:35:10;0,044"",
""Hook pos;14.10.2024 07:35:10;28,210"",
""Hook load;14.10.2024 07:35:10;25,057"",
""Mud pump1 SPM;14.10.2024 07:35:10;0,778"",
""Mud pump2 SPM;14.10.2024 07:35:10;-6,678"",
""Standpipe press;14.10.2024 07:35:10;1,332"",
""Flow in MP1;14.10.2024 07:35:10;1,433"",
""Flow in MP2;14.10.2024 07:35:10;0,695"",
""Active tank;14.10.2024 07:35:10;26,196"",
""Temp in;14.10.2024 07:35:10;8,506"",
""Temp out;14.10.2024 07:35:10;-24,949"",
""TD load pin;14.10.2024 07:35:10;23,016"",
""Bit Depth;14.10.2024 07:35:10;-1,526""]";

        private CultureInfo Culture { get; set; } = new CultureInfo(DefaultCulture);

        private UdpClient? UDPClient { get; set; } = null;

        public Worker(ILogger<IDWISWorker<ConfigurationForUDP>> logger, ILogger<DWISClientOPCF>? loggerDWISClient) : base(logger, loggerDWISClient)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartUDPServer();
            if (Configuration is not null && UDPClient != null)
            {
                await Loop(stoppingToken);
            }
        }

        protected override async Task Loop(CancellationToken stoppingToken)
        {
            PeriodicTimer timer = new PeriodicTimer(LoopSpan);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (UDPClient != null)
                {
                    await PublishUDPAsync();
                    lock (_lock)
                    {
                        if (Logger is not null && Logger.IsEnabled(LogLevel.Information) && !string.IsNullOrEmpty(json))
                        {
                            Logger.LogInformation("datagram: " + json);
                        }
                    }
                }
                ConfigurationUpdater<ConfigurationForUDP>.Instance.UpdateConfiguration(this);
            }
        }

        protected void StartUDPServer()
        {
            if (UDPClient is null)
            {
                Destination = new IPEndPoint(IPAddress.Broadcast, UDPPort);
                UDPClient = new UdpClient();    
                UDPClient.EnableBroadcast = true;
                UDPClient.Connect(Destination);
            }
        }

        protected async Task PublishUDPAsync()
        {
            if (UDPClient is not null && !string.IsNullOrEmpty(json))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                await UDPClient.SendAsync(buffer, buffer.Length);
            }
        }
    }
}
