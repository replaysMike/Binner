using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Binner.Common.IO.Printing
{
    public class LabelProperties
    {
        /// <summary>
        /// Label name
        /// </summary>
        public string LabelName { get; set; }
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
        public Size Dimensions { get; set; }
        public LabelProperties(string labelName, int topMargin, int leftMargin, int labelCount, int totalLines, Size dimensions)
        {
            LabelName = labelName;
            TopMargin = topMargin;
            LeftMargin = leftMargin;
            LabelCount = labelCount;
            TotalLines = totalLines;
            Dimensions = dimensions;
        }
    }
}
