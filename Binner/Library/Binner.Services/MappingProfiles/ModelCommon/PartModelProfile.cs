using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class PartModelProfile : Profile
    {
        public PartModelProfile()
        {
            CreateMap<DataModel.PartModel, PartModel>()
                .ForMember(x => x.PartModelId, options => options.MapFrom(x => x.PartModelId))
                .ForMember(x => x.PartId, options => options.MapFrom(x => x.PartId))
                .ForMember(x => x.Filename, options => options.MapFrom(x => x.Filename))
                .ForMember(x => x.ModelType, options => options.MapFrom(x => x.ModelType))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Source, options => options.MapFrom(x => x.Source))
                .ForMember(x => x.Url, options => options.MapFrom(x => x.Url))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.GlobalId, options => options.MapFrom(x => x.GlobalId))

                .ReverseMap()
                ;
        }
    }
}
