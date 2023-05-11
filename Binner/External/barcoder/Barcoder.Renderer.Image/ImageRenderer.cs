using System;
using System.IO;
using System.Numerics;
using Barcoder.Renderer.Image.Internal;
using Barcoder.Renderers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Barcoder.Renderer.Image
{
    public sealed class ImageRenderer : IRenderer
    {
        private readonly ImageRendererOptions _options;
        private readonly Lazy<IImageEncoder> _imageEncoder;

        public ImageRenderer(ImageRendererOptions options = null)
        {
            options = options ?? new ImageRendererOptions();

            if (options.PixelSize <= 0) throw new ArgumentOutOfRangeException(nameof(options.PixelSize), "Value must be larger than zero");
            if (options.BarHeightFor1DBarcode <= 0) throw new ArgumentOutOfRangeException(nameof(options.BarHeightFor1DBarcode), "Value must be larger than zero");
            if (options.JpegQuality < 0 || options.JpegQuality > 100) throw new ArgumentOutOfRangeException(nameof(options.JpegQuality), "Value must be a value between 0 and 100");

            _options = options;
            _imageEncoder = new Lazy<IImageEncoder>(GetImageEncoder);
        }
        
        private IImageEncoder GetImageEncoder()
        {
            switch (_options.ImageFormat)
            {
            case ImageFormat.Bmp: return new BmpEncoder();
            case ImageFormat.Gif: return new GifEncoder();
            case ImageFormat.Jpeg: return new JpegEncoder { Quality = _options.JpegQuality };
            case ImageFormat.Png: return new PngEncoder();
            default:
                throw new NotSupportedException($"Requested image format {_options.ImageFormat} is not supported");
            }
        }

        public void Render(IBarcode barcode, Stream outputStream)
        {
            barcode = barcode ?? throw new ArgumentNullException(nameof(barcode));
            outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
            if (barcode.Bounds.Y == 1)
            {
                using (var image = Render1D(barcode))
                {
                    image.Save(outputStream, _imageEncoder.Value);
                }
            }
            else if (barcode.Bounds.Y > 1)
            {
                using (var image = Render2D(barcode))
                {
                    image.Save(outputStream, _imageEncoder.Value);
                }
            }
            else
                throw new NotSupportedException($"Y value of {barcode.Bounds.Y} is invalid");
        }

        public Image<L8> Render(IBarcode barcode)
        {
            barcode = barcode ?? throw new ArgumentNullException(nameof(barcode));
            if (barcode.Bounds.Y == 1)
                return Render1D(barcode);
            else if (barcode.Bounds.Y > 1)
                return Render2D(barcode);
            else
                throw new NotSupportedException($"Y value of {barcode.Bounds.Y} is invalid");
        }

        private Image<L8> Render1D(IBarcode barcode)
        {
            int margin = _options.CustomMargin ?? barcode.Margin;
            int width = (barcode.Bounds.X + 2 * margin) * _options.PixelSize;
            int height = (_options.BarHeightFor1DBarcode + 2 * margin) * _options.PixelSize;

            var image = new Image<L8>(width, height);
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.White);
                for (var x = 0; x < barcode.Bounds.X; x++)
                {
                    if (!barcode.At(x, 0))
                        continue;
                    ctx.FillPolygon(
                        Color.Black,
                        new Vector2((margin + x) * _options.PixelSize, margin * _options.PixelSize),
                        new Vector2((margin + x + 1) * _options.PixelSize, margin * _options.PixelSize),
                        new Vector2((margin + x + 1) * _options.PixelSize, (_options.BarHeightFor1DBarcode + margin) * _options.PixelSize),
                        new Vector2((margin + x) * _options.PixelSize, (_options.BarHeightFor1DBarcode + margin) * _options.PixelSize));
                }
            });

            if (_options.IncludeEanContentAsText && barcode.IsEanBarcode())
                EanContentRenderer.Render(image, barcode, fontFamily: _options.EanFontFamily, scale: _options.PixelSize);

            return image;
        }

        private Image<L8> Render2D(IBarcode barcode)
        {
            int margin = _options.CustomMargin ?? barcode.Margin;
            int width = (barcode.Bounds.X + 2 * margin) * _options.PixelSize;
            int height = (barcode.Bounds.Y + 2 * margin) * _options.PixelSize;

            var image = new Image<L8>(width, height);
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.White);
                for (var y = 0; y < barcode.Bounds.Y; y++)
                {
                    for (var x = 0; x < barcode.Bounds.X; x++)
                    {
                        if (!barcode.At(x, y)) continue;
                        ctx.FillPolygon(
                            Color.Black,
                            new Vector2((margin + x) * _options.PixelSize, (margin + y) * _options.PixelSize),
                            new Vector2((margin + x + 1) * _options.PixelSize, (margin + y) * _options.PixelSize),
                            new Vector2((margin + x + 1) * _options.PixelSize, (margin + y + 1) * _options.PixelSize),
                            new Vector2((margin + x) * _options.PixelSize, (margin + y + 1) * _options.PixelSize));
                    }
                }
            });

            return image;
        }
    }
}
