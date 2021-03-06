using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MSpectralPan : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297313872};

        protected override string PresetFile { get; } = "MSpectralPanpresets.xml";

        protected override string RootTag { get; } = "MSpectralPanpresets";
    }
}