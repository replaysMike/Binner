namespace Binner.Common.Models
{
    public class CreatePartTypeRequest
    {
        /// <summary>
        /// Name of Part Type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of project
        /// </summary>
        public long? ParentPartTypeId { get; set; }
    }
}
