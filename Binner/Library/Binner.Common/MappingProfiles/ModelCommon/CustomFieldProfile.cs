using AutoMapper;
using System;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class CustomFieldProfile : Profile
    {
        public CustomFieldProfile()
        {
            CreateMap<CustomField, DataModel.CustomField>()
                .ForMember(x => x.CustomFieldId, options => options.MapFrom(x => x.CustomFieldId))
                .ForMember(x => x.CustomFieldTypeId, options => options.MapFrom(x => x.CustomFieldTypeId))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
                .ForMember(x => x.CustomFieldValues, options => options.Ignore())
                .ReverseMap()
                ;
        }
    }
}
