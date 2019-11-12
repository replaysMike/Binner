using Binner.Common.IO.Printing;
using System.Collections.Generic;

namespace Binner.Common.Models
{
    public class PrintLabelRequest
    {
        /// <summary>
        /// True to show diagnostic framing
        /// </summary>
        public bool ShowDiagnostic { get; set; }

        /// <summary>
        /// Label model number of label being printed
        /// </summary>
        public string LabelName { get; set; }

        /// <summary>
        /// Printer paper source to use
        /// </summary>
        public LabelSource LabelSource { get; set; } = LabelSource.Default;

        /// <summary>
        /// Lines to print
        /// </summary>
        public ICollection<LineConfiguration> Lines { get; set; } = new List<LineConfiguration>();

        /// <summary>
        /// True to generate image only
        /// </summary>
        public bool GenerateImageOnly { get; set; }
    }
}
