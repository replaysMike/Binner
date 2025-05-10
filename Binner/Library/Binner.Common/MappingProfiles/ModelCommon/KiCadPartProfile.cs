using AutoMapper;
using Binner.Model;
using Binner.Model.KiCad;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class KiCadPartProfile : Profile
    {
        public KiCadPartProfile()
        {
            CreateMap<Part, KiCadPart>()
                .ForMember(x => x.Name, options => options.MapFrom(x => x.PartNumber))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.Id, options => options.MapFrom(x => x.PartId.ToString()))
                .ReverseMap()
                ;
        }
    }
}
