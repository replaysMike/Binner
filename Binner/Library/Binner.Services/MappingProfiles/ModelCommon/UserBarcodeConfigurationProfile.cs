using AutoMapper;
using Binner.Model.Configuration;
using Binner.Model.Responses;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles
{
    public class UserBarcodeConfigurationProfile : Profile
    {
        public UserBarcodeConfigurationProfile()
        {
            CreateMap<DataModel.UserBarcodeConfiguration, UserBarcodeConfiguration>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.BufferTime, options => options.MapFrom(x => x.BufferTime))
                .ForMember(x => x.IsDebug, options => options.MapFrom(x => x.IsDebug))
                .ForMember(x => x.MaxKeystrokeThresholdMs, options => options.MapFrom(x => x.MaxKeystrokeThresholdMs))
                .ForMember(x => x.Prefix2D, options => options.MapFrom(x => x.Prefix2D))
                .ForMember(x => x.Profile, options => options.MapFrom(x => x.Profile))

                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                ;

            CreateMap<UserBarcodeConfiguration, DataModel.UserBarcodeConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.BufferTime, options => options.MapFrom(x => x.BufferTime))
                .ForMember(x => x.IsDebug, options => options.MapFrom(x => x.IsDebug))
                .ForMember(x => x.MaxKeystrokeThresholdMs, options => options.MapFrom(x => x.MaxKeystrokeThresholdMs))
                .ForMember(x => x.Prefix2D, options => options.MapFrom(x => x.Prefix2D))
                .ForMember(x => x.Profile, options => options.MapFrom(x => x.Profile))

                .ForMember(x => x.UserBarcodeConfigurationId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
                ;
        }
    }
}
