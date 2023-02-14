using Binner.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace Binner.Common.IO.Printing
{
    public class LineConfiguration
    {
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
        public Margin Margin { get; set; } = new Margin(0, 0, 0, 0);

        /// <summary>
        /// Font color to use.
        /// Will show on previews and for printers that use color
        /// </summary>
        public string? Color { get; set; }

        public LabelLines Line { get; set; }

        public LineConfiguration()
        {
        }

        public LineConfiguration(LineConfiguration lineConfiguration, LabelLines line)
        {
            Label = lineConfiguration.Label;
            Content = lineConfiguration.Content;
            FontName = lineConfiguration.FontName;
            AutoSize = lineConfiguration.AutoSize;
            UpperCase = lineConfiguration.UpperCase;
            LowerCase = lineConfiguration.LowerCase;
            FontSize = lineConfiguration.FontSize;
            Margin = lineConfiguration.Margin;
            Color = lineConfiguration.Color;
            Line = line;
        }
    }
}
