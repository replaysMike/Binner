using AutoMapper;
using System;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class UserTokenProfile : Profile
    {
        public UserTokenProfile()
        {
            CreateMap<Token, DataModel.UserToken>()
                .ForMember(x => x.Token, options => options.MapFrom(x => x.Value))
                .ForMember(x => x.TokenTypeId, options => options.MapFrom(x => x.TokenType))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateExpiredUtc, options => options.MapFrom(x => x.DateExpiredUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.DateRevokedUtc, options => options.Ignore())
                .ForMember(x => x.ReplacedByToken, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
                .ForMember(x => x.UserTokenId, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.Ip, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                ;

            CreateMap<DataModel.UserToken, Token>()
                .ForMember(x => x.TokenType, options => options.MapFrom(x => x.TokenTypeId))
                .ForMember(x => x.Value, options => options.MapFrom(x => x.Token))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateExpiredUtc, options => options.MapFrom(x => x.DateExpiredUtc))
                ;
        }

        private string GetBase64Image(byte[]? imageBytes) => imageBytes != null && imageBytes.Length > 0 ? Convert.ToBase64String(imageBytes) : string.Empty;
    }
}
