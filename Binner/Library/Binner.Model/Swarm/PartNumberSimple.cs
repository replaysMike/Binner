namespace Binner.Model.Swarm
{
    public class PartNumberSimple
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int PartNumberId { get; set; }

        /// <summary>
        /// The name of the part number
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Part type name
        /// </summary>
        public string? PartType { get; set; }

        /// <summary>
        /// Datasheet url (if available)
        /// </summary>
        public string? DatasheetUrl { get; set; }
    }
}
