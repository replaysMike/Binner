namespace Binner.Model
{
    public class Label
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int LabelId { get; set; }

        /// <summary>
        /// The label template to use (defines the label size)
        /// </summary>
        public int LabelTemplateId { get; set; }

        /// <summary>
        /// The label name
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Template stored as JSON
        /// </summary>
        public string Template { get; set; } = null!;

        /// <summary>
        /// If set to true, this record is used for the part label template
        /// </summary>
        public bool IsPartLabelTemplate { get; set; }

        public LabelTemplate? LabelTemplate { get; set; }
    }
}
