using Binner.Data;
using Microsoft.EntityFrameworkCore;

namespace Binner.StorageProvider.EntityFrameworkCore
{
    public class GenericDbContextFactory<TContext> : IGenericDbContextFactory
        where TContext : DbContext, IBinnerContext
    {
        IDbContextFactory<TContext> _factory;
        public GenericDbContextFactory(IDbContextFactory<TContext> factory)
        {
            _factory = factory;
        }

        public async Task<IBinnerContext> CreateDbContextAsync()
        {
            return await _factory.CreateDbContextAsync();
        }

        /// <summary>
        /// Create a DbContext as a specified concrete type
        /// </summary>
        /// <typeparam name="TTypedContext"></typeparam>
        /// <returns></returns>
        public async Task<TTypedContext> CreateDbContextAsync<TTypedContext>() where TTypedContext : DbContext, IBinnerContext
        {
            return await _factory.CreateDbContextAsync() as TTypedContext ?? throw new Exception($"The type {typeof(TContext)} cannot be converted to {typeof(TTypedContext)}");
        }
    }
}