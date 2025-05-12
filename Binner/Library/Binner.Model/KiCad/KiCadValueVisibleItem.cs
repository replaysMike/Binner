namespace Binner.Model.KiCad
{
    public class KiCadValueVisibleItem
    {
        public string Value { get; set; } = string.Empty;
        public string Visible { get; set; } = "false"; // "true", "false" (why not bool KiCad lol?)

        public KiCadValueVisibleItem() { }
        public KiCadValueVisibleItem(string? value)
        {
            Value = value ?? string.Empty;
        }

        public KiCadValueVisibleItem(string? value, bool visible) : this(value)
        {
            Visible = visible ? "true" : "false";
        }
    }
}
