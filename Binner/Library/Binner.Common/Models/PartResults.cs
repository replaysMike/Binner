using System;
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
        /// <summary>
        /// Unique Resource Id
        /// </summary>
        public Guid ResourceId { get; set; }

        /// <summary>
        /// Url to cover image
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Url to datasheet
        /// </summary>
        public string DatasheetUrl { get; set; }

        /// <summary>
        /// Title of datasheet
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Description of datasheet
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Manufacturer
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Images count in PDF
        /// </summary>
        public int ImageCount { get; set; }
        
        /// <summary>
        /// Page count
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Original url to datasheet
        /// </summary>
        public string? OriginalUrl { get; set; }

        /// <summary>
        /// Product url if applicable
        /// </summary>
        public string? ProductUrl { get; set; }

        public DatasheetSource(string? imageUrl, string datasheetUrl, string? title, string? description, string? manufacturer)
        {
            ResourceId = Guid.Empty;
            ImageUrl = imageUrl;
            DatasheetUrl = datasheetUrl;
            Title = title;
            Description = description;
            Manufacturer = manufacturer;
        }

        public DatasheetSource(Guid resourceId, int imageCount, int pageCount, string? imageUrl, string datasheetUrl, string? title, string? description, string? manufacturer, string? originalUrl, string? productUrl)
        {
            ResourceId = resourceId;
            ImageCount = imageCount;
            PageCount = pageCount;
            ImageUrl = imageUrl;
            DatasheetUrl = datasheetUrl;
            Title = title;
            Description = description;
            Manufacturer = manufacturer;
            OriginalUrl = originalUrl;
            ProductUrl = productUrl;
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
