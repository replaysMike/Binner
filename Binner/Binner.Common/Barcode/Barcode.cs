using Binner.Common.Barcode.Symbologies;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace Binner.Common.Barcode
{
    /// <summary>
    /// Generates a barcode image of a specified symbology from a string of data.
    /// </summary>
    [SecuritySafeCritical]
    public partial class Barcode : IDisposable
    {
        /// <summary>
        /// The default resolution of 96 dots per inch.
        /// </summary>
        const float DefaultResolution = 96f;

        /// <summary>
        /// The number of pixels in one point at 96DPI. Since there are 72 points in an inch, this is 96/72.
        /// </summary>
        public const float DotsPerPointAt96Dpi = DefaultResolution / 72;
        private const int DefaultWidth = 300;
        private const int DefaultHeight = 150;
        private bool _disposedValue = false;
        private IBarcode _barcode = new Blank();

        /// <summary>
        /// Gets or sets the raw data to encode.
        /// </summary>
        public string Data { get; set; } = "";

        /// <summary>
        /// Gets the encoded value.
        /// </summary>
        public string EncodedValue { get; private set; } = "";

        /// <summary>
        /// Gets the Country that assigned the Manufacturer Code.
        /// </summary>
        public string CountryAssigningManufacturerCode { get; private set; } = "N/A";

        /// <summary>
        /// Gets or sets the Encoded Type (ex. UPC-A, EAN-13 ... etc)
        /// </summary>
        public BarcodeType BarcodeType { set; get; } = BarcodeType.Unspecified;

        /// <summary>
        /// Gets the Image of the generated barcode.
        /// </summary>
        public Image EncodedImage { get; private set; } = null;

        /// <summary>
        /// Gets or sets the color of the bars. (Default is black)
        /// </summary>
        public Color ForeColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets or sets the background color. (Default is white)
        /// </summary>
        public Color BackColor { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the label font. (Default is Microsoft Sans Serif, 10pt, Bold)
        /// </summary>
        public Font LabelFont { get; set; } = new Font(SystemFonts.Find("Microsoft Sans Serif"), 10 * DotsPerPointAt96Dpi, FontStyle.Bold);

        /// <summary>
        /// Gets or sets the location of the label in relation to the barcode. (BOTTOMCENTER is default)
        /// </summary>
        public LabelPositions LabelPosition { get; set; } = LabelPositions.BottomCenter;

        /// <summary>
        /// Gets or sets the degree in which to rotate/flip the image
        /// </summary>
        public RotateMode RotateFlipType { get; set; } = RotateMode.None;

        /// <summary>
        /// Gets or sets the width of the image to be drawn. (Default is 300 pixels)
        /// </summary>
        public int Width { get; set; } = DefaultWidth;

        /// <summary>
        /// Gets or sets the height of the image to be drawn. (Default is 150 pixels)
        /// </summary>
        public int Height { get; set; } = DefaultHeight;

        /// <summary>
        /// The number of pixels per horizontal inch. Used when creating the Bitmap.
        /// </summary>
        public float HoritontalResolution { get; set; } = DefaultResolution;

        /// <summary>
        /// The number of pixels per vertical inch. Used when creating the Bitmap.
        /// </summary>
        public float VerticalResolution { get; set; } = DefaultResolution;

        /// <summary>
        /// If non-null, sets the width of a bar. <see cref="Width"/> is ignored and calculated automatically.
        /// </summary>
        public int? BarWidth { get; set; }

        /// <summary>
        /// If non-null, <see cref="Height"/> is ignored and set to <see cref="Width"/> divided by this value rounded down.
        /// </summary>
        /// <remarks><para>
        ///   As longer barcodes may be more difficult to align a scanner gun with,
        ///   growing the height based on the width automatically allows the gun to be rotated the
        ///   same amount regardless of how wide the barcode is. A recommended value is 2.
        ///   </para><para>
        ///   This value is applied to <see cref="Height"/> after after <see cref="Width"/> has been
        ///   calculated. So it is safe to use in conjunction with <see cref="BarWidth"/>.
        /// </para></remarks>
        public double? AspectRatio { get; set; }

        /// <summary>
        /// Gets or sets whether a label should be drawn below the image. (Default is false)
        /// </summary>
        public bool IncludeLabel { get; set; }

        /// <summary>
        /// Alternate label to be displayed.  (IncludeLabel must be set to true as well)
        /// </summary>
        public string AlternateLabel { get; set; }

        /// <summary>
        /// Try to standardize the label format. (Valid only for EAN13 and empty AlternateLabel, default is true)
        /// </summary>
        public bool StandardizeLabel { get; set; } = true;

        /// <summary>
        /// Gets or sets the amount of time in milliseconds that it took to encode and draw the barcode.
        /// </summary>
        public double EncodingTime { get; set; }

        /// <summary>
        /// Gets or sets the image format to use when encoding and returning images. (Png is default)
        /// </summary>
        public ImageFormat ImageFormat { get; set; } = ImageFormat.Png;

        /// <summary>
        /// Gets the list of errors encountered.
        /// </summary>
        public List<string> Errors => _barcode.Errors;

        /// <summary>
        /// Gets or sets the alignment of the barcode inside the image. (Not for Postnet or ITF-14)
        /// </summary>
        public AlignmentPositions Alignment { get; set; }

        /// <summary>
        /// Gets a byte array representation of the encoded image.
        /// </summary>
        public byte[] EncodedImageBytes
        {
            get
            {
                if (EncodedImage == null)
                    return null;

                using MemoryStream ms = new MemoryStream();
                switch (ImageFormat)
                {
                    case ImageFormat.Png:
                        EncodedImage.SaveAsPng(ms);
                        break;
                    case ImageFormat.Jpeg:
                        EncodedImage.SaveAsJpeg(ms);
                        break;
                    case ImageFormat.Bmp:
                        EncodedImage.SaveAsBmp(ms);
                        break;
                    case ImageFormat.Gif:
                        EncodedImage.SaveAsGif(ms);
                        break;
                    case ImageFormat.Tga:
                        EncodedImage.SaveAsTga(ms);
                        break;
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Gets the assembly version information.
        /// </summary>
        public static Version Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// Disables EAN13 invalid country code exception.
        /// </summary>
        public bool DisableEAN13CountryException { get; set; } = false;

        /// <summary>
        /// Does not populate the raw data. MUST be done via the RawData property before encoding.
        /// </summary>
        public Barcode()
        {
        }

        /// <summary>
        /// Populates the raw data. No whitespace will be added before or after the barcode.
        /// </summary>
        /// <param name="stringToEncode">String to be encoded.</param>
        public Barcode(string stringToEncode)
        {
            Data = stringToEncode;
        }

        /// <summary>
        /// Populates the raw data. No whitespace will be added before or after the barcode.
        /// </summary>
        /// <param name="stringToEncode">String to be encoded.</param>
        /// <param name="barcodeType">Barcode type to generate</param>
        public Barcode(string stringToEncode, BarcodeType barcodeType)
        {
            Data = stringToEncode;
            BarcodeType = barcodeType;
            Encode<Rgba32>(stringToEncode, barcodeType);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="width">Width of the resulting barcode.(pixels)</param>
        /// <param name="height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public Barcode(string stringToEncode, BarcodeType barcodeType, int width, int height)
        {
            Data = stringToEncode;
            BarcodeType = barcodeType;
            Encode<Rgba32>(stringToEncode, barcodeType, width, height);
        }

        #region General Encode

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="width">Width of the resulting barcode.(pixels)</param>
        /// <param name="height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public Image<TPixel> Encode<TPixel>(string stringToEncode, BarcodeType barcodeType, int width, int height) 
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Width = width;
            Height = height;
            return Encode<TPixel>(stringToEncode, barcodeType);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="foregroundColor">Foreground color</param>
        /// <param name="backgroundColor">Background color</param>
        /// <param name="width">Width of the resulting barcode.(pixels)</param>
        /// <param name="height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public Image<TPixel> Encode<TPixel>(string stringToEncode, BarcodeType barcodeType, Color foregroundColor, Color backgroundColor, int width, int height) where TPixel : unmanaged, IPixel<TPixel>
        {
            Width = width;
            Height = height;
            return Encode<TPixel>(stringToEncode, barcodeType, foregroundColor, backgroundColor);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="DrawColor">Foreground color</param>
        /// <param name="backgroundColor">Background color</param>
        /// <returns>Image representing the barcode.</returns>
        public Image<TPixel> Encode<TPixel>(string stringToEncode, BarcodeType barcodeType, Color foregroundColor, Color backgroundColor) where TPixel : unmanaged, IPixel<TPixel>
        {
            BackColor = backgroundColor;
            ForeColor = foregroundColor;
            return Encode<TPixel>(stringToEncode, barcodeType);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <returns>Image representing the barcode.</returns>
        public Image<TPixel> Encode<TPixel>(string stringToEncode, BarcodeType barcodeType) where TPixel : unmanaged, IPixel<TPixel>
        {
            Data = stringToEncode;
            return Encode<TPixel>(barcodeType);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="barcodeType">Type of encoding to use.</param>
        internal Image<TPixel> Encode<TPixel>(BarcodeType barcodeType) where TPixel : unmanaged, IPixel<TPixel>
        {
            BarcodeType = barcodeType;
            return Encode<TPixel>();
        }

        /// <summary>
        /// Encodes the raw data into a barcode image.
        /// </summary>
        internal Image<TPixel> Encode<TPixel>() where TPixel : unmanaged, IPixel<TPixel>
        {
            _barcode.Errors.Clear();

            var dtStartTime = DateTime.Now;

            EncodedValue = GenerateBarcode(Data);
            Data = _barcode.RawData;

            var encodedImage = GenerateImage<TPixel>();
            encodedImage.Mutate(x => x.Rotate(RotateFlipType));

            EncodedImage = encodedImage;
            EncodingTime = ((DateTime.Now - dtStartTime)).TotalMilliseconds;

            return encodedImage;
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.
        /// </summary>
        /// <returns>
        /// Returns a string containing the binary value of the barcode. 
        /// This also sets the internal values used within the class.
        /// </returns>
        /// <param name="stringToEncode" >Optional raw_data parameter to for quick barcode generation</param>
        public string GenerateBarcode(string stringToEncode)
        {
            if (string.IsNullOrWhiteSpace(stringToEncode))
                throw new BarcodeException("Input data not allowed to be blank.");

            if (BarcodeType == BarcodeType.Unspecified)
                throw new BarcodeException("Symbology type not allowed to be unspecified.");

            Data = stringToEncode;
            EncodedValue = "";
            CountryAssigningManufacturerCode = "N/A";


            switch (BarcodeType)
            {
                case BarcodeType.Ucci12:
                case BarcodeType.Upca:
                    _barcode = new Upca(Data);
                    break;
                case BarcodeType.Ucci13:
                case BarcodeType.Ean13:
                    _barcode = new Ean13(Data, DisableEAN13CountryException);
                    break;
                case BarcodeType.Interleaved2of5Mod10:
                case BarcodeType.Interleaved2of5:
                    _barcode = new Interleaved2of5(Data, BarcodeType);
                    break;
                case BarcodeType.Industrial2of5Mod10:
                case BarcodeType.Industrial2of5:
                case BarcodeType.Standard2of5Mod10:
                case BarcodeType.Standard2of5:
                    _barcode = new Standard2of5(Data, BarcodeType);
                    break;
                case BarcodeType.LogMars:
                case BarcodeType.Code39:
                    _barcode = new Code39(Data);
                    break;
                case BarcodeType.Code39Extended:
                    _barcode = new Code39(Data, true);
                    break;
                case BarcodeType.Code9Mod43:
                    _barcode = new Code39(Data, false, true);
                    break;
                case BarcodeType.Codabar:
                    _barcode = new Codabar(Data);
                    break;
                case BarcodeType.PostNet:
                    _barcode = new Postnet(Data);
                    break;
                case BarcodeType.Isbn:
                case BarcodeType.Bookland:
                    _barcode = new Isbn(Data);
                    break;
                case BarcodeType.Jan13:
                    _barcode = new Jan13(Data);
                    break;
                case BarcodeType.UpcSupplemental2Digit:
                    _barcode = new UpcSupplement2(Data);
                    break;
                //Encode_MSI();
                case BarcodeType.MsiMod10:
                case BarcodeType.Msi2Mod10:
                case BarcodeType.MsiMod11:
                case BarcodeType.MsiMod11Mod10:
                case BarcodeType.ModifiedPlessey:
                    _barcode = new Msi(Data, BarcodeType);
                    break;
                case BarcodeType.UpcSupplemental5Digit:
                    _barcode = new UpcSupplement5(Data);
                    break;
                case BarcodeType.Upce:
                    _barcode = new Upce(Data);
                    break;
                case BarcodeType.Ean8:
                    _barcode = new Ean8(Data);
                    break;
                case BarcodeType.Usd8:
                case BarcodeType.Code11:
                    _barcode = new Code11(Data);
                    break;
                case BarcodeType.Code128:
                    _barcode = new Code128(Data);
                    break;
                case BarcodeType.Code128A:
                    _barcode = new Code128(Data, Code128.TYPES.A);
                    break;
                case BarcodeType.Code128B:
                    _barcode = new Code128(Data, Code128.TYPES.B);
                    break;
                case BarcodeType.Code128C:
                    _barcode = new Code128(Data, Code128.TYPES.C);
                    break;
                case BarcodeType.Itf14:
                    _barcode = new ITF14(Data);
                    break;
                case BarcodeType.Code93:
                    _barcode = new Code93(Data);
                    break;
                case BarcodeType.Telepen:
                    _barcode = new Telepen(Data);
                    break;
                case BarcodeType.Fim:
                    _barcode = new Fim(Data);
                    break;
                case BarcodeType.PharmaCode:
                    _barcode = new Pharmacode(Data);
                    break;

                default: throw new BarcodeException($"Unsupported barcode type specified '{BarcodeType}'.");
            }

            return _barcode.EncodedValue;
        }

        #endregion

        #region Image Functions

        /// <summary>
        /// Create and preconfigures a Bitmap for use by the library. Ensures it is independent from
        /// system DPI, etc.
        /// </summary>
        internal Image<TPixel> CreateBitmap<TPixel>(int width, int height) where TPixel : unmanaged, IPixel<TPixel>
        {
            var image = new Image<TPixel>(width, height);
            image.Metadata.HorizontalResolution = HoritontalResolution;
            image.Metadata.VerticalResolution = VerticalResolution;
            return image;
        }

        /// <summary>
        /// Gets a bitmap representation of the encoded data.
        /// </summary>
        /// <returns>Bitmap of encoded value.</returns>
        private Image<TPixel> GenerateImage<TPixel>() where TPixel : unmanaged, IPixel<TPixel>
        {
            if (string.IsNullOrWhiteSpace(EncodedValue)) throw new BarcodeException("Must call Encode() before generating an image!");
            Image<TPixel> image;

            var dtStartTime = DateTime.Now;

            switch (BarcodeType)
            {
                case BarcodeType.Itf14:
                    {
                        // Automatically calculate the Width if applicable. Quite confusing with this
                        // barcode type, and it seems this method overestimates the minimum width. But
                        // at least it�s deterministic and doesn�t produce too small of a value.
                        if (BarWidth.HasValue)
                        {
                            // Width = (BarWidth * EncodedValue.Length) + bearerwidth + iquietzone
                            // Width = (BarWidth * EncodedValue.Length) + 2*Width/12.05 + 2*Width/20
                            // Width - 2*Width/12.05 - 2*Width/20 = BarWidth * EncodedValue.Length
                            // Width = (BarWidth * EncodedValue.Length)/(1 - 2/12.05 - 2/20)
                            // Width = (BarWidth * EncodedValue.Length)/((241 - 40 - 24.1)/241)
                            // Width = BarWidth * EncodedValue.Length / 176.9 * 241
                            // Rounding error? + 1
                            Width = (int)(241 / 176.9 * EncodedValue.Length * BarWidth.Value + 1);
                        }
                        Height = (int?)(Width / AspectRatio) ?? Height;

                        var ILHeight = Height;
                        if (IncludeLabel)
                        {
                            ILHeight -= LabelFont.LineHeight;
                        }

                        image = CreateBitmap<TPixel>(Width, Height);

                        var bearerwidth = (int)((image.Width) / 12.05);
                        var iquietzone = Convert.ToInt32(image.Width * 0.05);
                        var barWidth = (image.Width - (bearerwidth * 2) - (iquietzone * 2)) / EncodedValue.Length;
                        var shiftAdjustment = ((image.Width - (bearerwidth * 2) - (iquietzone * 2)) % EncodedValue.Length) / 2;

                        if (barWidth <= 0 || iquietzone <= 0)
                            throw new BarcodeException("Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel or quiet zone determined to be less than 1 pixel)");

                        // draw image
                        var pos = 0;

                        // fill background
                        image.Mutate(c => c.Clear(BackColor));

                        // lines are barWidth wide so draw the appropriate color line vertically
                        var pen = Pens.Solid(ForeColor, barWidth);
                        // pen.Alignment = PenAlignment.Right; (no known setting for ImageSharp)

                        while (pos < EncodedValue.Length)
                        {
                            // draw the appropriate color line vertically
                            if (EncodedValue[pos] == '1')
                            {
                                var startPoint = new Point((pos * barWidth) + shiftAdjustment + bearerwidth + iquietzone, 0);
                                var endPoint = new Point((pos * barWidth) + shiftAdjustment + bearerwidth + iquietzone, Height);
                                image.Mutate(c => c.DrawLines(pen, startPoint, endPoint));
                            }

                            pos++;
                        }

                        // bearer bars
                        pen = Pens.Solid(ForeColor, ILHeight / 8f);
                        //pen.Width = (float)ILHeight / 8;
                        //pen.Color = ForeColor;
                        //pen.Alignment = PenAlignment.Center; (no known setting for ImageSharp)

                        image.Mutate(c => c.DrawLines(pen, new Point(0, 0), new Point(image.Width, 0))); // top
                        image.Mutate(c => c.DrawLines(pen, new Point(0, ILHeight), new Point(image.Width, ILHeight))); // bottom
                        image.Mutate(c => c.DrawLines(pen, new Point(0, 0), new Point(0, ILHeight))); // left
                        image.Mutate(c => c.DrawLines(pen, new Point(image.Width, 0), new Point(image.Width, ILHeight))); // right

                        if (IncludeLabel)
                            LabelWriter.LabelITF14<TPixel>(this, image);

                        break;
                    }
                case BarcodeType.Upca:
                    {
                        // Automatically calculate Width if applicable.
                        Width = BarWidth * EncodedValue.Length ?? Width;

                        // Automatically calculate Height if applicable.
                        Height = (int?)(Width / AspectRatio) ?? Height;

                        var ILHeight = Height;
                        var topLabelAdjustment = 0;

                        var shiftAdjustment = 0;
                        var iBarWidth = Width / EncodedValue.Length;

                        // set alignment
                        switch (Alignment)
                        {
                            case AlignmentPositions.Left:
                                shiftAdjustment = 0;
                                break;
                            case AlignmentPositions.Right:
                                shiftAdjustment = (Width % EncodedValue.Length);
                                break;
                            case AlignmentPositions.Center:
                            default:
                                shiftAdjustment = (Width % EncodedValue.Length) / 2;
                                break;
                        }

                        if (IncludeLabel)
                        {
                            if ((AlternateLabel == null || Data.StartsWith(AlternateLabel)) && StandardizeLabel)
                            {
                                // UPCA standardized label
                                var defTxt = Data;
                                var labTxt = defTxt.Substring(0, 1) + "--" + defTxt.Substring(1, 6) + "--" + defTxt.Substring(7);
                                var fontSize = LabelWriter.GetFontsize(this, Width, Height, labTxt) * DotsPerPointAt96Dpi;
                                var labFont = new Font(LabelFont != null ? LabelFont.Family : SystemFonts.Find("Arial"), fontSize, FontStyle.Regular);
                                LabelFont = labFont;
                                ILHeight -= (labFont.LineHeight / 2);
                                iBarWidth = Width / EncodedValue.Length;
                            }
                            else
                            {
                                // Shift drawing down if top label.
                                if ((LabelPosition & (LabelPositions.TopCenter | LabelPositions.TopLeft | LabelPositions.TopRight)) > 0)
                                    topLabelAdjustment = LabelFont.LineHeight;

                                ILHeight -= LabelFont.LineHeight;
                            }
                        }

                        image = CreateBitmap<TPixel>(Width, Height);
                        var iBarWidthModifier = 1;
                        if (iBarWidth <= 0)
                            throw new BarcodeException("Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel)");

                        // draw image
                        var pos = 0;
                        var halfBarWidth = (int)(iBarWidth * 0.5);

                        // clears the image and colors the entire background
                        image.Mutate(c => c.Clear(BackColor));

                        var backPen = Pens.Solid(BackColor, iBarWidth / iBarWidthModifier);
                        var pen = Pens.Solid(ForeColor, iBarWidth / iBarWidthModifier);
                        // lines are fBarWidth wide so draw the appropriate color line vertically
                        while (pos < EncodedValue.Length)
                        {
                            if (EncodedValue[pos] == '1')
                            {
                                var startPoint = new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, topLabelAdjustment);
                                var endPoint = new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, ILHeight + topLabelAdjustment);
                                image.Mutate(c => c.DrawLines(pen, startPoint, endPoint));
                            }

                            pos++;
                        }

                        if (IncludeLabel)
                        {
                            if ((AlternateLabel == null || Data.StartsWith(AlternateLabel)) && StandardizeLabel)
                            {
                                LabelWriter.LabelUPCA<TPixel>(this, image);
                            }
                            else
                            {
                                LabelWriter.LabelGeneric<TPixel>(this, image);
                            }
                        }

                        break;
                    }
                case BarcodeType.Ean13:
                    {
                        // Automatically calculate Width if applicable.
                        Width = BarWidth * EncodedValue.Length ?? Width;

                        // Automatically calculate Height if applicable.
                        Height = (int?)(Width / AspectRatio) ?? Height;

                        var ILHeight = Height;
                        var topLabelAdjustment = 0;

                        var shiftAdjustment = 0;

                        // set alignment
                        switch (Alignment)
                        {
                            case AlignmentPositions.Left:
                                shiftAdjustment = 0;
                                break;
                            case AlignmentPositions.Right:
                                shiftAdjustment = (Width % EncodedValue.Length);
                                break;
                            case AlignmentPositions.Center:
                            default:
                                shiftAdjustment = (Width % EncodedValue.Length) / 2;
                                break;
                        }

                        if (IncludeLabel)
                        {
                            if (((AlternateLabel == null) || Data.StartsWith(AlternateLabel)) && StandardizeLabel)
                            {
                                // EAN13 standardized label
                                var defTxt = Data;
                                var labTxt = defTxt.Substring(0, 1) + "--" + defTxt.Substring(1, 6) + "--" + defTxt.Substring(7);

                                var font = LabelFont;

                                var fontSize = LabelWriter.GetFontsize(this, Width, Height, labTxt) * DotsPerPointAt96Dpi;
                                var labFont = new Font(font != null ? font.Family : SystemFonts.Find("Arial"), fontSize, FontStyle.Regular);
                                LabelFont = labFont;
                                ILHeight -= (labFont.LineHeight / 2);
                            }
                            else
                            {
                                // Shift drawing down if top label.
                                if ((LabelPosition & (LabelPositions.TopCenter | LabelPositions.TopLeft | LabelPositions.TopRight)) > 0)
                                    topLabelAdjustment = LabelFont.LineHeight;

                                ILHeight -= LabelFont.LineHeight;
                            }
                        }

                        image = CreateBitmap<TPixel>(Width, Height);
                        var iBarWidth = Width / EncodedValue.Length;
                        var iBarWidthModifier = 1;
                        if (iBarWidth <= 0)
                            throw new Exception("Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel)");

                        // draw image
                        var pos = 0;
                        var halfBarWidth = (int)(iBarWidth * 0.5);


                        // clears the image and colors the entire background
                        image.Mutate(c => c.Clear(BackColor));

                        var backPen = Pens.Solid(BackColor, iBarWidth / iBarWidthModifier);
                        var pen = Pens.Solid(ForeColor, iBarWidth / iBarWidthModifier);
                        while (pos < EncodedValue.Length)
                        {
                            if (EncodedValue[pos] == '1')
                            {
                                var startPoint = new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, topLabelAdjustment);
                                var endPoint = new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, ILHeight + topLabelAdjustment);
                                image.Mutate(c => c.DrawLines(pen, startPoint, endPoint));
                            }

                            pos++;
                        }

                        if (IncludeLabel)
                        {
                            if (((AlternateLabel == null) || Data.StartsWith(AlternateLabel)) && StandardizeLabel)
                            {
                                LabelWriter.LabelEAN13<TPixel>(this, image);
                            }
                            else
                            {
                                LabelWriter.LabelGeneric<TPixel>(this, image);
                            }
                        }

                        break;
                    }
                default:
                    {
                        // Automatically calculate Width if applicable.
                        Width = BarWidth * EncodedValue.Length ?? Width;

                        // Automatically calculate Height if applicable.
                        Height = (int?)(Width / AspectRatio) ?? Height;

                        var ILHeight = Height;
                        var topLabelAdjustment = 0;

                        if (IncludeLabel)
                        {
                            // Shift drawing down if top label.
                            if ((LabelPosition & (LabelPositions.TopCenter | LabelPositions.TopLeft | LabelPositions.TopRight)) > 0)
                                topLabelAdjustment = LabelFont.LineHeight;

                            ILHeight -= LabelFont.LineHeight;
                        }


                        image = CreateBitmap<TPixel>(Width, Height);
                        var iBarWidth = Width / EncodedValue.Length;
                        var shiftAdjustment = 0;
                        var iBarWidthModifier = 1;

                        if (BarcodeType == BarcodeType.PostNet)
                            iBarWidthModifier = 2;

                        // set alignment
                        switch (Alignment)
                        {
                            case AlignmentPositions.Left:
                                shiftAdjustment = 0;
                                break;
                            case AlignmentPositions.Right:
                                shiftAdjustment = (Width % EncodedValue.Length);
                                break;
                            case AlignmentPositions.Center:
                            default:
                                shiftAdjustment = (Width % EncodedValue.Length) / 2;
                                break;
                        }

                        if (iBarWidth <= 0)
                            throw new BarcodeException("Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel)");

                        // draw image
                        var pos = 0;
                        var halfBarWidth = (int)Math.Round(iBarWidth * 0.5);

                        // clears the image and colors the entire background
                        image.Mutate(c => c.Clear(BackColor));

                        var backPen = Pens.Solid(BackColor, iBarWidth / iBarWidthModifier);
                        var pen = Pens.Solid(ForeColor, iBarWidth / iBarWidthModifier);
                        // lines are fBarWidth wide so draw the appropriate color line vertically
                        while (pos < EncodedValue.Length)
                        {
                            if (BarcodeType == BarcodeType.PostNet)
                            {
                                //draw half bars in postnet
                                if (EncodedValue[pos] == '0')
                                {
                                    var startPoint = new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, ILHeight + topLabelAdjustment);
                                    var endPoint = new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, (ILHeight / 2) + topLabelAdjustment);
                                    image.Mutate(c => c.DrawLines(pen, startPoint, endPoint));
                                }
                                else
                                {
                                    var startPoint = new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, ILHeight + topLabelAdjustment);
                                    var endPoint = new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, topLabelAdjustment);
                                    image.Mutate(c => c.DrawLines(pen, startPoint, endPoint));
                                }
                            }
                            else
                            {
                                if (EncodedValue[pos] == '1')
                                {
                                    var startPoint = new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, topLabelAdjustment);
                                    var endPoint = new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, ILHeight + topLabelAdjustment);
                                    image.Mutate(c => c.DrawLines(pen, startPoint, endPoint));
                                }
                            }
                            pos++;
                        }

                        if (IncludeLabel)
                        {
                            LabelWriter.LabelGeneric<TPixel>(this, image);
                        }

                        break;
                    }
            }

            EncodedImage = image;
            EncodingTime += ((DateTime.Now - dtStartTime)).TotalMilliseconds;

            return image;
        }

        /// <summary>
        /// Gets the bytes that represent the image.
        /// </summary>
        /// <param name="savetype">File type to put the data in before returning the bytes.</param>
        /// <returns>Bytes representing the encoded image.</returns>
        public byte[] GetImageData(SaveTypes savetype)
        {
            byte[] imageData = null;

            try
            {
                if (EncodedImage != null)
                {
                    // Save the image to a memory stream so that we can get a byte array!      
                    using var ms = new MemoryStream();
                    SaveImage(ms, savetype);
                    imageData = ms.ToArray();
                    ms.Flush();
                    ms.Close();
                }
            }
            catch (Exception ex)
            {
                throw new BarcodeException("Could not retrieve image data due to exception! " + ex.Message, ex);
            }
            return imageData;
        }

        /// <summary>
        /// Saves an encoded image to a specified file and type.
        /// </summary>
        /// <param name="filename">Filename to save to.</param>
        /// <param name="fileType">Format to use.</param>
        public void SaveImage(string filename, SaveTypes fileType)
        {
            try
            {
                if (EncodedImage != null)
                {
                    switch (fileType)
                    {
                        case SaveTypes.Bmp:
                            EncodedImage.SaveAsBmp(filename);
                            break;
                        case SaveTypes.Gif:
                            EncodedImage.SaveAsGif(filename);
                            break;
                        case SaveTypes.Jpg:
                            EncodedImage.SaveAsJpeg(filename);
                            break;
                        case SaveTypes.Tiff:
                            EncodedImage.SaveAsTga(filename);
                            break;
                        default:
                        case SaveTypes.Png:
                            EncodedImage.SaveAsPng(filename);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BarcodeException("Could not save image due to exception! " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Saves an encoded image to a specified stream.
        /// </summary>
        /// <param name="stream">Stream to write image to.</param>
        /// <param name="fileType">Format to use.</param>
        public void SaveImage(Stream stream, SaveTypes fileType)
        {
            try
            {
                if (EncodedImage != null)
                {
                    switch (fileType)
                    {
                        case SaveTypes.Bmp:
                            EncodedImage.SaveAsBmp(stream);
                            break;
                        case SaveTypes.Gif:
                            EncodedImage.SaveAsGif(stream);
                            break;
                        case SaveTypes.Jpg:
                            EncodedImage.SaveAsJpeg(stream);
                            break;
                        case SaveTypes.Tiff:
                            EncodedImage.SaveAsTga(stream);
                            break;
                        default:
                        case SaveTypes.Png:
                            EncodedImage.SaveAsPng(stream);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BarcodeException("Could not save image due to exception! " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Returns the size of the EncodedImage in real world coordinates (millimeters or inches).
        /// </summary>
        /// <param name="units">Specify the real world units</param>
        /// <returns></returns>
        public ImageSize GetSizeOfImage(ImageUnits units)
        {
            var width = 0d;
            var height = 0d;
            if (EncodedImage != null && EncodedImage.Width > 0 && EncodedImage.Height > 0)
            {
                var MillimetersPerInch = 25.4d;
                width = EncodedImage.Width / EncodedImage.Metadata.HorizontalResolution;
                height = EncodedImage.Height / EncodedImage.Metadata.VerticalResolution;

                if (units == ImageUnits.Millimeters)
                {
                    width *= MillimetersPerInch;
                    height *= MillimetersPerInch;
                }
            }

            return new ImageSize(width, height, units);
        }

        #endregion

        #region XML Methods

        private SaveData GetSaveData(bool includeImage = true)
        {
            var saveData = new SaveData
            {
                Type = BarcodeType.ToString(),
                RawData = Data,
                EncodedValue = EncodedValue,
                EncodingTime = EncodingTime,
                IncludeLabel = IncludeLabel,
                Forecolor = ForeColor.ToHex(),
                Backcolor = BackColor.ToHex(),
                CountryAssigningManufacturingCode = CountryAssigningManufacturerCode,
                ImageWidth = Width,
                ImageHeight = Height,
                RotateFlipType = RotateFlipType,
                LabelPosition = (int)LabelPosition,
                LabelFont = LabelFont.ToString(),
                ImageFormat = ImageFormat.ToString(),
                Alignment = (int)Alignment
            };

            // get image in base 64
            if (includeImage)
            {
                saveData.Image = Convert.ToBase64String(EncodedImageBytes, Base64FormattingOptions.None);
            }
            return saveData;
        }

        /// <summary>
        /// Get the SaveData as Json
        /// </summary>
        /// <param name="includeImage"></param>
        /// <returns></returns>
        public string ToJson(bool includeImage = true)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(GetSaveData(includeImage));
            return (new UTF8Encoding(false)).GetString(bytes); // no BOM
        }

        /// <summary>
        /// Get the SaveData as Xml
        /// </summary>
        /// <param name="includeImage"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string ToXml(bool includeImage = true)
        {
            if (EncodedValue == "")
                throw new BarcodeException("Could not retrieve XML due to the barcode not being encoded first. Please call Encode first.");
            else
            {
                try
                {
                    var xml = GetSaveData(includeImage);
                    var writer = new XmlSerializer(typeof(SaveData));
                    using var sw = new Utf8StringWriter();
                    writer.Serialize(sw, xml);
                    return sw.ToString();
                }
                catch (Exception ex)
                {
                    throw new BarcodeException("Could not convert to Xml. ", ex);
                }
            }
        }
        
        /// <summary>
        /// Get a SaveData object from a json formatted stream
        /// </summary>
        /// <param name="jsonStream"></param>
        /// <returns></returns>
        public static SaveData FromJson(Stream jsonStream)
        {
            using (jsonStream)
            {
                if (jsonStream is MemoryStream stream)
                {
                    return JsonSerializer.Deserialize<SaveData>(stream.ToArray());
                }
                else
                {
                    using var memoryStream = new MemoryStream();
                    jsonStream.CopyTo(memoryStream);
                    return JsonSerializer.Deserialize<SaveData>(memoryStream.ToArray());
                }

            }
        }

        /// <summary>
        /// Get the SaveData from an xml formatted stream
        /// </summary>
        /// <param name="xmlStream"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SaveData FromXml(Stream xmlStream)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(SaveData));
                using var reader = XmlReader.Create(xmlStream);
                return (SaveData)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw new BarcodeException("Could not parse Xml to SaveData.", ex);
            }
        }

        /// <summary>
        /// Get the image from a SaveData object
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="saveData"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Image<TPixel> GetImageFromSaveData<TPixel>(SaveData saveData) where TPixel : unmanaged, IPixel<TPixel>
        {
            try
            {
                // loading it to memory stream and then to image object
                using var ms = new MemoryStream(Convert.FromBase64String(saveData.Image));
                return Image.Load<TPixel>(ms);
            }
            catch (Exception ex)
            {
                throw new BarcodeException("Could not get image from SaveData.", ex);
            }
        }

        #endregion

        #region Static Encode Methods

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image<TPixel> DoEncode<TPixel>(string stringToEncode, BarcodeType barcodeType) where TPixel : unmanaged, IPixel<TPixel>
        {
            using var b = new Barcode();
            return b.Encode<TPixel>(stringToEncode, barcodeType);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="xml">XML representation of the data and the image of the barcode.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image<TPixel> DoEncode<TPixel>(string stringToEncode, BarcodeType barcodeType, ref string xml) where TPixel : unmanaged, IPixel<TPixel>
        {
            using var b = new Barcode();
            var i = b.Encode<TPixel>(stringToEncode, barcodeType);
            xml = b.ToXml();
            return i;
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="includeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image<TPixel> DoEncode<TPixel>(string stringToEncode, BarcodeType barcodeType, bool includeLabel) where TPixel : unmanaged, IPixel<TPixel>
        {
            using var b = new Barcode();
            b.IncludeLabel = includeLabel;
            return b.Encode<TPixel>(stringToEncode, barcodeType);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="includeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="width">Width of the resulting barcode.(pixels)</param>
        /// <param name="height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image<TPixel> DoEncode<TPixel>(string stringToEncode, BarcodeType barcodeType, bool includeLabel, int width, int height) where TPixel : unmanaged, IPixel<TPixel>
        {
            using var b = new Barcode();
            b.IncludeLabel = includeLabel;
            return b.Encode<TPixel>(stringToEncode, barcodeType, width, height);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="includeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="foregroundColor">Foreground color</param>
        /// <param name="backgroundColor">Background color</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image<TPixel> DoEncode<TPixel>(string stringToEncode, BarcodeType barcodeType, bool includeLabel, Color foregroundColor, Color backgroundColor) where TPixel : unmanaged, IPixel<TPixel>
        {
            using var b = new Barcode();
            b.IncludeLabel = includeLabel;
            return b.Encode<TPixel>(stringToEncode, barcodeType, foregroundColor, backgroundColor);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="includeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="foregroundColor">Foreground color</param>
        /// <param name="backgroundColor">Background color</param>
        /// <param name="width">Width of the resulting barcode.(pixels)</param>
        /// <param name="height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image<TPixel> DoEncode<TPixel>(string stringToEncode, BarcodeType barcodeType, bool includeLabel, Color foregroundColor, Color backgroundColor, int width, int height) where TPixel : unmanaged, IPixel<TPixel>
        {
            using var b = new Barcode();
            b.IncludeLabel = includeLabel;
            return b.Encode<TPixel>(stringToEncode, barcodeType, foregroundColor, backgroundColor, width, height);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="includeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="foregroundColor">Foreground color</param>
        /// <param name="backgroundColor">Background color</param>
        /// <param name="width">Width of the resulting barcode.(pixels)</param>
        /// <param name="height">Height of the resulting barcode.(pixels)</param>
        /// <param name="xml">XML representation of the data and the image of the barcode.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image<TPixel> DoEncode<TPixel>(string stringToEncode, BarcodeType barcodeType, bool includeLabel, Color foregroundColor, Color backgroundColor, int width, int height, ref string xml)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using var b = new Barcode();
            b.IncludeLabel = includeLabel;
            var i = b.Encode<TPixel>(stringToEncode, barcodeType, foregroundColor, backgroundColor, width, height);
            xml = b.ToXml();
            return i;
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
                LabelFont = null;

                EncodedImage?.Dispose();
                EncodedImage = null;

                Data = null;
                EncodedValue = null;
                CountryAssigningManufacturerCode = null;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        #endregion

        #endregion
    }
}