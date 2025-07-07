using AutoMapper;
using Binner.Model.Configuration;
using Binner.Model.Responses;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles
{
    public class UserLocaleConfigurationProfile : Profile
    {
        public UserLocaleConfigurationProfile()
        {
            CreateMap<DataModel.UserLocaleConfiguration, UserLocaleConfiguration>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Language))

                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                ;

            CreateMap<UserLocaleConfiguration, DataModel.UserLocaleConfiguration>()
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Language))

                .ForMember(x => x.UserLocaleConfigurationId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
                ;

            CreateMap<LocaleConfiguration, LocaleSettingsResponse>()
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Language))
                ;

            CreateMap<LocaleConfiguration, UserLocaleConfiguration>()
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Language))

                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                ;
        }
    }
}
