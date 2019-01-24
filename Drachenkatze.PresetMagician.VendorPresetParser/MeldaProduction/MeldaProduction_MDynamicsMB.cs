using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MDynamicsMB: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131368, 1296131424};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MMultiBandDynamicspresets.xml", "MMultiBandDynamicspresetspresets");
        }
    }
}