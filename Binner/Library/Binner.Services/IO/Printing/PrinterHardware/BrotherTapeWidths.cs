namespace Binner.Services.IO.Printing.PrinterHardware
{
    public static class BrotherTapeWidths
    {
        public static IList<BrotherTapeWidth> Values => new List<BrotherTapeWidth>
        {
            new BrotherTapeWidth("0.13\"", 3.5f, 0, 2f),
            new BrotherTapeWidth("0.23\"", 6f, 0, 2f),
            new BrotherTapeWidth("0.35\"", 9f, 0, 2f),
            new BrotherTapeWidth("0.47\"", 12f, 0, 2f),
            new BrotherTapeWidth("0.70\"", 18f, 0, 2f),
            new BrotherTapeWidth("0.94\"", 24f, 0, 2f),
            new BrotherTapeWidth("1.5\"", 36f, 0, 2f),
        };
    }

    public class BrotherTapeWidth
    {
        public string ModelName { get; set; }
        public float TapeWidthMm { get; set; }
        public float LeftMarginMm { get; set; }
        public float TopMarginMm { get; set; }

        public BrotherTapeWidth(string modelName, float tapeWidthMm, float leftMarginMm, float topMarginMm)
        {
            ModelName = modelName;
            TapeWidthMm = tapeWidthMm;
            LeftMarginMm = leftMarginMm;
            TopMarginMm = topMarginMm;
        }
    }
}
