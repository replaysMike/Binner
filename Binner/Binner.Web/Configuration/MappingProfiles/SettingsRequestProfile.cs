using AutoMapper;
using Binner.Common.Models.Configuration;
using Binner.Common.Models.Requests;

namespace Binner.Web.Configuration.MappingProfiles
{
    public class SettingsRequestProfile : Profile
    {
        public SettingsRequestProfile()
        {
            CreateMap<SettingsRequest, WebHostServiceConfiguration>(MemberList.None)
                .ForMember(x => x.Integrations, options => options.MapFrom(x => new IntegrationConfiguration { 
                    Digikey = x.Digikey,
                    Mouser = new MouserConfiguration {
                        ApiKeys = new MouserApiKeys
                        {
                            CartApiKey = x.Mouser.CartApiKey,
                            OrderApiKey = x.Mouser.OrderApiKey,
                            SearchApiKey = x.Mouser.SearchApiKey
                        }
                    },
                    Octopart = x.Octopart
                }))
                .ForMember(x => x.PrinterConfiguration, options => options.MapFrom(x => x.Printer));
        }
    }
}
