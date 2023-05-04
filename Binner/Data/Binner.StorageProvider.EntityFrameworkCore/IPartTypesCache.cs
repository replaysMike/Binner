using static Binner.StorageProvider.EntityFrameworkCore.PartTypesCache;

namespace Binner.StorageProvider.EntityFrameworkCore;

public interface IPartTypesCache
{
    /// <summary>
    /// Get the PartTypes cache
    /// </summary>
    List<CachedPartTypeResponse> Cache { get; }

    /// <summary>
    /// Reload the part types cache
    /// </summary>
    void InvalidateCache();
}