using SixLabors.Fonts;
using System;
using System.IO;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Manages loading of fonts
    /// </summary>
    public sealed class FontManager
    {
        private const string FontsPath = "./Fonts";
        private static Lazy<FontCollection> _fontCollection = new Lazy<FontCollection>(() => new FontCollection());

        /// <summary>
        /// Get the installed fonts
        /// </summary>
        public FontCollection InstalledFonts => _fontCollection.Value;

        /// <summary>
        /// Manages loading of fonts
        /// </summary>
        public FontManager() => LoadFonts();

        private static FontCollection LoadFonts()
        {
            // Load fonts
            if (!_fontCollection.IsValueCreated)
            {
                var fontFiles = new FontScanner().GetFonts(FontsPath);
                foreach (var fontFile in fontFiles)
                {
                    if (File.Exists(fontFile))
                    {
                        try
                        {
                            _fontCollection.Value.Add(fontFile);
                        }
                        catch (Exception)
                        {
                            // can't use font
                        }
                    }
                }
            }

            return _fontCollection.Value;
        }
    }
}
