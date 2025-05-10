namespace Binner.StorageProvider.EntityFrameworkCore;

public class CachedPartTypeResponse
{
    public long PartTypeId { get; set; }

    public string? Name { get; set; }
    /// <summary>
    /// Part type description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Reference designator
    /// </summary>
    public string? ReferenceDesignator { get; set; }

    /// <summary>
    /// The symbol id of the part type (KiCad)
    /// </summary>
    public string? SymbolId { get; set; }

    /// <summary>
    /// Optional keywords to help with search
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Name or SVG content of icon
    /// If left empty, default icon choices will be applied.
    /// </summary>
    public string? Icon { get;set; }

    public long? ParentPartTypeId { get; set; }

    public CachedPartTypeResponse? ParentPartType { get; set; }

    public long Parts { get; set; }

    public bool IsSystem { get; set; }

    public IDictionary<string, string?> CustomFields { get; set; } = new Dictionary<string, string?>();

    public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime DateModifiedUtc { get; set; }

    public int? UserId { get; set; }

    public int? OrganizationId { get; set; }
}