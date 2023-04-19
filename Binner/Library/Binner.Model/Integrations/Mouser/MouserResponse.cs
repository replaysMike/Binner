namespace Binner.Model.Integrations.Mouser
{
    public class MouserResponse
    {
        public ICollection<Error>? Errors { get; set; }
    }

    public class Error
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
        public string? ResourceKey { get; set; }
        public string? ResourceFormatString { get; set; }
        public string? ResourceFormatString2 { get; set; }
        public string? PropertyName { get; set; }
    }
}
