namespace Binner.Model.KiCad
{
    public class KiCadFields
    {
        public KiCadValueVisibleItem Footprint { get; set; } = new();
        public KiCadValueVisibleItem Datasheet { get; set; } = new();
        public KiCadValueItem Value { get; set; } = new();
        public KiCadValueItem Reference { get; set; } = new();
        public KiCadValueVisibleItem Description { get; set; } = new();
        public KiCadValueVisibleItem Keywords { get; set; } = new();
        public KiCadValueVisibleItem? Custom1 { get; set; }
        public KiCadValueVisibleItem? Custom2 { get; set; }
        public KiCadValueVisibleItem? Custom3 { get; set; }
    }
}
