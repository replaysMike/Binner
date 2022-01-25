using AutoMapper;
using Binner.Common.IO.Printing;
using Binner.Common.Models.Configuration;
using Binner.Common.Models.Responses;
using System.Collections.Generic;

namespace Binner.Web.Configuration.MappingProfiles
{
    public class SettingsResponseProfile : Profile
    {
        public SettingsResponseProfile()
        {
            CreateMap<WebHostServiceConfiguration, SettingsResponse>()
                .ForMember(x => x.Digikey, options => options.MapFrom(x => x.Integrations.Digikey))
                .ForMember(x => x.Mouser, options => options.MapFrom(x => x.Integrations.Mouser))
                .ForMember(x => x.Octopart, options => options.MapFrom(x => x.Integrations.Octopart))
                .ForMember(x => x.Printer, options => options.MapFrom(x => x.PrinterConfiguration))
                .ReverseMap();

            CreateMap<MouserConfiguration, MouserConfigurationResponse>()
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.ApiUrl))
                .ForMember(x => x.OrderApiKey, options => options.MapFrom(x => x.ApiKeys.OrderApiKey))
                .ForMember(x => x.SearchApiKey, options => options.MapFrom(x => x.ApiKeys.SearchApiKey))
                .ForMember(x => x.CartApiKey, options => options.MapFrom(x => x.ApiKeys.CartApiKey))
                .ReverseMap();

            CreateMap<PrinterConfiguration, PrinterSettingsResponse>()
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.PartLabelSource))
                .ForMember(x => x.PrinterName, options => options.MapFrom(x => x.PrinterName))
                .ForMember(x => x.Lines, options => options.MapFrom(x => new List<LineConfiguration> { x.PartLabelTemplate.Line1, x.PartLabelTemplate.Line2, x.PartLabelTemplate.Line3, x.PartLabelTemplate.Line4 }))
                .ForMember(x => x.Identifiers, options => options.MapFrom(x => new List<LineConfiguration> { x.PartLabelTemplate.Identifier, x.PartLabelTemplate.Identifier2 }))
                .ReverseMap(); // this probably won't work here
        }
    }
}
