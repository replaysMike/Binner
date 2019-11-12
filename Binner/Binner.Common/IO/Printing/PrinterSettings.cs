using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;

namespace Binner.Common.IO.Printing
{
    public class PrinterSettings : IPrinterSettings
    {
        public string PrinterName { get; set; } = "Dymo LabelWriter 450";
        
        public string LabelName { get; set; } = "30346"; // LW 1/2" x 1 7/8"
        
        public LabelSource LabelSource { get; set; } = LabelSource.Default;

        /// <summary>
        /// Template for printing part labels
        /// </summary>
        public PartLabelTemplate PartLabelTemplate { get; set; } = new PartLabelTemplate();
    }

    public class PartLabelTemplate
    {
        public LineConfiguration Line1 { get; set; } = new LineConfiguration();
        public LineConfiguration Line2 { get; set; } = new LineConfiguration();
        public LineConfiguration Line3 { get; set; } = new LineConfiguration();
        public LineConfiguration Line4 { get; set; } = new LineConfiguration();
        /// <summary>
        /// Optional Location identifier
        /// </summary>
        public LineConfiguration Identifier { get; set; } = new LineConfiguration();
    }

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
        public string Content { get; set; }

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
    }

    /// <summary>
    /// Margin class
    /// Don't use Printer.Margins class, it defaults values to 100
    /// </summary>
    public class Margin
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }
        public Margin() { }
        public Margin(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }

    public enum LabelPosition
    {
        Left,
        Right,
        Center
    }
}
