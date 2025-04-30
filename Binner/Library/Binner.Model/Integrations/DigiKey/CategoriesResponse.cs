namespace Binner.Model.Integrations.DigiKey
{
    public class CategoriesResponse
    {
        public int ProductCount { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; } = null!;
        public long ProductCount { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();
    }
}
