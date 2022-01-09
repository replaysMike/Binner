using SixLabors.ImageSharp;

namespace Binner.Common.IO.Printing
{
    public class LabelDefinition
    {
        /// <summary>
        /// Label name
        /// </summary>
        public string LabelName => MediaSize?.Name;

        /// <summary>
        /// The media size information of the label
        /// </summary>
        public MediaSize MediaSize { get; set; }

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
        public float Dpi { get; set; } = 300f;

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
        /// <param name="dpi">The printing resolution in dots per inch</param>
        /// <param name="invertLabelDimensions">True to invert the label dimensions (invert width and height)</param>
        public LabelDefinition(MediaSize mediaSize, int topMargin, int leftMargin, int labelCount, int totalLines, float dpi, bool invertLabelDimensions = true)
        {
            MediaSize = mediaSize;
            TopMargin = topMargin;
            LeftMargin = leftMargin;
            LabelCount = labelCount;
            TotalLines = totalLines;
            Dpi = dpi;
            InvertLabelDimensions = invertLabelDimensions;
            UpdateDimensions();
        }

        /// <summary>
        /// Calculate the label dimensions based on properties
        /// </summary>
        internal void UpdateDimensions()
        {
            // convert points to inches, 72 points per inch
            var widthInches = MediaSize.Width / 72f;
            var heightInches = MediaSize.Height / 72f;

            // convert inches to pixels
            var width = widthInches * Dpi;
            var height = heightInches * Dpi;

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
