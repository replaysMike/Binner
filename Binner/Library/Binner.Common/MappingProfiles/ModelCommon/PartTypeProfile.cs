using AutoMapper;
using Binner.Common.Models;
using Binner.Model.Common;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class PartTypeProfile : Profile
    {
        public PartTypeProfile()
        {
            CreateMap<DataModel.PartType, PartType>(MemberList.None)
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.ParentPartTypeId, options => options.MapFrom(x => x.ParentPartTypeId))
                .ForMember(x => x.PartTypeId, options => options.MapFrom(x => x.PartTypeId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                ;

            CreateMap<PartType, DataModel.PartType>(MemberList.None)
                .ForMember(x => x.ParentPartTypeId, options => options.MapFrom(x => x.ParentPartTypeId))
                .ForMember(x => x.PartTypeId, options => options.MapFrom(x => x.PartTypeId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
                .ForMember(x => x.ParentPartType, options => options.Ignore())
                .ForMember(x => x.Parts, options => options.Ignore())
                ;

            CreateMap<DataModel.PartType, PartTypeResponse>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.ParentPartTypeId, options => options.MapFrom(x => x.ParentPartTypeId))
                .ForMember(x => x.PartTypeId, options => options.MapFrom(x => x.PartTypeId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Parts, options => options.MapFrom(x => x.Parts != null ? x.Parts.Count : 0))
                ;
        }
    }
}
