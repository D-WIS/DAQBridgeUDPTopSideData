using DWIS.Client.ReferenceImplementation.OPCFoundation;
using DWIS.RigOS.Common.Worker;
using Microsoft.Extensions.Logging;

namespace DWIS.DAQBridge.UDPTopSideData.Sink
{
    public class Worker : DWISWorker<Configuration, object>
    {
        private Model.UDPTopSideData UDPTopSideData { get; set; } = new Model.UDPTopSideData();

        public Worker(ILogger<IDWISWorker<Configuration>> logger, ILogger<DWISClientOPCF>? loggerDWISClient) : base(logger, loggerDWISClient)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectToBlackboard();
            if (_DWISClient != null && _DWISClient.Connected)
            {
                await RegisterQueries(UDPTopSideData);
                await Loop(stoppingToken);
            }
        }

        protected override async Task Loop(CancellationToken cancellationToken)
        {
            PeriodicTimer timer = new PeriodicTimer(LoopSpan);
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await ReadBlackboardAsync(UDPTopSideData, cancellationToken);
                if (Logger is not null && Logger.IsEnabled(LogLevel.Information) &&
                    UDPTopSideData.StandPipePressure is not null &&
                    UDPTopSideData.StandPipePressure.Value is not null)
                {
                    Logger.LogInformation("UDP Datagram data SPP: " + UDPTopSideData.StandPipePressure.Value.Value.ToString("F3"));
                }
                ConfigurationUpdater<Configuration>.Instance.UpdateConfiguration(this);
            }
        }
    }
}
