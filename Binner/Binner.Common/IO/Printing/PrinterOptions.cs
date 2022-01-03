namespace Binner.Common.IO.Printing
{
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
