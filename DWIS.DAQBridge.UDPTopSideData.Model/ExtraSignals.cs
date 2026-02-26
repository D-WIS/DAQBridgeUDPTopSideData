using DWIS.API.DTO;
using DWIS.RigOS.Common.Worker;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DWIS.DAQBridge.UDPTopSideData.Model
{
    public class ExtraSignals : DWISDataWithOPCUA
    {
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, OPCUANode>> LocalOPCUANodes = new(BuildOPCUANodes(typeof(ExtraSignals)));
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> LocalSparQLQueries = new(BuildSparQLQueries(typeof(ExtraSignals)));
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> LocalManifests = new(BuildManifests(typeof(ExtraSignals), "BaseStarDataManifest", "Halliburton", "DWISBridge"));
        public override Lazy<IReadOnlyDictionary<PropertyInfo, OPCUANode>> OPCUANodes { get => LocalOPCUANodes; }
        public override Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> SparQLQueries { get => LocalSparQLQueries; }
        public override Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> Manifests { get => LocalManifests; }

        [OPCUANode("http://ddhub.no/BaseStarDataManifest/Variables/", "BaseStarDataManifest.", "PumpStrokeRate1")]
        public ScalarProperty? PumpStrokeRate1 { get; set; } = null;

        [OPCUANode("http://ddhub.no/BaseStarDataManifest/Variables/", "BaseStarDataManifest.", "PumpStrokeRate2")]
        public ScalarProperty? PumpStrokeRate2 { get; set; } = null;

        [OPCUANode("http://ddhub.no/BaseStarDataManifest/Variables/", "BaseStarDataManifest.", "BottomHoleDepth")]
        public ScalarProperty? BottomHoleDepth { get; set; } = null;

    }
}
