using System.Collections.Generic;
using System.IO;

namespace Binner.Common.IO
{
    /// <summary>
    /// Scans for fonts
    /// </summary>
    public class FontScanner
    {
        /// <summary>
        /// List the fonts available in a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IEnumerable<string> GetFonts(string path)
        {
            return Directory.GetFiles(path, "*.tt?");
        }
    }
}
