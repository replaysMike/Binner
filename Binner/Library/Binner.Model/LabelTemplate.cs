namespace Binner.Model
{
    public class LabelTemplate
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int LabelTemplateId { get; set; }

        /// <summary>
        /// Name of label template
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Template description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Driver name value (for CUPS drivers)
        /// </summary>
        public string? DriverName { get; set; }

        /// <summary>
        /// Driver extra data value
        /// </summary>
        public string? ExtraData { get; set; }

        /// <summary>
        /// Driver width value in pixels
        /// </summary>
        public int DriverWidth { get; set; }

        /// <summary>
        /// Driver height value in pixels
        /// </summary>
        public int DriverHeight { get; set; }

        /// <summary>
        /// Width of label in inches
        /// </summary>
        public string Width { get; set; } = null!;

        /// <summary>
        /// Height of label in inches
        /// </summary>
        public string Height { get; set; } = null!;

        /// <summary>
        /// Number of labels (for 2 up stacked labels)
        /// </summary>
        public int LabelCount { get; set; }

        /// <summary>
        /// Label margins in the format of "0 0 0 0" or "0" or "0 0"
        /// </summary>
        public string Margin { get; set; } = null!;

        /// <summary>
        /// Dpi to use for template
        /// </summary>
        public int Dpi { get; set; }

        /// <summary>
        /// The paper source (left or right, if applicable)
        /// </summary>
        public int LabelPaperSource { get; set; }

        public int? OrganizationId { get; set; }

        public bool IsEditable => OrganizationId != null;
    }
}
