namespace Binner.Model
{
    /// <summary>
    /// Common part category (used as metadata from suppliers only)
    /// </summary>
    public class CommonCategory
    {
        /// <summary>
        /// Child categories
        /// </summary>
        public ICollection<CommonCategory> ChildCategories { get; set; } = new List<CommonCategory>();

        /// <summary>
        /// Category name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Category description
        /// </summary>
        public string? Description { get; set; }
    }
}
