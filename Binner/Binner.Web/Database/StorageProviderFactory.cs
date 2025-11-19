using AutoMapper;
using Binner.Common.StorageProviders;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.StorageProvider.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Binner.Web.Database
{
    public class StorageProviderFactory : IStorageProviderFactory
    {
        public IDictionary<string, Type> Providers { get; } = new Dictionary<string, Type>();

        public StorageProviderFactory()
        {
            Providers.Add(EntityFrameworkStorageProvider.ProviderName.ToLower(), typeof(EntityFrameworkStorageProvider));
        }

        public IStorageProvider Create(IServiceProvider serviceProvider, StorageProviderConfiguration storageProviderConfiguration)
        {
            var provider = Providers[EntityFrameworkStorageProvider.ProviderName.ToLower()];
            // materialize the dependencies
            var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<BinnerContext>>();
            var mapper = serviceProvider.GetRequiredService<IMapper>();
            var partTypesCache = serviceProvider.GetRequiredService<IPartTypesCache>();
            var licensedStorageProvider = serviceProvider.GetRequiredService<ILicensedStorageProvider>();
            var logger = serviceProvider.GetRequiredService<ILogger<EntityFrameworkStorageProvider>>();
            var requestContext = serviceProvider.GetRequiredService<IRequestContextAccessor>();
            var instance = CreateInstance(provider, contextFactory, mapper, storageProviderConfiguration, partTypesCache, licensedStorageProvider, logger, requestContext);
            return instance;
        }

        public IStorageProvider CreateLimited(IServiceProvider serviceProvider, StorageProviderConfiguration storageProviderConfiguration)
        {
            // override: all providers now redirect to the EF provider
            var provider = Providers[EntityFrameworkStorageProvider.ProviderName.ToLower()];
            // materialize the dependencies
            var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<BinnerContext>>();
            // call the alternate constructor
            var instance = Activator.CreateInstance(provider, contextFactory, storageProviderConfiguration.Provider, storageProviderConfiguration.ProviderConfiguration) as IStorageProvider 
                ?? throw new Exception($"Unable to create StorageProvider: {EntityFrameworkStorageProvider.ProviderName}");
            return instance;
        }

        private IStorageProvider CreateInstance(Type provider, IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, StorageProviderConfiguration storageProviderConfiguration, IPartTypesCache partTypesCache, ILicensedStorageProvider licensedStorageProvider, ILogger<EntityFrameworkStorageProvider> logger, IRequestContextAccessor requestContext)
        {
            var instance = Activator.CreateInstance(provider, contextFactory, mapper, storageProviderConfiguration.Provider, storageProviderConfiguration.ProviderConfiguration,
                partTypesCache, licensedStorageProvider, logger, requestContext) as IStorageProvider
                ?? throw new Exception($"Unable to create StorageProvider: {EntityFrameworkStorageProvider.ProviderName}");
            return instance;
        }
    }
}
