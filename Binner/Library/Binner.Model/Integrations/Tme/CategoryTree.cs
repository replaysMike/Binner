namespace Binner.Model.Integrations.Tme
{
    public class CategoryTree
    {
        public int TotalProducts { get; set; }
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int Depth { get; set; }
        public string? Name { get; set; }
        public string? Thumbnail { get; set; }
        public int SubTreeCount { get; set; }
        public List<CategoryTree>? SubTree { get; set; } = new ();
    }
}
