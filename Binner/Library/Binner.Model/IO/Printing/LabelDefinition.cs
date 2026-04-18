using SixLabors.ImageSharp;

namespace Binner.Model.IO.Printing
{
    public class LabelDefinition
    {
        /// <summary>
        /// Label name
        /// </summary>
        public string LabelName => MediaSize.Name;

        /// <summary>
        /// The media size information of the label
        /// </summary>
        public MediaSize MediaSize { get; set; } = new();

        /// <summary>
        /// Top margin of label
        /// </summary>
        public int TopMargin { get; set; }

        /// <summary>
        /// Left margin of label
        /// </summary>
        public int LeftMargin { get; set; }

        /// <summary>
        /// Number of lines to shift before printing first line
        /// </summary>
        public int LabelCount { get; set; }

        /// <summary>
        /// Total number of lines supported
        /// </summary>
        public int TotalLines { get; set; }

        /// <summary>
        /// Pixel dimensions of label
        /// </summary>
        public Size ImageDimensions { get; private set; }

        /// <summary>
        /// The print dimensions in inches
        /// </summary>
        public SizeF PrintDimensions { get; private set; }

        /// <summary>
        /// True to invert the label dimensions (invert width and height)
        /// </summary>
        public bool InvertLabelDimensions { get; set; } = true;

        /// <summary>
        /// The printing resolution in dots per inch
        /// </summary>
        public float HorizontalDpi { get; set; } = 600f;

        /// <summary>
        /// The printing resolution in dots per inch
        /// </summary>
        public float Dpi { get; set; } = 300f;

        /// <summary>
        /// For tape style printers, the tape width
        /// </summary>
        public float? TapeWidthMm { get; set; }

        /// <summary>
        /// Tape label length in mm
        /// </summary>
        public float TapeLengthMm { get; set; }

        /// <summary>
        /// Tape label left margin in millimeters
        /// </summary>
        public float TapeLeftMarginMm { get; set; }

        /// <summary>
        /// Tape label top margin in millimeters
        /// </summary>
        public float TapeTopMarginMm { get; set; }

        /// <summary>
        /// Tape label bottom margin in millimeters
        /// </summary>
        public float TapeBottomMarginMm { get; set; }

        /// <summary>
        /// Create label properties definition
        /// </summary>
        public LabelDefinition() { }

        /// <summary>
        /// Create label properties definition
        /// </summary>
        /// <param name="mediaSize">The media size information of the label</param>
        /// <param name="topMargin">Top margin of label</param>
        /// <param name="leftMargin">Left margin of label</param>
        /// <param name="labelCount">Number of lines to shift before printing first line</param>
        /// <param name="totalLines">Total number of lines supported</param>
        /// <param name="horizontalDpi">The printing resolution in dots per inch</param>
        /// <param name="verticalDpi">The printing resolution in dots per inch</param>
        /// <param name="invertLabelDimensions">True to invert the label dimensions (invert width and height)</param>
        public LabelDefinition(MediaSize mediaSize, int topMargin, int leftMargin, int labelCount, int totalLines, float horizontalDpi, float verticalDpi, bool invertLabelDimensions = true)
        {
            MediaSize = mediaSize;
            TopMargin = topMargin;
            LeftMargin = leftMargin;
            LabelCount = labelCount;
            TotalLines = totalLines;
            HorizontalDpi = horizontalDpi;
            Dpi = verticalDpi;
            InvertLabelDimensions = invertLabelDimensions;
            UpdateDimensions();
        }

        /// <summary>
        /// Create label properties definition
        /// </summary>
        /// <param name="mediaSize">The media size information of the label</param>
        /// <param name="tapeWidthMm">The tape width in mm</param>
        /// <param name="tapeLengthMm">The tape length in mm</param>
        /// <param name="tapeBottomMarginMm"></param>
        /// <param name="tapeLeftMarginMm"></param>
        /// <param name="tapeTopMarginMm"></param>
        public LabelDefinition(MediaSize mediaSize, float tapeWidthMm, float tapeLengthMm, float tapeLeftMarginMm, float tapeTopMarginMm, float tapeBottomMarginMm)
        {
            MediaSize = mediaSize;
            TapeWidthMm = tapeWidthMm;
            TapeLengthMm = tapeLengthMm;
            TapeLeftMarginMm = tapeLeftMarginMm;
            TapeTopMarginMm = tapeTopMarginMm;
            TapeBottomMarginMm = tapeBottomMarginMm;
            UpdateDimensions();
        }

        /// <summary>
        /// Calculate the label dimensions based on properties
        /// </summary>
        public void UpdateDimensions()
        {
            // convert points to inches, 72 points per inch
            var widthInches = MediaSize.Width / 72f;
            var heightInches = MediaSize.Height / 72f;

            // convert inches to pixels
            var width = widthInches * Dpi;
            var height = heightInches * HorizontalDpi;

            if (InvertLabelDimensions)
            {
                PrintDimensions = new SizeF(heightInches, widthInches);
                ImageDimensions = new Size((int)height, (int)width);
            }
            else
            {
                PrintDimensions = new SizeF(widthInches, heightInches);
                ImageDimensions = new Size((int)width, (int)height);
            }

        }

    }
}
