namespace Binner.Model.Integrations.Tme
{
    public class Category
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int Depth { get; set; }
        public string? Name { get; set; }
        public int TotalProducts { get; set; }
        public int SubTreeCount { get; set; }
        public string? Thumbnail { get; set; }
    }
}
