namespace Binner.StorageProvider.EntityFrameworkCore;

public class CachedPartTypeResponse
{
    public long PartTypeId { get; set; }
    public string? Name { get; set; }
    public long? ParentPartTypeId { get; set; }
    public CachedPartTypeResponse? ParentPartType { get; set; }
    public long Parts { get; set; }
    public bool IsSystem { get; set; }
    public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime DateModifiedUtc { get; set; }
    public int? UserId { get; set; }
    public int? OrganizationId { get; set; }
}