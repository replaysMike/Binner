using AutoMapper;
using Binner.Model.Common;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class OAuthCredentialsProfile : Profile
    {
        public OAuthCredentialsProfile()
        {
            CreateMap<OAuthCredential, DataModel.OAuthCredential>()
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.Provider, options => options.MapFrom(x => x.Provider))
                .ForMember(x => x.AccessToken, options => options.MapFrom(x => x.AccessToken))
                .ForMember(x => x.RefreshToken, options => options.MapFrom(x => x.RefreshToken))
                .ForMember(x => x.DateExpiresUtc, options => options.MapFrom(x => x.DateExpiresUtc))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
#if INITIALCREATE
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
#endif
                .ForMember(x => x.Ip, options => options.Ignore())
                .ReverseMap();
        }
    }
}
