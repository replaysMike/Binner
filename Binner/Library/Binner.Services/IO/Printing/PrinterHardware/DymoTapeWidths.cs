namespace Binner.Services.IO.Printing.PrinterHardware
{
    public static class DymoTapeWidths
    {
        public static IList<DymoTapeWidth> Values => new List<DymoTapeWidth>
        {
            new DymoTapeWidth("6 mm (1/4\")", 6f, 0, 2f, 0f),
            new DymoTapeWidth("9 mm (3/8\")", 9f, 0, 2f, 0f),
            new DymoTapeWidth("12 mm (1/2\")", 12f, 0, 2f, 0f),
            new DymoTapeWidth("19 mm (3/4\")", 19f, 0, 2f, 0f),
            new DymoTapeWidth("24 mm (1\")", 24f, 0, 2f, 0f),
        };
    }

    public class DymoTapeWidth
    {
        public string ModelName { get; set; }
        public float TapeWidthMm { get; set; }
        public float LeftMarginMm { get; set; }
        public float TopMarginMm { get; set; }
        public float BottomMarginMm { get; set; }

        public DymoTapeWidth(string modelName, float tapeWidthMm, float leftMarginMm, float topMarginMm, float bottomMarginMm)
        {
            ModelName = modelName;
            TapeWidthMm = tapeWidthMm;
            LeftMarginMm = leftMarginMm;
            TopMarginMm = topMarginMm;
            BottomMarginMm = bottomMarginMm;
        }
    }
}
