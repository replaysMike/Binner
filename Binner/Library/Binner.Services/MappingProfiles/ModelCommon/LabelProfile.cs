using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class LabelProfile : Profile
    {
        public LabelProfile()
        {
            CreateMap<Label, DataModel.Label>()
                .ForMember(x => x.LabelTemplateId, options => options.MapFrom(x => x.LabelTemplateId))
                .ForMember(x => x.IsPartLabelTemplate, options => options.MapFrom(x => x.IsPartLabelTemplate))
                .ForMember(x => x.LabelTemplate, options => options.MapFrom(x => x.LabelTemplate))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Template, options => options.MapFrom(x => x.Template))
                .ForMember(x => x.LabelId, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                ;

            CreateMap<DataModel.Label, Label>()
                .ForMember(x => x.LabelId, options => options.MapFrom(x => x.LabelId))
                .ForMember(x => x.LabelTemplateId, options => options.MapFrom(x => x.LabelTemplateId))
                .ForMember(x => x.IsPartLabelTemplate, options => options.MapFrom(x => x.IsPartLabelTemplate))
                .ForMember(x => x.LabelTemplate, options => options.MapFrom(x => x.LabelTemplate))
                .ForMember(x => x.Template, options => options.MapFrom(x => x.Template))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.CustomFields, options => options.Ignore()) // mapped manually
                ;
        }
    }
}
