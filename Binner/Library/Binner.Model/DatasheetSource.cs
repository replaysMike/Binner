namespace Binner.Model;

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