using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class OAuthRequestProfile : Profile
    {
        public OAuthRequestProfile()
        {
            CreateMap<OAuthRequest, DataModel.OAuthRequest>()
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.Provider, options => options.MapFrom(x => x.Provider))
                .ForMember(x => x.Error, options => options.MapFrom(x => x.Error))
                .ForMember(x => x.ErrorDescription, options => options.MapFrom(x => x.ErrorDescription))
                .ForMember(x => x.ReturnToUrl, options => options.MapFrom(x => x.ReturnToUrl))
                .ForMember(x => x.OAuthRequestId, options => options.MapFrom(x => x.OAuthRequestId))
                .ForMember(x => x.AuthorizationCode, options => options.MapFrom(x => x.AuthorizationCode))
                .ForMember(x => x.AuthorizationReceived, options => options.MapFrom(x => x.AuthorizationReceived))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.RequestId, options => options.MapFrom(x => x.RequestId))
#if INITIALCREATE
                .ForMember(x => x.User, options => options.Ignore())
#endif
                .ForMember(x => x.Ip, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ReverseMap();

            CreateMap<OAuthAuthorization, DataModel.OAuthRequest>()
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.Provider, options => options.MapFrom(x => x.Provider))
                .ForMember(x => x.Error, options => options.MapFrom(x => x.Error))
                .ForMember(x => x.ErrorDescription, options => options.MapFrom(x => x.ErrorDescription))
                .ForMember(x => x.ReturnToUrl, options => options.MapFrom(x => x.ReturnToUrl))
                .ForMember(x => x.AuthorizationCode, options => options.MapFrom(x => x.AuthorizationCode))
                .ForMember(x => x.AuthorizationReceived, options => options.MapFrom(x => x.AuthorizationReceived))
                .ForMember(x => x.RequestId, options => options.MapFrom(x => x.Id))
                .ForMember(x => x.OAuthRequestId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
#if INITIALCREATE
                .ForMember(x => x.User, options => options.Ignore())
#endif
                .ForMember(x => x.Ip, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ReverseMap();
        }
    }
}
