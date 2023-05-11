namespace Barcoder.Renderer.Image
{
    public sealed class ImageRendererOptions
    {
        public int PixelSize { get; set; } = 10;

        public int BarHeightFor1DBarcode { get; set; } = 40; 
        
        public ImageFormat ImageFormat { get; set; } = ImageFormat.Png;
        
        public int JpegQuality { get; set; } = 75;
        
        public bool IncludeEanContentAsText { get; set; } = false;
        
        public string EanFontFamily { get; set; } = "Arial";

        public int? CustomMargin { get; set; } = null;
    }
}
