namespace Binner.Model.KiCad
{
    public class KiCadValueVisibleItem
    {
        public string Value { get; set; } = null!;
        public string Visible { get; set; } = "true"; // "true", "false" (why not bool KiCad lol?)
    }
}
