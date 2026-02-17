namespace Binner.Model
{
    public class PartModel
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long PartModelId { get; set; }

        /// <summary>
        /// The associated part
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// Name of model
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Filename of model
        /// </summary>
        public string? Filename { get; set; } = string.Empty;

        /// <summary>
        /// Part model type
        /// </summary>
        public PartModelTypes ModelType { get; set; }

        /// <summary>
        /// The source where the model came from
        /// </summary>
        public PartModelSources Source { get; set; }

        /// <summary>
        /// Provided if the model has a url associated with it
        /// </summary>
        public string? Url { get; set; }

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
