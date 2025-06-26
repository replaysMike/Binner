using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            CreateMap<DataModel.Project, Project>()
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.Location, options => options.MapFrom(x => x.Location))
                .ForMember(x => x.ProjectId, options => options.MapFrom(x => x.ProjectId))
                .ForMember(x => x.Color, options => options.MapFrom(x => x.Color))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.Notes, options => options.MapFrom(x => x.Notes))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.CustomFields, options => options.Ignore()) // mapped manually
                .ReverseMap();
        }
    }
}
