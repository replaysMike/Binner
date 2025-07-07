using AutoMapper;
using Binner.Model.Configuration.Integrations;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles
{
    public class UserIntegrationConfigurationProfile : Profile
    {
        public UserIntegrationConfigurationProfile()
        {
            CreateMap<DataModel.UserIntegrationConfiguration, UserIntegrationConfiguration>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.SwarmEnabled, options => options.MapFrom(x => x.SwarmEnabled))
                .ForMember(x => x.SwarmApiKey, options => options.MapFrom(x => x.SwarmApiKey))
                .ForMember(x => x.SwarmApiUrl, options => options.MapFrom(x => x.SwarmApiUrl))
                .ForMember(x => x.SwarmTimeout, options => options.MapFrom(x => x.SwarmTimeout))
                .ForMember(x => x.DigiKeyEnabled, options => options.MapFrom(x => x.DigiKeyEnabled))
                .ForMember(x => x.DigiKeySite, options => options.MapFrom(x => x.DigiKeySite))
                .ForMember(x => x.DigiKeyClientId, options => options.MapFrom(x => x.DigiKeyClientId))
                .ForMember(x => x.DigiKeyClientSecret, options => options.MapFrom(x => x.DigiKeyClientSecret))
                .ForMember(x => x.DigiKeyOAuthPostbackUrl, options => options.MapFrom(x => x.DigiKeyOAuthPostbackUrl))
                .ForMember(x => x.DigiKeyApiUrl, options => options.MapFrom(x => x.DigiKeyApiUrl))
                .ForMember(x => x.MouserEnabled, options => options.MapFrom(x => x.MouserEnabled))
                .ForMember(x => x.MouserSearchApiKey, options => options.MapFrom(x => x.MouserSearchApiKey))
                .ForMember(x => x.MouserCartApiKey, options => options.MapFrom(x => x.MouserCartApiKey))
                .ForMember(x => x.MouserOrderApiKey, options => options.MapFrom(x => x.MouserOrderApiKey))
                .ForMember(x => x.MouserApiUrl, options => options.MapFrom(x => x.MouserApiUrl))
                .ForMember(x => x.ArrowEnabled, options => options.MapFrom(x => x.ArrowEnabled))
                .ForMember(x => x.ArrowUsername, options => options.MapFrom(x => x.ArrowUsername))
                .ForMember(x => x.ArrowApiKey, options => options.MapFrom(x => x.ArrowApiKey))
                .ForMember(x => x.ArrowApiUrl, options => options.MapFrom(x => x.ArrowApiUrl))
                .ForMember(x => x.NexarEnabled, options => options.MapFrom(x => x.OctopartEnabled))
                .ForMember(x => x.NexarClientId, options => options.MapFrom(x => x.OctopartClientId))
                .ForMember(x => x.NexarClientSecret, options => options.MapFrom(x => x.OctopartClientSecret))
                .ForMember(x => x.TmeEnabled, options => options.MapFrom(x => x.TmeEnabled))
                .ForMember(x => x.TmeApiKey, options => options.MapFrom(x => x.TmeApiKey))
                .ForMember(x => x.TmeApplicationSecret, options => options.MapFrom(x => x.TmeApplicationSecret))
                .ForMember(x => x.TmeCountry, options => options.MapFrom(x => x.TmeCountry))
                .ForMember(x => x.TmeApiUrl, options => options.MapFrom(x => x.TmeApiUrl))
                .ForMember(x => x.TmeResolveExternalLinks, options => options.MapFrom(x => x.TmeResolveExternalLinks))

                .ForMember(x => x.UserIntegrationConfigurationId, options => options.MapFrom(x => x.UserIntegrationConfigurationId))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                ;

            CreateMap<UserIntegrationConfiguration, DataModel.UserIntegrationConfiguration>()
                .ForMember(x => x.SwarmEnabled, options => options.MapFrom(x => x.SwarmEnabled))
                .ForMember(x => x.SwarmApiKey, options => options.MapFrom(x => x.SwarmApiKey))
                .ForMember(x => x.SwarmApiUrl, options => options.MapFrom(x => x.SwarmApiUrl))
                .ForMember(x => x.SwarmTimeout, options => options.MapFrom(x => x.SwarmTimeout))
                .ForMember(x => x.DigiKeyEnabled, options => options.MapFrom(x => x.DigiKeyEnabled))
                .ForMember(x => x.DigiKeySite, options => options.MapFrom(x => x.DigiKeySite))
                .ForMember(x => x.DigiKeyClientId, options => options.MapFrom(x => x.DigiKeyClientId))
                .ForMember(x => x.DigiKeyClientSecret, options => options.MapFrom(x => x.DigiKeyClientSecret))
                .ForMember(x => x.DigiKeyOAuthPostbackUrl, options => options.MapFrom(x => x.DigiKeyOAuthPostbackUrl))
                .ForMember(x => x.DigiKeyApiUrl, options => options.MapFrom(x => x.DigiKeyApiUrl))
                .ForMember(x => x.MouserEnabled, options => options.MapFrom(x => x.MouserEnabled))
                .ForMember(x => x.MouserSearchApiKey, options => options.MapFrom(x => x.MouserSearchApiKey))
                .ForMember(x => x.MouserCartApiKey, options => options.MapFrom(x => x.MouserCartApiKey))
                .ForMember(x => x.MouserOrderApiKey, options => options.MapFrom(x => x.MouserOrderApiKey))
                .ForMember(x => x.MouserApiUrl, options => options.MapFrom(x => x.MouserApiUrl))
                .ForMember(x => x.ArrowEnabled, options => options.MapFrom(x => x.ArrowEnabled))
                .ForMember(x => x.ArrowUsername, options => options.MapFrom(x => x.ArrowUsername))
                .ForMember(x => x.ArrowApiKey, options => options.MapFrom(x => x.ArrowApiKey))
                .ForMember(x => x.ArrowApiUrl, options => options.MapFrom(x => x.ArrowApiUrl))
                .ForMember(x => x.OctopartEnabled, options => options.MapFrom(x => x.NexarEnabled))
                .ForMember(x => x.OctopartClientId, options => options.MapFrom(x => x.NexarClientId))
                .ForMember(x => x.OctopartClientSecret, options => options.MapFrom(x => x.NexarClientSecret))
                .ForMember(x => x.TmeEnabled, options => options.MapFrom(x => x.TmeEnabled))
                .ForMember(x => x.TmeApiKey, options => options.MapFrom(x => x.TmeApiKey))
                .ForMember(x => x.TmeApplicationSecret, options => options.MapFrom(x => x.TmeApplicationSecret))
                .ForMember(x => x.TmeCountry, options => options.MapFrom(x => x.TmeCountry))
                .ForMember(x => x.TmeApiUrl, options => options.MapFrom(x => x.TmeApiUrl))
                .ForMember(x => x.TmeResolveExternalLinks, options => options.MapFrom(x => x.TmeResolveExternalLinks))

                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.UserIntegrationConfigurationId, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
                ;
        }
    }
}
