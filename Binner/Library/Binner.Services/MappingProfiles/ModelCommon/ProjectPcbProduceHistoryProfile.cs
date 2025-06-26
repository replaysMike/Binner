using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class ProjectPcbProduceHistoryProfile : Profile
    {
        public ProjectPcbProduceHistoryProfile()
        {
            CreateMap<ProjectPcbProduceHistory, DataModel.ProjectPcbProduceHistory>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.SerialNumber, options => options.MapFrom(x => x.SerialNumber))
                .ForMember(x => x.PartsConsumed, options => options.MapFrom(x => x.PartsConsumed))
                .ForMember(x => x.PcbQuantity, options => options.MapFrom(x => x.PcbQuantity))
                .ForMember(x => x.PcbCost, options => options.MapFrom(x => x.PcbCost))
                .ForMember(x => x.ProjectPcbProduceHistoryId, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.Pcb, options => options.Ignore())
                .ForMember(x => x.ProjectProduceHistory, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
#if INITIALCREATE
                .ForMember(x => x.User, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
#endif
                ;

            CreateMap<DataModel.ProjectPcbProduceHistory, ProjectPcbProduceHistory>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.PcbQuantity, options => options.MapFrom(x => x.PcbQuantity))
                .ForMember(x => x.PcbCost, options => options.MapFrom(x => x.PcbCost))
                .ForMember(x => x.SerialNumber, options => options.MapFrom(x => x.SerialNumber))
                .ForMember(x => x.PartsConsumed, options => options.MapFrom(x => x.PartsConsumed))
                .ForMember(x => x.ProjectPcbProduceHistoryId, options => options.MapFrom(x => x.ProjectPcbProduceHistoryId))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.ProjectProduceHistoryId, options => options.MapFrom(x => x.ProjectProduceHistoryId))
                .ForMember(x => x.Pcb, options => options.MapFrom(x => x.Pcb))
                ;
        }
    }
}
