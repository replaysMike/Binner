using AutoMapper;
using Binner.Data;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.StorageProvider.EntityFrameworkCore;
using LightInject;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Binner.Common.StorageProviders
{
    public class StorageProviderFactory : IStorageProviderFactory
    {
        public IDictionary<string, Type> Providers { get; } = new Dictionary<string, Type>();

        public StorageProviderFactory()
        {
            Providers.Add(EntityFrameworkStorageProvider.ProviderName.ToLower(), typeof(EntityFrameworkStorageProvider));
        }

        public IStorageProvider Create(LightInject.IServiceContainer container, string providerName, IDictionary<string, string> config)
        {
            // override: all providers now redirect to the EF provider
            var provider = Providers[EntityFrameworkStorageProvider.ProviderName.ToLower()];
            // materialize the dependencies
            var contextFactory = container.GetInstance<IDbContextFactory<BinnerContext>>();
            var mapper = container.GetInstance<IMapper>();
            var licensedStorageProvider = container.GetInstance<ILicensedStorageProvider>();
            var instance = Activator.CreateInstance(provider, contextFactory, mapper, providerName, config, licensedStorageProvider) as IStorageProvider ?? throw new Exception($"Unable to create StorageProvider: {EntityFrameworkStorageProvider.ProviderName}");
            return instance;
        }
    }
}
