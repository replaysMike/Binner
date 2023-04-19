namespace Binner.Model
{
    public class PartResults
    {
        /// <summary>
        /// List of matching parts
        /// </summary>
        public ICollection<CommonPart> Parts { get; set; } = new List<CommonPart>();

        /// <summary>
        /// List of available product images
        /// </summary>
        public ICollection<NameValuePair<string>> ProductImages { get; set; } = new List<NameValuePair<string>>();

        /// <summary>
        /// List of available datasheets
        /// </summary>
        public ICollection<NameValuePair<DatasheetSource>> Datasheets { get; set; } = new List<NameValuePair<DatasheetSource>>();

        /// <summary>
        /// List of pinout example images
        /// </summary>
        public ICollection<NameValuePair<string>> PinoutImages { get; set; } = new List<NameValuePair<string>>();

        /// <summary>
        /// List of circuit example images
        /// </summary>
        public ICollection<NameValuePair<string>> CircuitImages { get; set; } = new List<NameValuePair<string>>();
    }
}
