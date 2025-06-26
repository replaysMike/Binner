namespace Binner.Global.Common
{
    /// <summary>
    /// Stores minimal value of CustomFieldValue
    /// </summary>
    public class CustomValue
    {
        public string Field { get; set; } = null!;
        public string? Value { get; set; }

        public CustomValue() { }

        public CustomValue(string name, string? value)
        {
            Field = name;
            Value = value;
        }
    }
}
