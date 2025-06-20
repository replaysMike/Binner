using AutoMapper;
using Binner.Model;
using Binner.Model.Responses;
using Binner.StorageProvider.EntityFrameworkCore;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class PartTypeProfile : Profile
    {
        public PartTypeProfile()
        {
            CreateMap<DataModel.PartType, PartType>(MemberList.None)
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.ParentPartTypeId, options => options.MapFrom(x => x.ParentPartTypeId))
                .ForMember(x => x.PartTypeId, options => options.MapFrom(x => x.PartTypeId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.ReferenceDesignator, options => options.MapFrom(x => x.ReferenceDesignator))
                .ForMember(x => x.SymbolId, options => options.MapFrom(x => x.SymbolId))
                .ForMember(x => x.Keywords, options => options.MapFrom(x => x.Keywords))
                .ForMember(x => x.Icon, options => options.MapFrom(x => x.Icon))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.CustomFields, options => options.Ignore()) // mapped manually
                ;

            CreateMap<PartType, DataModel.PartType>(MemberList.None)
                .ForMember(x => x.ParentPartTypeId, options => options.MapFrom(x => x.ParentPartTypeId))
                .ForMember(x => x.PartTypeId, options => options.MapFrom(x => x.PartTypeId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.ReferenceDesignator, options => options.MapFrom(x => x.ReferenceDesignator))
                .ForMember(x => x.SymbolId, options => options.MapFrom(x => x.SymbolId))
                .ForMember(x => x.Keywords, options => options.MapFrom(x => x.Keywords))
                .ForMember(x => x.Icon, options => options.MapFrom(x => x.Icon))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
                .ForMember(x => x.ParentPartType, options => options.Ignore())
                .ForMember(x => x.Parts, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                ;

            CreateMap<DataModel.PartType, PartTypeResponse>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.ParentPartTypeId, options => options.MapFrom(x => x.ParentPartTypeId))
                .ForMember(x => x.PartTypeId, options => options.MapFrom(x => x.PartTypeId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Icon, options => options.MapFrom(x => x.Icon))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.ReferenceDesignator, options => options.MapFrom(x => x.ReferenceDesignator))
                .ForMember(x => x.SymbolId, options => options.MapFrom(x => x.SymbolId))
                .ForMember(x => x.Keywords, options => options.MapFrom(x => x.Keywords))
                .ForMember(x => x.Parts, options => options.MapFrom(x => x.Parts != null ? x.Parts.Count : 0))
                .ForMember(x => x.CustomFields, options => options.Ignore()) // mapped manually
                ;

            CreateMap<PartType, CachedPartTypeResponse>()
                .ForMember(x => x.ParentPartTypeId, options => options.MapFrom(x => x.ParentPartTypeId))
                .ForMember(x => x.PartTypeId, options => options.MapFrom(x => x.PartTypeId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.ReferenceDesignator, options => options.MapFrom(x => x.ReferenceDesignator))
                .ForMember(x => x.SymbolId, options => options.MapFrom(x => x.SymbolId))
                .ForMember(x => x.Keywords, options => options.MapFrom(x => x.Keywords))
                .ForMember(x => x.Icon, options => options.MapFrom(x => x.Icon))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.Parts, options => options.Ignore())
                .ForMember(x => x.IsSystem, options => options.MapFrom(x => x.UserId == null))
                .ForMember(x => x.ParentPartType, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.MapFrom(x => x.CustomFields))
                ;
            CreateMap<CachedPartTypeResponse, PartType>()
                .ForMember(x => x.ParentPartTypeId, options => options.MapFrom(x => x.ParentPartTypeId))
                .ForMember(x => x.PartTypeId, options => options.MapFrom(x => x.PartTypeId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.ReferenceDesignator, options => options.MapFrom(x => x.ReferenceDesignator))
                .ForMember(x => x.SymbolId, options => options.MapFrom(x => x.SymbolId))
                .ForMember(x => x.Keywords, options => options.MapFrom(x => x.Keywords))
                .ForMember(x => x.Icon, options => options.MapFrom(x => x.Icon))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.CustomFields, options => options.MapFrom(x => x.CustomFields))
                ;
            CreateMap<CachedPartTypeResponse, PartTypeResponse>()
                .ForMember(x => x.ParentPartTypeId, options => options.MapFrom(x => x.ParentPartTypeId))
                .ForMember(x => x.PartTypeId, options => options.MapFrom(x => x.PartTypeId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.ReferenceDesignator, options => options.MapFrom(x => x.ReferenceDesignator))
                .ForMember(x => x.Keywords, options => options.MapFrom(x => x.Keywords))
                .ForMember(x => x.SymbolId, options => options.MapFrom(x => x.SymbolId))
                .ForMember(x => x.Icon, options => options.MapFrom(x => x.Icon))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.Parts, options => options.MapFrom(x => x.Parts))
                .ForMember(x => x.ParentPartType, options => options.MapFrom(x => x.ParentPartType != null ? x.ParentPartType.Name : null))
                .ForMember(x => x.CustomFields, options => options.MapFrom(x => x.CustomFields))
                ;
        }
    }
}
