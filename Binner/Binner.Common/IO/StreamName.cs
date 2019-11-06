namespace Binner.Common.IO
{
    public class StreamName
    {
        public string Name { get; set; }
        public string FileExtension { get; set; }

        public StreamName(string name, string fileExtension)
        {
            Name = name;
            FileExtension = fileExtension;
        }
    }
}
