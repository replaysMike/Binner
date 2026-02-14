namespace Binner.Global.Common
{
    public class CustomFieldValue
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long CustomFieldValueId { get; set; }

        /// <summary>
        /// The field that this value is associated with
        /// </summary>
        public long CustomFieldId { get; set; }

        /// <summary>
        /// The type of data this custom field holds.
        /// Duplicated to make it easier to query
        /// </summary>
        public CustomFieldTypes CustomFieldTypeId { get; set; }

        /// <summary>
        /// The primary key record that this value is associated with
        /// </summary>
        public long RecordId { get; set; }

        /// <summary>
        /// The value of the custom field
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Modification date
        /// </summary>
        public DateTime DateModifiedUtc { get; set; }

        public Guid GlobalId { get; set; }
    }
}
