using AutoMapper;
using Binner.Global.Common;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class CustomFieldValueProfile : Profile
    {
        public CustomFieldValueProfile()
        {
            CreateMap<CustomFieldValue, DataModel.CustomFieldValue>()
                .ForMember(x => x.CustomFieldId, options => options.MapFrom(x => x.CustomFieldId))
                .ForMember(x => x.CustomFieldTypeId, options => options.MapFrom(x => x.CustomFieldTypeId))
                .ForMember(x => x.RecordId, options => options.MapFrom(x => x.RecordId))
                .ForMember(x => x.Value, options => options.MapFrom(x => x.Value))
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
                .ForMember(x => x.CustomField, options => options.Ignore())
                .ReverseMap()
                ;
        }
    }
}
