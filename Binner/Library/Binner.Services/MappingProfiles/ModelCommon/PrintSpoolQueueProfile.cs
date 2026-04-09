using AutoMapper;
using Binner.Model;
using Binner.Model.Responses;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class PrintSpoolQueueProfile : Profile
    {
        public PrintSpoolQueueProfile()
        {
            CreateMap<DataModel.PrintSpoolQueue, PrintSpoolQueue>()
                .ForMember(x => x.PrintType, options => options.MapFrom(x => x.PrintType))
                .ForMember(x => x.Json, options => options.MapFrom(x => x.Json))
                .ForMember(x => x.LabelJson, options => options.MapFrom(x => x.LabelJson))
                .ForMember(x => x.TemplateJson, options => options.MapFrom(x => x.TemplateJson))
                .ForMember(x => x.Crc32, options => options.MapFrom(x => x.Crc32))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.GlobalId, options => options.MapFrom(x => x.GlobalId))
                ;
        }
    }
}
