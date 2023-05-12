namespace Binner.Model.Requests
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
        /// Name or SVG content of icon
        /// If left empty, default icon choices will be applied.
        /// </summary>
        public string? Icon { get;set; }

        /// <summary>
        /// Description of project
        /// </summary>
        public long? ParentPartTypeId { get; set; }
    }
}
