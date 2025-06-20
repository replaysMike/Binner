namespace Binner.Model.Integrations.Mouser
{
    public class MouserCategory
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? ParentId { get; set; }
        public string? Url { get; set; }
        public List<MouserCategory> ChildCategories { get; set; } = new List<MouserCategory>();
        public override string ToString()
            => Name ?? string.Empty;
    }
}