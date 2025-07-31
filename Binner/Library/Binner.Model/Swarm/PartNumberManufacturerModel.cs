namespace Binner.Model.Swarm
{
    public class PartNumberManufacturerModel
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int PartNumberManufacturerModelId { get; set; }

        /// <summary>
        /// The part number
        /// </summary>
        public int PartNumberManufacturerId { get; set; }

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
    }
}
