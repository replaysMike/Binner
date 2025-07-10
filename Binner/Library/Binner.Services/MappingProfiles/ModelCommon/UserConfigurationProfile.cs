using AutoMapper;
using Binner.Model.Configuration;
using Binner.Model.Responses;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles
{
    public class UserConfigurationProfile : Profile
    {
        public UserConfigurationProfile()
        {
            CreateMap<DataModel.UserConfiguration, UserConfiguration>()
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Language))
                .ForMember(x => x.BarcodeBufferTime, options => options.MapFrom(x => x.BarcodeBufferTime))
                .ForMember(x => x.BarcodeEnabled, options => options.MapFrom(x => x.BarcodeEnabled))
                .ForMember(x => x.BarcodeIsDebug, options => options.MapFrom(x => x.BarcodeIsDebug))
                .ForMember(x => x.BarcodeMaxKeystrokeThresholdMs, options => options.MapFrom(x => x.BarcodeMaxKeystrokeThresholdMs))
                .ForMember(x => x.BarcodePrefix2D, options => options.MapFrom(x => x.BarcodePrefix2D))
                .ForMember(x => x.BarcodeProfile, options => options.MapFrom(x => x.BarcodeProfile))
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.EnableAutoPartSearch, options => options.MapFrom(x => x.EnableAutoPartSearch))
                .ForMember(x => x.EnableDarkMode, options => options.MapFrom(x => x.EnableDarkMode))
                .ForMember(x => x.DefaultPartLabelId, options => options.MapFrom(x => x.DefaultPartLabelId))
            ;

            CreateMap<UserConfiguration, DataModel.UserConfiguration>()
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Language))
                .ForMember(x => x.BarcodeBufferTime, options => options.MapFrom(x => x.BarcodeBufferTime))
                .ForMember(x => x.BarcodeEnabled, options => options.MapFrom(x => x.BarcodeEnabled))
                .ForMember(x => x.BarcodeIsDebug, options => options.MapFrom(x => x.BarcodeIsDebug))
                .ForMember(x => x.BarcodeMaxKeystrokeThresholdMs, options => options.MapFrom(x => x.BarcodeMaxKeystrokeThresholdMs))
                .ForMember(x => x.BarcodePrefix2D, options => options.MapFrom(x => x.BarcodePrefix2D))
                .ForMember(x => x.BarcodeProfile, options => options.MapFrom(x => x.BarcodeProfile))
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.EnableAutoPartSearch, options => options.MapFrom(x => x.EnableAutoPartSearch))
                .ForMember(x => x.EnableDarkMode, options => options.MapFrom(x => x.EnableDarkMode))
                .ForMember(x => x.DefaultPartLabelId, options => options.MapFrom(x => x.DefaultPartLabelId))

                .ForMember(x => x.UserConfigurationId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
            ;

            CreateMap<UserConfiguration, LocaleSettingsResponse>()
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Language))
            ;

            CreateMap<UserConfiguration, UserLocaleConfiguration>()
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Language))

                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
            ;

            CreateMap<LocaleConfiguration, UserConfiguration>()
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Language))
                .ForMember(x => x.BarcodeBufferTime, options => options.Ignore())
                .ForMember(x => x.BarcodeEnabled, options => options.Ignore())
                .ForMember(x => x.BarcodeIsDebug, options => options.Ignore())
                .ForMember(x => x.BarcodeMaxKeystrokeThresholdMs, options => options.Ignore())
                .ForMember(x => x.BarcodePrefix2D, options => options.Ignore())
                .ForMember(x => x.BarcodeProfile, options => options.Ignore())
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.DefaultPartLabelId, options => options.Ignore())
            ;

            CreateMap<BarcodeConfiguration, UserConfiguration>()
                .ForMember(x => x.Currency, options => options.Ignore())
                .ForMember(x => x.Language, options => options.Ignore())
                .ForMember(x => x.BarcodeBufferTime, options => options.MapFrom(x => x.BufferTime))
                .ForMember(x => x.BarcodeEnabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.BarcodeIsDebug, options => options.MapFrom(x => x.IsDebug))
                .ForMember(x => x.BarcodeMaxKeystrokeThresholdMs, options => options.MapFrom(x => x.MaxKeystrokeThresholdMs))
                .ForMember(x => x.BarcodePrefix2D, options => options.MapFrom(x => x.Prefix2D))
                .ForMember(x => x.BarcodeProfile, options => options.MapFrom(x => x.Profile))
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.DefaultPartLabelId, options => options.Ignore())
            ;
        }
    }
}
