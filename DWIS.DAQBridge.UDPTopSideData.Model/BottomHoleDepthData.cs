using DWIS.API.DTO;
using DWIS.RigOS.Common.Worker;
using DWIS.Vocabulary.Schemas;
using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DWIS.DAQBridge.UDPTopSideData.Model
{
    public class BottomHoleDepthData : DWISData
    {
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> LocalSparQLQueries = new(BuildSparQLQueries(typeof(BottomHoleDepthData)));
        private static readonly Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> LocalManifests = new(BuildManifests(typeof(BottomHoleDepthData), "UDPTopSideDataManifest", "NORCE", "DWISBridge"));
        public override Lazy<IReadOnlyDictionary<PropertyInfo, Dictionary<string, QuerySpecification>>> SparQLQueries { get => LocalSparQLQueries; }
        public override Lazy<IReadOnlyDictionary<PropertyInfo, ManifestFile>> Manifests { get => LocalManifests; }

        [AccessToVariable(CommonProperty.VariableAccessType.Readable)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticExclusiveOr(1, 2)]
        [SemanticDiracVariable("BH_depth")]
        [SemanticFact("BH_depth", Nouns.Enum.DynamicDrillingSignal)]
        [SemanticFact("BH_depth#01", Nouns.Enum.Measurement)]
        [SemanticFact("BH_depth#01", Nouns.Enum.ContinuousDataType)]
        [SemanticFact("BH_depth#01", Verbs.Enum.HasDynamicValue, "BH_depth")]
        [SemanticFact("BH_depth#01", Verbs.Enum.IsOfMeasurableQuantity, DrillingPhysicalQuantity.QuantityEnum.DepthDrilling)]
        [OptionalFact(1, "movingAverageBH_depth", Nouns.Enum.MovingAverage)]
        [OptionalFact(1, "BH_depth#01", Verbs.Enum.IsTransformationOutput, "movingAverageBH_depth")]
        [OptionalFact(1, "curvilinearAbscissaFrame#01", Nouns.Enum.OneDimensionalCurviLinearReferenceFrame)]
        [OptionalFact(1, "BH_depth#01", Verbs.Enum.HasReferenceFrame, "curvilinearAbscissaFrame#01")]
        [OptionalFact(1, "bh#01", Nouns.Enum.HoleBottomLocation)]
        [OptionalFact(1, "BH_depth#01", Verbs.Enum.IsPhysicallyLocatedAt, "bh#01")]
        [OptionalFact(2, "BH_depth#01", Nouns.Enum.HoleDepth)]
        [MQTTTopic("DWIS/Measurement/DepthDrilling/HoleBottomLocation/BottomHoleDepth")]
        public ScalarProperty? BottomHoleDepth { get; set; } = null;

    }
}
