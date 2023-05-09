namespace Binner.Model.Requests
{
    public class CustomLabelRequest : IImagesToken
    {
        public ICollection<LabelBox> Boxes { get; set; } = new List<LabelBox>();

        public PrinterLabel Label { get; set; } = new();

        /// <summary>
        /// True to generate image only
        /// </summary>
        public bool GenerateImageOnly { get; set; }

        /// <summary>
        /// The image token to validate the insecure request with
        /// </summary>
        public string? Token { get; set; }
    }

    public class PrinterLabel
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public int? TemplateId { get; set; }
        public string? Name { get; set; }
        public int Dpi { get; set; }
        public string? Margin { get; set; }
        public bool ShowBoundaries { get; set; }
    }

    public class LabelBox
    {
        public string? Name { get; set; }
        public bool AcceptsValue { get; set; }
        public bool DisplayValue { get; set; }
        public string? Id { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Resize { get; set; }
        public LabelProperty Properties { get; set; } = new();
    }

    public class LabelProperty
    {
        public string? Name { get; set; }
        public string? Font { get; set; }
        public LabelAlign Align { get; set; }
        public FontSizes FontSize { get; set; }
        public FontWeights FontWeight { get; set; }
        public LabelColors Color { get; set; }
        public Rotations Rotate { get; set; }

        /// <summary>
        /// Value is used for barcodes and custom text fields
        /// </summary>
        public string? Value { get; set; }
    }

    public enum Rotations
    {
        Zero = 0,
        FourtyFive,
        Ninety,
        OneThirtyFive,
        OneEighty,
        TwoTwentyFive,
        TwoSeventy,
        ThreeFifteen
    }

    public enum LabelAlign
    {
        Center = 0,
        Left,
        Right
    }

    public enum FontWeights
    {
        Normal = 0,
        Bold
    }

    public enum FontSizes
    {
        Tiny = 0,
        Small,
        Normal,
        Medium,
        Large,
        ExtraLarge,
        VeryLarge,
    }

    public enum LabelColors
    {
        Black = 0,
        Blue,
        Gray,
        Green,
        Orange,
        Purple,
        Red,
        Yellow
    }
}
