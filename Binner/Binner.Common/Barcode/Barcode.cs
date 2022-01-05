using Binner.Common.Barcode.Symbologies;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        ///  The number of pixels in one point at 96DPI. Since there are 72 points in an inch, this is 96/72.
        /// </summary>
        public const float DotsPerPointAt96Dpi = DefaultResolution / 72;

        private bool _disposedValue = false;
        private IBarcode _barcode = new Blank();

        /// <summary>
        /// Gets or sets the raw data to encode.
        /// </summary>
        public string RawData { get; set; } = "";

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
        public Font LabelFont { get; set; } = new Font("Microsoft Sans Serif", 10 * DotsPerPointAt96Dpi, FontStyle.Bold, GraphicsUnit.Pixel);

        /// <summary>
        /// Gets or sets the location of the label in relation to the barcode. (BOTTOMCENTER is default)
        /// </summary>
        public LabelPositions LabelPosition { get; set; } = LabelPositions.BottomCenter;

        /// <summary>
        /// Gets or sets the degree in which to rotate/flip the image.(No action is default)
        /// </summary>
        public RotateFlipType RotateFlipType { get; set; } = RotateFlipType.RotateNoneFlipNone;

        /// <summary>
        /// Gets or sets the width of the image to be drawn. (Default is 300 pixels)
        /// </summary>
        public int Width { get; set; } = 300;

        /// <summary>
        /// Gets or sets the height of the image to be drawn. (Default is 150 pixels)
        /// </summary>
        public int Height { get; set; } = 150;

        /// <summary>
        ///   The number of pixels per horizontal inch. Used when creating the Bitmap.
        /// </summary>
        public float HoritontalResolution { get; set; } = DefaultResolution;

        /// <summary>
        ///   The number of pixels per vertical inch. Used when creating the Bitmap.
        /// </summary>
        public float VerticalResolution { get; set; } = DefaultResolution;

        /// <summary>
        ///   If non-null, sets the width of a bar. <see cref="Width"/> is ignored and calculated automatically.
        /// </summary>
        public int? BarWidth { get; set; }

        /// <summary>
        ///   If non-null, <see cref="Height"/> is ignored and set to <see cref="Width"/> divided by this value rounded down.
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
        /// Gets or sets the image format to use when encoding and returning images. (Jpeg is default)
        /// </summary>
        public ImageFormat ImageFormat { get; set; } = ImageFormat.Jpeg;

        /// <summary>
        /// Gets the list of errors encountered.
        /// </summary>
        public List<string> Errors => _barcode.Errors;

        /// <summary>
        /// Gets or sets the alignment of the barcode inside the image. (Not for Postnet or ITF-14)
        /// </summary>
        public AlignmentPositions Alignment { get; set; }

        /// <summary>
        /// Gets a byte array representation of the encoded image. (Used for Crystal Reports)
        /// </summary>
        public byte[] Encoded_Image_Bytes
        {
            get
            {
                if (EncodedImage == null)
                    return null;

                using MemoryStream ms = new MemoryStream();
                EncodedImage.Save(ms, ImageFormat);
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
        /// <param name="data">String to be encoded.</param>
        public Barcode(string data)
        {
            RawData = data;
        }

        /// <summary>
        /// Populates the raw data. No whitespace will be added before or after the barcode.
        /// </summary>
        /// <param name="data">String to be encoded.</param>
        /// <param name="barcodeType">Barcode type to generate</param>
        public Barcode(string data, BarcodeType barcodeType)
        {
            RawData = data;
            BarcodeType = barcodeType;
            GenerateBarcode();
        }

        #region General Encode

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="width">Width of the resulting barcode.(pixels)</param>
        /// <param name="height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public Image Encode(BarcodeType barcodeType, string stringToEncode, int width, int height)
        {
            Width = width;
            Height = height;
            return Encode(barcodeType, stringToEncode);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="foreColor">Foreground color</param>
        /// <param name="backColor">Background color</param>
        /// <param name="width">Width of the resulting barcode.(pixels)</param>
        /// <param name="height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public Image Encode(BarcodeType barcodeType, string stringToEncode, Color foreColor, Color backColor, int width, int height)
        {
            Width = width;
            Height = height;
            return Encode(barcodeType, stringToEncode, foreColor, backColor);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <param name="DrawColor">Foreground color</param>
        /// <param name="backColor">Background color</param>
        /// <returns>Image representing the barcode.</returns>
        public Image Encode(BarcodeType barcodeType, string stringToEncode, Color foreColor, Color backColor)
        {
            BackColor = backColor;
            ForeColor = foreColor;
            return Encode(barcodeType, stringToEncode);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="barcodeType">Type of encoding to use.</param>
        /// <param name="stringToEncode">Raw data to encode.</param>
        /// <returns>Image representing the barcode.</returns>
        public Image Encode(BarcodeType barcodeType, string stringToEncode)
        {
            RawData = stringToEncode;
            return Encode(barcodeType);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="barcodeType">Type of encoding to use.</param>
        internal Image Encode(BarcodeType barcodeType)
        {
            BarcodeType = barcodeType;
            return Encode();
        }

        /// <summary>
        /// Encodes the raw data into a barcode image.
        /// </summary>
        internal Image Encode()
        {
            _barcode.Errors.Clear();

            var dtStartTime = DateTime.Now;

            EncodedValue = GenerateBarcode();
            RawData = _barcode.RawData;

            EncodedImage = Generate_Image();

            EncodedImage.RotateFlip(RotateFlipType);

            EncodingTime = ((TimeSpan)(DateTime.Now - dtStartTime)).TotalMilliseconds;

            return EncodedImage;
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.
        /// </summary>
        /// <returns>
        /// Returns a string containing the binary value of the barcode. 
        /// This also sets the internal values used within the class.
        /// </returns>
        /// <param name="raw_data" >Optional raw_data parameter to for quick barcode generation</param>
        public string GenerateBarcode(string raw_data = "")
        {
            if (raw_data != "")
            {
                RawData = raw_data;
            }

            // make sure there is something to encode
            if (RawData.Trim() == "")
                throw new Exception("EENCODE-1: Input data not allowed to be blank.");

            if (BarcodeType == BarcodeType.Unspecified)
                throw new Exception("EENCODE-2: Symbology type not allowed to be unspecified.");

            EncodedValue = "";
            CountryAssigningManufacturerCode = "N/A";


            switch (BarcodeType)
            {
                case BarcodeType.Ucci12:
                case BarcodeType.Upca: //Encode_UPCA();
                    _barcode = new UPCA(RawData);
                    break;
                case BarcodeType.Ucci13:
                case BarcodeType.Ean13: //Encode_EAN13();
                    _barcode = new EAN13(RawData, DisableEAN13CountryException);
                    break;
                case BarcodeType.Interleaved2of5Mod10:
                case BarcodeType.Interleaved2of5: //Encode_Interleaved2of5();
                    _barcode = new Interleaved2of5(RawData, BarcodeType);
                    break;
                case BarcodeType.Industrial2of5Mod10:
                case BarcodeType.Industrial2of5:
                case BarcodeType.Standard2of5Mod10:
                case BarcodeType.Standard2of5: //Encode_Standard2of5();
                    _barcode = new Standard2of5(RawData, BarcodeType);
                    break;
                case BarcodeType.LogMars:
                case BarcodeType.Code39: //Encode_Code39();
                    _barcode = new Code39(RawData);
                    break;
                case BarcodeType.Code39Extended:
                    _barcode = new Code39(RawData, true);
                    break;
                case BarcodeType.Code9Mod43:
                    _barcode = new Code39(RawData, false, true);
                    break;
                case BarcodeType.Codabar: //Encode_Codabar();
                    _barcode = new Codabar(RawData);
                    break;
                case BarcodeType.PostNet: //Encode_PostNet();
                    _barcode = new Postnet(RawData);
                    break;
                case BarcodeType.Isbn:
                case BarcodeType.Bookland: //Encode_ISBN_Bookland();
                    _barcode = new ISBN(RawData);
                    break;
                case BarcodeType.Jan13: //Encode_JAN13();
                    _barcode = new JAN13(RawData);
                    break;
                case BarcodeType.UpcSupplemental2Digit: //Encode_UPCSupplemental_2();
                    _barcode = new UPCSupplement2(RawData);
                    break;
                case BarcodeType.MsiMod10:
                case BarcodeType.Msi2Mod10:
                case BarcodeType.MsiMod11:
                case BarcodeType.MsiMod11Mod10:
                case BarcodeType.ModifiedPlessey: //Encode_MSI();
                    _barcode = new MSI(RawData, BarcodeType);
                    break;
                case BarcodeType.UpcSupplemental5Digit: //Encode_UPCSupplemental_5();
                    _barcode = new UPCSupplement5(RawData);
                    break;
                case BarcodeType.Upce: //Encode_UPCE();
                    _barcode = new UPCE(RawData);
                    break;
                case BarcodeType.Ean8: //Encode_EAN8();
                    _barcode = new EAN8(RawData);
                    break;
                case BarcodeType.Usd8:
                case BarcodeType.Code11: //Encode_Code11();
                    _barcode = new Code11(RawData);
                    break;
                case BarcodeType.Code128: //Encode_Code128();
                    _barcode = new Code128(RawData);
                    break;
                case BarcodeType.Code128A:
                    _barcode = new Code128(RawData, Code128.TYPES.A);
                    break;
                case BarcodeType.Code128B:
                    _barcode = new Code128(RawData, Code128.TYPES.B);
                    break;
                case BarcodeType.Code128C:
                    _barcode = new Code128(RawData, Code128.TYPES.C);
                    break;
                case BarcodeType.Itf14:
                    _barcode = new ITF14(RawData);
                    break;
                case BarcodeType.Code93:
                    _barcode = new Code93(RawData);
                    break;
                case BarcodeType.Telepen:
                    _barcode = new Telepen(RawData);
                    break;
                case BarcodeType.Fim:
                    _barcode = new FIM(RawData);
                    break;
                case BarcodeType.PharmaCode:
                    _barcode = new Pharmacode(RawData);
                    break;

                default: throw new Exception("EENCODE-2: Unsupported encoding type specified.");
            }

            return _barcode.Encoded_Value;
        }

        #endregion

        #region Image Functions

        /// <summary>
        /// Create and preconfigures a Bitmap for use by the library. Ensures it is independent from
        /// system DPI, etc.
        /// </summary>
        internal Bitmap CreateBitmap(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            bitmap.SetResolution(HoritontalResolution, VerticalResolution);
            return bitmap;
        }

        /// <summary>
        /// Gets a bitmap representation of the encoded data.
        /// </summary>
        /// <returns>Bitmap of encoded value.</returns>
        private Bitmap Generate_Image()
        {
            if (EncodedValue == "") throw new Exception("EGENERATE_IMAGE-1: Must be encoded first.");
            Bitmap bitmap;

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
                            ILHeight -= LabelFont.Height;
                        }

                        bitmap = CreateBitmap(Width, Height);

                        var bearerwidth = (int)((bitmap.Width) / 12.05);
                        var iquietzone = Convert.ToInt32(bitmap.Width * 0.05);
                        var iBarWidth = (bitmap.Width - (bearerwidth * 2) - (iquietzone * 2)) / EncodedValue.Length;
                        var shiftAdjustment = ((bitmap.Width - (bearerwidth * 2) - (iquietzone * 2)) % EncodedValue.Length) / 2;

                        if (iBarWidth <= 0 || iquietzone <= 0)
                            throw new Exception("EGENERATE_IMAGE-3: Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel or quiet zone determined to be less than 1 pixel)");

                        // draw image
                        var pos = 0;

                        using (var g = Graphics.FromImage(bitmap))
                        {
                            // fill background
                            g.Clear(BackColor);

                            // lines are fBarWidth wide so draw the appropriate color line vertically
                            using (var pen = new Pen(ForeColor, iBarWidth))
                            {
                                pen.Alignment = PenAlignment.Right;

                                while (pos < EncodedValue.Length)
                                {
                                    //draw the appropriate color line vertically
                                    if (EncodedValue[pos] == '1')
                                        g.DrawLine(pen, new Point((pos * iBarWidth) + shiftAdjustment + bearerwidth + iquietzone, 0), new Point((pos * iBarWidth) + shiftAdjustment + bearerwidth + iquietzone, Height));

                                    pos++;
                                }

                                // bearer bars
                                pen.Width = (float)ILHeight / 8;
                                pen.Color = ForeColor;
                                pen.Alignment = PenAlignment.Center;
                                g.DrawLine(pen, new Point(0, 0), new Point(bitmap.Width, 0));//top
                                g.DrawLine(pen, new Point(0, ILHeight), new Point(bitmap.Width, ILHeight));//bottom
                                g.DrawLine(pen, new Point(0, 0), new Point(0, ILHeight));//left
                                g.DrawLine(pen, new Point(bitmap.Width, 0), new Point(bitmap.Width, ILHeight));//right
                            }
                        }

                        if (IncludeLabel)
                            Labels.LabelITF14(this, bitmap);

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
                            case AlignmentPositions.LEFT:
                                shiftAdjustment = 0;
                                break;
                            case AlignmentPositions.RIGHT:
                                shiftAdjustment = (Width % EncodedValue.Length);
                                break;
                            case AlignmentPositions.CENTER:
                            default:
                                shiftAdjustment = (Width % EncodedValue.Length) / 2;
                                break;
                        }

                        if (IncludeLabel)
                        {
                            if ((AlternateLabel == null || RawData.StartsWith(AlternateLabel)) && StandardizeLabel)
                            {
                                // UPCA standardized label
                                var defTxt = RawData;
                                var labTxt = defTxt.Substring(0, 1) + "--" + defTxt.Substring(1, 6) + "--" + defTxt.Substring(7);

                                var labFont = new Font(LabelFont != null ? LabelFont.FontFamily.Name : "Arial", Labels.GetFontsize(this, Width, Height, labTxt) * DotsPerPointAt96Dpi, FontStyle.Regular, GraphicsUnit.Pixel);
                                if (LabelFont != null)
                                {
                                    LabelFont.Dispose();
                                }
                                LabelFont = labFont;

                                ILHeight -= (labFont.Height / 2);

                                iBarWidth = Width / EncodedValue.Length;
                            }
                            else
                            {
                                // Shift drawing down if top label.
                                if ((LabelPosition & (LabelPositions.TopCenter | LabelPositions.TopLeft | LabelPositions.TopRight)) > 0)
                                    topLabelAdjustment = LabelFont.Height;

                                ILHeight -= LabelFont.Height;
                            }
                        }

                        bitmap = CreateBitmap(Width, Height);
                        var iBarWidthModifier = 1;
                        if (iBarWidth <= 0)
                            throw new Exception("EGENERATE_IMAGE-2: Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel)");

                        // draw image
                        var pos = 0;
                        var halfBarWidth = (int)(iBarWidth * 0.5);

                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            // clears the image and colors the entire background
                            g.Clear(BackColor);

                            // lines are fBarWidth wide so draw the appropriate color line vertically
                            using (Pen backpen = new Pen(BackColor, iBarWidth / iBarWidthModifier))
                            {
                                using (Pen pen = new Pen(ForeColor, iBarWidth / iBarWidthModifier))
                                {
                                    while (pos < EncodedValue.Length)
                                    {
                                        if (EncodedValue[pos] == '1')
                                        {
                                            g.DrawLine(pen, new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, topLabelAdjustment), new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, ILHeight + topLabelAdjustment));
                                        }

                                        pos++;
                                    }
                                }
                            }
                        }
                        if (IncludeLabel)
                        {
                            if ((AlternateLabel == null || RawData.StartsWith(AlternateLabel)) && StandardizeLabel)
                            {
                                Labels.LabelUPCA(this, bitmap);
                            }
                            else
                            {
                                Labels.LabelGeneric(this, bitmap);
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
                            case AlignmentPositions.LEFT:
                                shiftAdjustment = 0;
                                break;
                            case AlignmentPositions.RIGHT:
                                shiftAdjustment = (Width % EncodedValue.Length);
                                break;
                            case AlignmentPositions.CENTER:
                            default:
                                shiftAdjustment = (Width % EncodedValue.Length) / 2;
                                break;
                        }

                        if (IncludeLabel)
                        {
                            if (((AlternateLabel == null) || RawData.StartsWith(AlternateLabel)) && StandardizeLabel)
                            {
                                // EAN13 standardized label
                                var defTxt = RawData;
                                var labTxt = defTxt.Substring(0, 1) + "--" + defTxt.Substring(1, 6) + "--" + defTxt.Substring(7);

                                var font = LabelFont;
                                var labFont = new Font(font != null ? font.FontFamily.Name : "Arial", Labels.GetFontsize(this, Width, Height, labTxt) * DotsPerPointAt96Dpi, FontStyle.Regular, GraphicsUnit.Pixel);

                                if (font != null)
                                {
                                    LabelFont.Dispose();
                                }

                                LabelFont = labFont;

                                ILHeight -= (labFont.Height / 2);
                            }
                            else
                            {
                                // Shift drawing down if top label.
                                if ((LabelPosition & (LabelPositions.TopCenter | LabelPositions.TopLeft | LabelPositions.TopRight)) > 0)
                                    topLabelAdjustment = LabelFont.Height;

                                ILHeight -= LabelFont.Height;
                            }
                        }

                        bitmap = CreateBitmap(Width, Height);
                        var iBarWidth = Width / EncodedValue.Length;
                        var iBarWidthModifier = 1;
                        if (iBarWidth <= 0)
                            throw new Exception("EGENERATE_IMAGE-2: Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel)");

                        // draw image
                        var pos = 0;
                        var halfBarWidth = (int)(iBarWidth * 0.5);

                        using (var g = Graphics.FromImage(bitmap))
                        {
                            // clears the image and colors the entire background
                            g.Clear(BackColor);

                            // lines are fBarWidth wide so draw the appropriate color line vertically
                            using (var backpen = new Pen(BackColor, iBarWidth / iBarWidthModifier))
                            {
                                using (var pen = new Pen(ForeColor, iBarWidth / iBarWidthModifier))
                                {
                                    while (pos < EncodedValue.Length)
                                    {
                                        if (EncodedValue[pos] == '1')
                                        {
                                            g.DrawLine(pen, new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, topLabelAdjustment), new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, ILHeight + topLabelAdjustment));
                                        }

                                        pos++;
                                    }
                                }
                            }
                        }
                        if (IncludeLabel)
                        {
                            if (((AlternateLabel == null) || RawData.StartsWith(AlternateLabel)) && StandardizeLabel)
                            {
                                Labels.LabelEAN13(this, bitmap);
                            }
                            else
                            {
                                Labels.LabelGeneric(this, bitmap);
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
                                topLabelAdjustment = LabelFont.Height;

                            ILHeight -= LabelFont.Height;
                        }


                        bitmap = CreateBitmap(Width, Height);
                        var iBarWidth = Width / EncodedValue.Length;
                        var shiftAdjustment = 0;
                        var iBarWidthModifier = 1;

                        if (BarcodeType == BarcodeType.PostNet)
                            iBarWidthModifier = 2;

                        // set alignment
                        switch (Alignment)
                        {
                            case AlignmentPositions.LEFT:
                                shiftAdjustment = 0;
                                break;
                            case AlignmentPositions.RIGHT:
                                shiftAdjustment = (Width % EncodedValue.Length);
                                break;
                            case AlignmentPositions.CENTER:
                            default:
                                shiftAdjustment = (Width % EncodedValue.Length) / 2;
                                break;
                        }

                        if (iBarWidth <= 0)
                            throw new Exception("EGENERATE_IMAGE-2: Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel)");

                        // draw image
                        var pos = 0;
                        var halfBarWidth = (int)Math.Round(iBarWidth * 0.5);

                        using (var g = Graphics.FromImage(bitmap))
                        {
                            // clears the image and colors the entire background
                            g.Clear(BackColor);

                            // lines are fBarWidth wide so draw the appropriate color line vertically
                            using (var backpen = new Pen(BackColor, iBarWidth / iBarWidthModifier))
                            {
                                using (var pen = new Pen(ForeColor, iBarWidth / iBarWidthModifier))
                                {
                                    while (pos < EncodedValue.Length)
                                    {
                                        if (BarcodeType == BarcodeType.PostNet)
                                        {
                                            //draw half bars in postnet
                                            if (EncodedValue[pos] == '0')
                                                g.DrawLine(pen, new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, ILHeight + topLabelAdjustment), new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, (ILHeight / 2) + topLabelAdjustment));
                                            else
                                                g.DrawLine(pen, new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, ILHeight + topLabelAdjustment), new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, topLabelAdjustment));
                                        }
                                        else
                                        {
                                            if (EncodedValue[pos] == '1')
                                                g.DrawLine(pen, new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, topLabelAdjustment), new Point(pos * iBarWidth + shiftAdjustment + halfBarWidth, ILHeight + topLabelAdjustment));
                                        }
                                        pos++;
                                    }
                                }
                            }
                        }
                        if (IncludeLabel)
                        {
                            Labels.LabelGeneric(this, bitmap);
                        }

                        break;
                    }
            }

            EncodedImage = bitmap;

            EncodingTime += ((DateTime.Now - dtStartTime)).TotalMilliseconds;

            return bitmap;
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
                throw new Exception("EGETIMAGEDATA-1: Could not retrieve image data. " + ex.Message);
            }  
            return imageData;
        }

        /// <summary>
        /// Saves an encoded image to a specified file and type.
        /// </summary>
        /// <param name="Filename">Filename to save to.</param>
        /// <param name="FileType">Format to use.</param>
        public void SaveImage(string Filename, SaveTypes FileType)
        {
            try
            {
                if (EncodedImage != null)
                {
                    ImageFormat imageformat;
                    switch (FileType)
                    {
                        case SaveTypes.Bmp: imageformat = ImageFormat.Bmp; break;
                        case SaveTypes.Gif: imageformat = ImageFormat.Gif; break;
                        case SaveTypes.Jpg: imageformat = ImageFormat.Jpeg; break;
                        case SaveTypes.Png: imageformat = ImageFormat.Png; break;
                        case SaveTypes.Tiff: imageformat = ImageFormat.Tiff; break;
                        default: imageformat = ImageFormat; break;
                    }
                    ((Bitmap)EncodedImage).Save(Filename, imageformat);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ESAVEIMAGE-1: Could not save image.\n\n=======================\n\n" + ex.Message);
            }
        }

        /// <summary>
        /// Saves an encoded image to a specified stream.
        /// </summary>
        /// <param name="stream">Stream to write image to.</param>
        /// <param name="FileType">Format to use.</param>
        public void SaveImage(Stream stream, SaveTypes FileType)
        {
            try
            {
                if (EncodedImage != null)
                {
                    ImageFormat imageformat;
                    switch (FileType)
                    {
                        case SaveTypes.Bmp: imageformat = ImageFormat.Bmp; break;
                        case SaveTypes.Gif: imageformat = ImageFormat.Gif; break;
                        case SaveTypes.Jpg: imageformat = ImageFormat.Jpeg; break;
                        case SaveTypes.Png: imageformat = ImageFormat.Png; break;
                        case SaveTypes.Tiff: imageformat = ImageFormat.Tiff; break;
                        default: imageformat = ImageFormat; break;
                    }
                    ((Bitmap)EncodedImage).Save(stream, imageformat);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ESAVEIMAGE-2: Could not save image.\n\n=======================\n\n" + ex.Message);
            }
        }

        /// <summary>
        /// Returns the size of the EncodedImage in real world coordinates (millimeters or inches).
        /// </summary>
        /// <param name="Metric">Millimeters if true, otherwise Inches.</param>
        /// <returns></returns>
        public ImageSize GetSizeOfImage(bool Metric)
        {
            var Width = 0d;
            var Height = 0d;
            if (EncodedImage != null && EncodedImage.Width > 0 && EncodedImage.Height > 0)
            {
                var MillimetersPerInch = 25.4d;
                using var g = Graphics.FromImage(EncodedImage);
                Width = EncodedImage.Width / g.DpiX;
                Height = EncodedImage.Height / g.DpiY;

                if (Metric)
                {
                    Width *= MillimetersPerInch;
                    Height *= MillimetersPerInch;
                }
            }

            return new ImageSize(Width, Height, Metric);
        }

        #endregion

        #region XML Methods

        private SaveData GetSaveData(Boolean includeImage = true)
        {
            var saveData = new SaveData();
            saveData.Type = BarcodeType.ToString();
            saveData.RawData = RawData;
            saveData.EncodedValue = EncodedValue;
            saveData.EncodingTime = EncodingTime;
            saveData.IncludeLabel = IncludeLabel;
            saveData.Forecolor = ColorTranslator.ToHtml(ForeColor);
            saveData.Backcolor = ColorTranslator.ToHtml(BackColor);
            saveData.CountryAssigningManufacturingCode = CountryAssigningManufacturerCode;
            saveData.ImageWidth = Width;
            saveData.ImageHeight = Height;
            saveData.RotateFlipType = RotateFlipType;
            saveData.LabelPosition = (int)LabelPosition;
            saveData.LabelFont = LabelFont.ToString();
            saveData.ImageFormat = ImageFormat.ToString();
            saveData.Alignment = (int)Alignment;

            // get image in base 64
            if (includeImage)
            {
                using var ms = new MemoryStream();
                EncodedImage.Save(ms, ImageFormat);
                saveData.Image = Convert.ToBase64String(ms.ToArray(), Base64FormattingOptions.None);
            }
            return saveData;
        }

        public string ToJSON(bool includeImage = true)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(GetSaveData(includeImage));
            return (new UTF8Encoding(false)).GetString(bytes); // no BOM
        }

        public string ToXML(bool includeImage = true)
        {
            if (EncodedValue == "")
                throw new Exception("EGETXML-1: Could not retrieve XML due to the barcode not being encoded first.  Please call Encode first.");
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
                    throw new Exception("EGETXML-2: " + ex.Message);
                }
            }
        }

        public static SaveData FromJSON(Stream jsonStream)
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

        public static SaveData FromXML(Stream xmlStream)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(SaveData));
                using var reader = XmlReader.Create(xmlStream);
                return (SaveData)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw new Exception("EGETIMAGEFROMXML-1: " + ex.Message);
            }
        }

        public static Image GetImageFromSaveData(SaveData saveData)
        {
            try
            {
                // loading it to memory stream and then to image object
                using var ms = new MemoryStream(Convert.FromBase64String(saveData.Image));
                return Image.FromStream(ms);
            }
            catch (Exception ex)
            {
                throw new Exception("EGETIMAGEFROMXML-1: " + ex.Message);
            }
        }

        #endregion

        #region Static Encode Methods

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarcodeType iType, string Data)
        {
            using var b = new Barcode();
            return b.Encode(iType, Data);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <param name="XML">XML representation of the data and the image of the barcode.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarcodeType iType, string Data, ref string XML)
        {
            using var b = new Barcode();
            var i = b.Encode(iType, Data);
            XML = b.ToXML();
            return i;
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarcodeType iType, string Data, bool IncludeLabel)
        {
            using var b = new Barcode();
            b.IncludeLabel = IncludeLabel;
            return b.Encode(iType, Data);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="data">Raw data to encode.</param>
        /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="Width">Width of the resulting barcode.(pixels)</param>
        /// <param name="Height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarcodeType iType, string Data, bool IncludeLabel, int Width, int Height)
        {
            using var b = new Barcode();
            b.IncludeLabel = IncludeLabel;
            return b.Encode(iType, Data, Width, Height);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="DrawColor">Foreground color</param>
        /// <param name="BackColor">Background color</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarcodeType iType, string Data, bool IncludeLabel, Color DrawColor, Color BackColor)
        {
            using var b = new Barcode();
            b.IncludeLabel = IncludeLabel;
            return b.Encode(iType, Data, DrawColor, BackColor);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="DrawColor">Foreground color</param>
        /// <param name="BackColor">Background color</param>
        /// <param name="Width">Width of the resulting barcode.(pixels)</param>
        /// <param name="Height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarcodeType iType, string Data, bool IncludeLabel, Color DrawColor, Color BackColor, int Width, int Height)
        {
            using var b = new Barcode();
            b.IncludeLabel = IncludeLabel;
            return b.Encode(iType, Data, DrawColor, BackColor, Width, Height);
        }

        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="DrawColor">Foreground color</param>
        /// <param name="BackColor">Background color</param>
        /// <param name="Width">Width of the resulting barcode.(pixels)</param>
        /// <param name="Height">Height of the resulting barcode.(pixels)</param>
        /// <param name="XML">XML representation of the data and the image of the barcode.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarcodeType iType, string Data, bool IncludeLabel, Color DrawColor, Color BackColor, int Width, int Height, ref string XML)
        {
            using var b = new Barcode();
            b.IncludeLabel = IncludeLabel;
            var i = b.Encode(iType, Data, DrawColor, BackColor, Width, Height);
            XML = b.ToXML();
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
                LabelFont?.Dispose();
                LabelFont = null;

                EncodedImage?.Dispose();
                EncodedImage = null;

                RawData = null;
                EncodedValue = null;
                CountryAssigningManufacturerCode = null;
                ImageFormat = null;
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