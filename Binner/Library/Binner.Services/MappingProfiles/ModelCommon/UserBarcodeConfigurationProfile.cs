using AutoMapper;
using Binner.Model.Configuration;

namespace Binner.Services.MappingProfiles
{
    public class UserBarcodeConfigurationProfile : Profile
    {
        public UserBarcodeConfigurationProfile()
        {
            CreateMap<BarcodeConfiguration, UserBarcodeConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.BufferTime, options => options.MapFrom(x => x.BufferTime))
                .ForMember(x => x.IsDebug, options => options.MapFrom(x => x.IsDebug))
                .ForMember(x => x.MaxKeystrokeThresholdMs, options => options.MapFrom(x => x.MaxKeystrokeThresholdMs))
                .ForMember(x => x.Prefix2D, options => options.MapFrom(x => x.Prefix2D))
                .ForMember(x => x.Profile, options => options.MapFrom(x => x.Profile))

                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
            ;
        }
    }
}
