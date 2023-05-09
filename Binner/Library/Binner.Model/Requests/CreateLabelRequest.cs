namespace Binner.Model.Requests
{
    public class CreateLabelRequest : CustomLabelRequest
    {
        /// <summary>
        /// Label name
        /// </summary>
        public string Name { get; set; } = null!;

        public int? LabelId { get; set; }

        /// <summary>
        /// True to set this label as the default part label
        /// </summary>
        public bool IsDefaultPartLabel { get; set; }
    }
}
