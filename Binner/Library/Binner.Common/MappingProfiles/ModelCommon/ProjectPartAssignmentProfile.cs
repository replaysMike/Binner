using AutoMapper;
using Binner.Model.Common;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class ProjectPartAssignmentProfile : Profile
    {
        public ProjectPartAssignmentProfile()
        {
            CreateMap<ProjectPartAssignment, DataModel.ProjectPartAssignment>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.Notes, options => options.MapFrom(x => x.Notes))
                .ForMember(x => x.PartId, options => options.MapFrom(x => x.PartId))
                .ForMember(x => x.PartName, options => options.MapFrom(x => x.PartName))
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.ProjectId, options => options.MapFrom(x => x.ProjectId))
                .ForMember(x => x.ProjectPartAssignmentId, options => options.MapFrom(x => x.ProjectPartAssignmentId))
                .ForMember(x => x.Quantity, options => options.MapFrom(x => x.Quantity))
                .ForMember(x => x.QuantityAvailable, options => options.MapFrom(x => x.QuantityAvailable))
                .ForMember(x => x.ReferenceId, options => options.MapFrom(x => x.ReferenceId))
                .ForMember(x => x.Cost, options => options.MapFrom(x => x.Cost))
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.CustomDescription, options => options.MapFrom(x => x.CustomDescription))
                .ForMember(x => x.SchematicReferenceId, options => options.MapFrom(x => x.SchematicReferenceId))
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.Part, options => options.Ignore())
                .ForMember(x => x.Project, options => options.Ignore())
#if INITIALCREATE
                .ForMember(x => x.User, options => options.Ignore())
#endif
                ;

            CreateMap<DataModel.ProjectPartAssignment, ProjectPartAssignment>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.Notes, options => options.MapFrom(x => x.Notes))
                .ForMember(x => x.PartId, options => options.MapFrom(x => x.PartId))
                .ForMember(x => x.PartName, options => options.MapFrom(x => x.PartName))
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.ProjectId, options => options.MapFrom(x => x.ProjectId))
                .ForMember(x => x.ProjectPartAssignmentId, options => options.MapFrom(x => x.ProjectPartAssignmentId))
                .ForMember(x => x.Quantity, options => options.MapFrom(x => x.Quantity))
                .ForMember(x => x.QuantityAvailable, options => options.MapFrom(x => x.QuantityAvailable))
                .ForMember(x => x.ReferenceId, options => options.MapFrom(x => x.ReferenceId))
                .ForMember(x => x.Cost, options => options.MapFrom(x => x.Cost))
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Currency))
                .ForMember(x => x.CustomDescription, options => options.MapFrom(x => x.CustomDescription))
                .ForMember(x => x.SchematicReferenceId, options => options.MapFrom(x => x.SchematicReferenceId))
                //.ForMember(x => x.Part, options => options.MapFrom(x => x.Part))
                ;
        }
    }
}
