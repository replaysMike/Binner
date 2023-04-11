using AutoMapper;
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
        }
    }
}
