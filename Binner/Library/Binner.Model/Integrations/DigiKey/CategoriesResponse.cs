namespace Binner.Model.Integrations.DigiKey
{
    public class CategoriesResponse
    {
        public int ProductCount { get; set; }
        public ICollection<FullCategory> Categories { get; set; } = new List<FullCategory>();
    }

    public class FullCategory
    {
        public int CategoryId { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; } = null!;
        public long ProductCount { get; set; }
        public ICollection<FullCategory> Children { get; set; } = new List<FullCategory>();
    }
}
