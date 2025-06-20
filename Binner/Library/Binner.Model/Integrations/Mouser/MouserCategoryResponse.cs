namespace Binner.Model.Integrations.Mouser
{
    /// <summary>
    /// All Mouser categories
    /// </summary>
    public class MouserCategoryResponse
    {
        /// <summary>
        /// List of mouser categories, hierarchical
        /// </summary>
        public List<MouserCategory> Categories { get; set; } = new List<MouserCategory>();
    }
}