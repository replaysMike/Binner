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
        /// Width of label in inches
        /// </summary>
        public string Width { get; set; } = null!;

        /// <summary>
        /// Height of label in inches
        /// </summary>
        public string Height { get; set; } = null!;

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
    }
}
