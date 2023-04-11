using AutoMapper;
using Binner.Model.Common;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class ProjectPcbAssignmentProfile : Profile
    {
        public ProjectPcbAssignmentProfile()
        {
            CreateMap<ProjectPcbAssignment, DataModel.ProjectPcbAssignment>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.ProjectId, options => options.MapFrom(x => x.ProjectId))
                .ForMember(x => x.ProjectPcbAssignmentId, options => options.MapFrom(x => x.ProjectPcbAssignmentId))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.Pcb, options => options.Ignore())
                .ForMember(x => x.Project, options => options.Ignore())
                ;

            CreateMap<DataModel.ProjectPcbAssignment, ProjectPcbAssignment>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.ProjectId, options => options.MapFrom(x => x.ProjectId))
                .ForMember(x => x.ProjectPcbAssignmentId, options => options.MapFrom(x => x.ProjectPcbAssignmentId))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                ;
        }
    }
}
