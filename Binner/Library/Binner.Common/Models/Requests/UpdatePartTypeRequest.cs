namespace Binner.Common.Models
{
    public class UpdatePartTypeRequest
    {
        /// <summary>
        /// Part Type id
        /// </summary>
        public long PartTypeId { get; set; }

        /// <summary>
        /// Name of Part Type
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Description of project
        /// </summary>
        public long? ParentPartTypeId { get; set; }
    }
}
