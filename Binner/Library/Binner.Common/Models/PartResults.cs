using System.Collections.Generic;

namespace Binner.Common.Models
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

    public class DatasheetSource
    {
        public string? ImageUrl { get; set; }
        public string DatasheetUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public DatasheetSource(string? imageUrl, string datasheetUrl, string title, string description, string manufacturer)
        {
            ImageUrl = imageUrl;
            DatasheetUrl = datasheetUrl;
            Title = title;
            Description = description;
            Manufacturer = manufacturer;
        }
    }

    public class NameValuePair<T>
    {
        public string? Name { get; set; }
        public T? Value { get; set; }
        public NameValuePair() { }
        public NameValuePair(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }
}
