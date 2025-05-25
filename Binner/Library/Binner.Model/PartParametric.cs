namespace Binner.Model
{
    public class PartParametric
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long PartParametricId { get; set; }

        /// <summary>
        /// The associated part
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// Name of parametric
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Value of parametric
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Value as a number if numeric
        /// </summary>
        public double ValueNumber { get; set; }

        /// <summary>
        /// The measurement units of the value
        /// </summary>
        public ParametricUnits Units { get; set; }

        public int DigiKeyParameterId { get; set; }

        public int DigiKeyValueId { get; set; }

        public string? DigiKeyParameterType { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Modification date
        /// </summary>
        public DateTime DateModifiedUtc { get; set; }
    }
}
