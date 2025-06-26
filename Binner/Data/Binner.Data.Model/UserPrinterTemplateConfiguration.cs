using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Binner.Model.IO.Printing;

namespace Binner.Data.Model
{
#if INITIALCREATE
    /// <summary>
    /// Stores user defined printer template configurations
    /// </summary>
    public class UserPrinterTemplateConfiguration : IEntity, IOptionalUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserPrinterTemplateConfigurationId { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// The printer configuration
        /// </summary>
        public int UserPrinterConfigurationId { get; set; }

        /// <summary>
        /// Indicates which label line this template is for
        /// </summary>
        public LabelLines Line { get; set; } = LabelLines.Line1;

        /// <summary>
        /// The label number to print on (1-2)
        /// </summary>
        [Range(1, 2)]
        public int Label { get; set; } = 1;

        /// <summary>
        /// Content template, encoded Part property with braces. 
        /// Eg. "{partNumber}", "{date}", "{description}", "{manufacturerPartNumber}" etc
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Font name
        /// </summary>
        public string FontName { get; set; } = "Segoe UI";

        /// <summary>
        /// Auto size the font size if it exceeds the line
        /// </summary>
        public bool AutoSize { get; set; }

        /// <summary>
        /// True to uppercase entire string
        /// </summary>
        public bool UpperCase { get; set; }

        /// <summary>
        /// True to lowercase entire string
        /// </summary>
        public bool LowerCase { get; set; }

        /// <summary>
        /// Font size in points
        /// </summary>
        public int FontSize { get; set; } = 8;

        /// <summary>
        /// True to print as barcode
        /// </summary>
        public bool Barcode { get; set; }

        /// <summary>
        /// Rotation value in degrees
        /// </summary>
        public int Rotate { get; set; } = 0;

        /// <summary>
        /// Label position (default, Center)
        /// </summary>
        public LabelPosition Position { get; set; } = LabelPosition.Center;

        /// <summary>
        /// Label margins
        /// </summary>
        public int MarginTop { get; set; }

        /// <summary>
        /// Label margins
        /// </summary>
        public int MarginBottom { get; set; }

        /// <summary>
        /// Label margins
        /// </summary>
        public int MarginLeft { get; set; }

        /// <summary>
        /// Label margins
        /// </summary>
        public int MarginRight { get; set; }

        /// <summary>
        /// Font color to use.
        /// Will show on previews and for printers that use color
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(UserPrinterConfigurationId))]
        public UserPrinterConfiguration? UserPrinterConfiguration { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
#endif
}
