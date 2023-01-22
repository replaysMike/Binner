using System;
using System.ComponentModel.DataAnnotations;

namespace Binner.Common.Models
{
    public class DatasheetBasic
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int DatasheetId { get; set; }

        /// <summary>
        /// Unique resource Id.
        /// Indicates the file name & path on the resource server.
        /// </summary>
        public Guid ResourceId { get; set; }

        /// <summary>
        /// Pdf document type
        /// </summary>
        public PdfDocumentTypes DocumentType { get; set; }

        /// <summary>
        /// The resource server Url of the datasheet and it's associated content
        /// </summary>
        public string ResourceSourceUrl { get; set; } = null!;

        /// <summary>
        /// The resource path of the datasheet and it's associated content
        /// </summary>
        public string ResourcePath { get; set; } = null!;

        /// <summary>
        /// The short one-line description of the datasheet
        /// </summary>
        [MaxLength(255)]
        public string? ShortDescription { get; set; }

        /// <summary>
        /// Title indicated by the original PDF
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Original import filename
        /// </summary>
        public string? OriginalUrl { get; set; }

        /// <summary>
        /// The base part number.
        /// For example: LMx58-N
        /// Other part numbers may be LM158-N, LM258-N, LM358-N etc.
        /// </summary>
        public string? BasePartNumber { get; set; } = null!;

        /// <summary>
        /// Product url if applicable
        /// </summary>
        public string? ProductUrl { get; set; }

        /// <summary>
        /// Manufacturer of datasheet
        /// </summary>
        public string? ManufacturerName { get; set; }

        /// <summary>
        /// The number of images available for the datasheet
        /// </summary>
        public int ImageCount { get; set; }

        /// <summary>
        /// The number of pages in the datasheet
        /// </summary>
        public int PageCount { get; set; }
    }
}
