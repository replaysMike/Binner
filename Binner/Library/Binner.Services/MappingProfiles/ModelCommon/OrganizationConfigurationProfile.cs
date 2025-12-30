using AutoMapper;
using Binner.Model.Configuration;
using Binner.Model.Integrations;
using Newtonsoft.Json;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles
{
    public class OrganizationConfigurationProfile : Profile
    {
        public OrganizationConfigurationProfile()
        {
            CreateMap<DataModel.OrganizationConfiguration, OrganizationConfiguration>()
                .ForMember(x => x.UseModule, options => options.MapFrom(x => x.UseModule))
                .ForMember(x => x.LicenseKey, options => options.MapFrom(x => x.LicenseKey))
                .ForMember(x => x.MaxCacheItems, options => options.MapFrom(x => x.MaxCacheItems))
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.MapFrom(x => x.CacheSlidingExpirationMinutes))
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.MapFrom(x => x.CacheAbsoluteExpirationMinutes))
                .ForMember(x => x.KiCad, options => options.MapFrom(x => string.IsNullOrEmpty(x.KiCadSettingsJson) ? new KiCadSettings() : JsonConvert.DeserializeObject(x.KiCadSettingsJson)))
                .ForMember(x => x.enableAutomaticMetadataFetchingForExistingParts, options => options.MapFrom(x => x.enableAutomaticMetadataFetchingForExistingParts))
            ;

            CreateMap<OrganizationConfiguration, DataModel.OrganizationConfiguration>()
                .ForMember(x => x.UseModule, options => options.MapFrom(x => x.UseModule))
                .ForMember(x => x.LicenseKey, options => options.MapFrom(x => x.LicenseKey))
                .ForMember(x => x.MaxCacheItems, options => options.MapFrom(x => x.MaxCacheItems))
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.MapFrom(x => x.CacheSlidingExpirationMinutes))
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.MapFrom(x => x.CacheAbsoluteExpirationMinutes))
                .ForMember(x => x.KiCadSettingsJson, options => options.MapFrom(x => JsonConvert.SerializeObject(x.KiCad)))
                .ForMember(x => x.enableAutomaticMetadataFetchingForExistingParts, options => options.MapFrom(x => x.enableAutomaticMetadataFetchingForExistingParts))

                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.OrganizationConfigurationId, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.Organization, options => options.Ignore())
            ;
        }
    }
}
