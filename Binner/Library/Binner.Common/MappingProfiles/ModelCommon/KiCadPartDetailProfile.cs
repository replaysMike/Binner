using AutoMapper;
using Binner.Model;
using Binner.Model.KiCad;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class KiCadPartDetailProfile : Profile
    {
        public KiCadPartDetailProfile()
        {
            CreateMap<Part, KiCadPartDetail>()
                .ForMember(x => x.Name, options => options.MapFrom(x => x.PartNumber))
                .ForMember(x => x.SymbolIdStr, options => options.MapFrom(x => x.SymbolName))
                .ForMember(x => x.Id, options => options.MapFrom(x => x.PartId.ToString()))
                // mapped manually
                .ForMember(x => x.Fields, options => options.Ignore())
                .ForMember(x => x.ExcludeFromBom, options => options.Ignore())
                .ForMember(x => x.ExcludeFromBoard, options => options.Ignore())
                .ForMember(x => x.ExcludeFromSim, options => options.Ignore())
                .ReverseMap()
                ;
        }
    }
}
