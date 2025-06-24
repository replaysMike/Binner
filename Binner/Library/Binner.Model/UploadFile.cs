namespace Binner.Model
{
    public class UploadFile
    {
        public string Filename { get; set; }

        public Stream Stream { get; set; }

        public UploadFile(string filename, Stream stream)
        {
            Filename = filename;
            Stream = stream;
        }
    }
}
