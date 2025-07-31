namespace Binner.Model.Swarm
{
    public class PartNumberManufacturerParametric
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int PartNumberManufacturerParametricId { get; set; }

        /// <summary>
        /// The part number
        /// </summary>
        public int PartNumberManufacturerId { get; set; }

        /// <summary>
        /// The parametric name
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// The type of parametric value
        /// </summary>
        public ParametricTypes ParametricType { get; set; }

        /// <summary>
        /// The units that the parametric describes if applicable
        /// </summary>
        public ParametricUnits? Units { get; set; }

        /// <summary>
        /// Value as a number
        /// </summary>
        public double? ValueNumber { get; set; }

        /// <summary>
        /// Value as a string
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Value as a bool
        /// </summary>
        public bool? ValueBool { get; set; }

        /// <summary>
        /// Original DigiKey parameter id
        /// </summary>
        public int DigiKeyParameterId { get; set; }

        /// <summary>
        /// Original DigiKey parameter text
        /// </summary>
        public string? DigiKeyParameterText { get; set; }

        /// <summary>
        /// Original DigiKey parameter type name
        /// </summary>
        public string? DigiKeyParameterType { get; set; }

        /// <summary>
        /// Original DigiKey value id
        /// </summary>
        public string? DigiKeyValueId { get; set; }

        /// <summary>
        /// Original DigiKey value text
        /// </summary>
        public string? DigiKeyValueText { get; set; }
    }
}
