using AutoMapper;
using Binner.Common.Models;
using Binner.Model.Common;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class ProjectProduceHistoryProfile : Profile
    {
        public ProjectProduceHistoryProfile()
        {
            CreateMap<ProjectProduceHistory, DataModel.ProjectProduceHistory>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.ProjectId, options => options.MapFrom(x => x.ProjectId))
                .ForMember(x => x.ProduceUnassociated, options => options.MapFrom(x => x.ProduceUnassociated))
                .ForMember(x => x.PartsConsumed, options => options.MapFrom(x => x.PartsConsumed))
                .ForMember(x => x.Quantity, options => options.MapFrom(x => x.Quantity))
                .ForMember(x => x.ProjectProduceHistoryId, options => options.MapFrom(x => x.ProjectProduceHistoryId))
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.Project, options => options.Ignore())
                .ForMember(x => x.ProjectPcbProduceHistory, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
#if INITIALCREATE
                .ForMember(x => x.User, options => options.Ignore())
#endif
                ;

            CreateMap<DataModel.ProjectProduceHistory, ProjectProduceHistory>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.ProjectId, options => options.MapFrom(x => x.ProjectId))
                .ForMember(x => x.ProduceUnassociated, options => options.MapFrom(x => x.ProduceUnassociated))
                .ForMember(x => x.PartsConsumed, options => options.MapFrom(x => x.PartsConsumed))
                .ForMember(x => x.Quantity, options => options.MapFrom(x => x.Quantity))
                .ForMember(x => x.Pcbs, options => options.MapFrom(x => x.ProjectPcbProduceHistory))
                .ForMember(x => x.ProjectProduceHistoryId, options => options.MapFrom(x => x.ProjectProduceHistoryId))
                ;
        }
    }
}
