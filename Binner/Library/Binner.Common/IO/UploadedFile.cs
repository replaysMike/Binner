using Binner.Model;
using System.IO;

namespace Binner.Common.IO
{
    public class UploadedFile
    {
        public string Filename { get; set; }

        public Stream Stream { get; set; }


        public UploadedFile(string filename, Stream stream)
        {
            Filename = filename;
            Stream = stream;
        }
    }
}
