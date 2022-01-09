namespace Binner.Common.IO.Printing
{
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
}
