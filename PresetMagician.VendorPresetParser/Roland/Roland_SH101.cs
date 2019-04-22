using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.Roland
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Roland_SH101: RolandPlugoutParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1449227056};

        protected override string GetProductName()
        {
            return "SH-101";
        }

        protected override byte[] GetExportConfig()
        {
            return VendorResources.Roland_SH_101_ExportConfig;
        }

        public override byte[] GetSuffixData()
        {
            return VendorResources.Roland_SH_101_Suffix;
        }

        public override byte[] GetDefinitionData()
        {
            return VendorResources.Roland_SH_101_Script;
        }
    }
    
    
}