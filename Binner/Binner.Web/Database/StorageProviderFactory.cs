﻿using AutoMapper;
using Binner.Common.StorageProviders;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.StorageProvider.EntityFrameworkCore;
using LightInject;
using Microsoft.EntityFrameworkCore;
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

        public IStorageProvider Create(LightInject.IServiceContainer container, StorageProviderConfiguration configuration)
        {
            // override: all providers now redirect to the EF provider
            var provider = Providers[EntityFrameworkStorageProvider.ProviderName.ToLower()];
            // materialize the dependencies
            var contextFactory = container.GetInstance<IDbContextFactory<BinnerContext>>();
            //var contextFactory = container.GetInstance<IGenericDbContextFactory>();
            var mapper = container.GetInstance<IMapper>();
            var partTypesCache = container.GetInstance<IPartTypesCache>();
            var licensedStorageProvider = container.GetInstance<ILicensedStorageProvider>();
            var logger = container.GetInstance<ILogger<EntityFrameworkStorageProvider>>();
            var requestContext = container.GetInstance<IRequestContextAccessor>();
            var instance = Activator.CreateInstance(provider, contextFactory, mapper, configuration.Provider, configuration.ProviderConfiguration, 
                partTypesCache, licensedStorageProvider, logger, requestContext) as IStorageProvider 
                ?? throw new Exception($"Unable to create StorageProvider: {EntityFrameworkStorageProvider.ProviderName}");
            return instance;
        }

        public IStorageProvider CreateLimited(LightInject.IServiceContainer container, StorageProviderConfiguration configuration)
        {
            // override: all providers now redirect to the EF provider
            var provider = Providers[EntityFrameworkStorageProvider.ProviderName.ToLower()];
            // materialize the dependencies
            var contextFactory = container.GetInstance<IDbContextFactory<BinnerContext>>();
            var instance = Activator.CreateInstance(provider, contextFactory, configuration.Provider, configuration.ProviderConfiguration) as IStorageProvider ?? throw new Exception($"Unable to create StorageProvider: {EntityFrameworkStorageProvider.ProviderName}");
            return instance;
        }
    }
}
