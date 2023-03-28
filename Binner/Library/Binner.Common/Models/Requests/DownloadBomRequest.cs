namespace Binner.Common.Models
{
    public class DownloadBomRequest
    {
        /// <summary>
        /// The project id
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Format of file
        /// </summary>
        public DownloadFormats Format { get; set; } = DownloadFormats.Csv;
    }

    public enum DownloadFormats
    {
        Csv,
        Excel
    }
}
