using AutoMapper;
using Binner.Model;
using Binner.Model.KiCad;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class KiCadCategoryProfile : Profile
    {
        public KiCadCategoryProfile()
        {
            CreateMap<PartType, KiCadCategory>()
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.ReferenceDesignator, options => options.MapFrom(x => x.ReferenceDesignator))
                .ForMember(x => x.SymbolId, options => options.MapFrom(x => x.SymbolId))
                .ForMember(x => x.Id, options => options.MapFrom(x => x.PartTypeId.ToString()))
                .ReverseMap()
                ;
        }
    }
}
