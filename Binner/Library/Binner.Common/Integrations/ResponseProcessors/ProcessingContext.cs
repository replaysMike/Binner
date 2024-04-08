using Binner.Model;
using System.Collections.Generic;

namespace Binner.Common.Integrations.ResponseProcessors
{
    public class ProcessingContext
    {
        public const int MaxImagesPerSupplier = 5;
        public const int MaxImagesTotal = 10;
        public const string MissingDatasheetCoverName = "datasheetcover.png";

        /// <summary>
        /// Part number being searched
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        public string? PartType { get; set; }

        public string? MountingType { get; set; }

        public string? SupplierPartNumbers { get; set; }

        public int UserId { get; set; }

        /// <summary>
        /// Hashtable of all responses by Api name
        /// </summary>
        public Dictionary<string, Model.Integrations.ApiResponseState> ApiResponses { get; set; } = new();

        /// <summary>
        /// The final results object
        /// </summary>
        public PartResults Results { get; set; } = new();

        /// <summary>
        /// The part in inventory, if it exists
        /// </summary>
        public Part? InventoryPart { get; set; }

        /// <summary>
        /// All part types
        /// </summary>
        public ICollection<PartType> PartTypes { get; set; }

    }
}
