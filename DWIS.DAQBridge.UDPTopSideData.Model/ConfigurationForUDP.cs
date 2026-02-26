using DWIS.RigOS.Common.Worker;

namespace DWIS.DAQBridge.UDPTopSideData.Model
{
    public class ConfigurationForUDP : ConfigurationForOPCUA
    {
        public int? UDPPort { get; set; } = null;
        public string? Culture { get; set; } = null;

        public bool UseJson { get; set; } = false;
    }
}
