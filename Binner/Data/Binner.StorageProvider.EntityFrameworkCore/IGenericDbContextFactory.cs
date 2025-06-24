using Binner.Data;
using Microsoft.EntityFrameworkCore;

namespace Binner.StorageProvider.EntityFrameworkCore
{
    public interface IGenericDbContextFactory
    {
        Task<IBinnerContext> CreateDbContextAsync();
        Task<TTypedContext> CreateDbContextAsync<TTypedContext>() where TTypedContext : DbContext, IBinnerContext;
    }
}