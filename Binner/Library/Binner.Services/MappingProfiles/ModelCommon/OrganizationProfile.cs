using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles
{
    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<DataModel.Organization, Organization>()
                .ForMember(x => x.OrganizationId, options => options.MapFrom(x => x.OrganizationId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.DateLockedUtc, options => options.MapFrom(x => x.DateLockedUtc))
                .ForMember(x => x.GlobalId, options => options.MapFrom(x => x.GlobalId))
            ;

            CreateMap<Organization, DataModel.Organization>()
                .ForMember(x => x.OrganizationId, options => options.MapFrom(x => x.OrganizationId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.DateLockedUtc, options => options.MapFrom(x => x.DateLockedUtc))

                .ForMember(x => x.Users, options => options.Ignore())
                .ForMember(x => x.OrganizationConfigurations, options => options.Ignore())
                .ForMember(x => x.OrganizationIntegrationConfigurations, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.GlobalId, options => options.MapFrom(x => x.GlobalId))
            ;
        }
    }
}
