using Binner.Common.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Label printer
    /// </summary>
    public interface ILabelPrinter
    {
        /// <summary>
        /// Printer settings
        /// </summary>
        IPrinterSettings PrinterSettings { get; set; }

        /// <summary>
        /// Print a label
        /// </summary>
        /// <param name="content"></param>
        Image PrintLabel(LabelContent content);

        /// <summary>
        /// Print a label
        /// </summary>
        /// <param name="content"></param>
        /// <param name="options">Printer options overrides</param>
        /// <returns></returns>
        Image PrintLabel(LabelContent content, PrinterOptions options);

        /// <summary>
        /// Print a custom label
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Image PrintLabel(ICollection<LineConfiguration> lines, PrinterOptions options);
    }

    public class LabelContent
    {
        /// <summary>
        /// A part to use as the template source. If not provided, strings will be literals.
        /// </summary>
        public Part Part { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
        public string Identifier { get; set; }
    }

    public class PrinterOptions
    {
        /// <summary>
        /// Override the label source to use
        /// </summary>
        public LabelSource? LabelSource { get; set; }

        /// <summary>
        /// Label model name
        /// </summary>
        public string LabelName { get; set; }

        /// <summary>
        /// True to generate the label image only
        /// </summary>
        public bool GenerateImageOnly { get; set; }

        /// <summary>
        /// True to show diagnostic framing
        /// </summary>
        public bool ShowDiagnostic { get; set; }

        /// <summary>
        /// No options to override
        /// </summary>
        public static PrinterOptions None => new PrinterOptions();

        /// <summary>
        /// Generate the label image only, do not print
        /// </summary>
        public static PrinterOptions ImageOnly => new PrinterOptions(true);

        public PrinterOptions() { }

        public PrinterOptions(LabelSource labelSource)
        {
            LabelSource = labelSource;
        }

        public PrinterOptions(string labelName)
        {
            LabelName = labelName;
        }

        public PrinterOptions(bool generateImageOnly)
        {
            GenerateImageOnly = generateImageOnly;
        }

        public PrinterOptions(LabelSource labelSource, bool generateImageOnly)
        {
            LabelSource = labelSource;
            GenerateImageOnly = generateImageOnly;
        }

        public PrinterOptions(LabelSource labelSource, string labelName, bool generateImageOnly)
        {
            LabelSource = labelSource;
            LabelName = labelName;
            GenerateImageOnly = generateImageOnly;
        }

        public PrinterOptions(LabelSource labelSource, string labelName, bool generateImageOnly, bool showDiagnostic)
        {
            LabelSource = labelSource;
            LabelName = labelName;
            GenerateImageOnly = generateImageOnly;
            ShowDiagnostic = showDiagnostic;
        }
    }
}
