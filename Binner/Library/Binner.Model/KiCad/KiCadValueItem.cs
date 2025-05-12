namespace Binner.Model.KiCad
{
    public class KiCadValueItem
    {
        public string Value { get; set; } = string.Empty;

        public KiCadValueItem() { }
        public KiCadValueItem(string? value)
        {
            Value = value ?? string.Empty;
        }
    }
}
