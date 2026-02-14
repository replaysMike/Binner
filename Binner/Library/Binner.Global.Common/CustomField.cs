namespace Binner.Global.Common
{
    public class CustomField
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long CustomFieldId { get; set; }

        /// <summary>
        /// The type of data this custom field holds
        /// </summary>
        public CustomFieldTypes CustomFieldTypeId { get; set; }

        /// <summary>
        /// Name of the custom field
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Description of the custom field
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Modification date
        /// </summary>
        public DateTime DateModifiedUtc { get; set; }
    }
}
