using AutoMapper;
using Binner.Common.IO.Printing;
using Binner.Common.Models;
using Binner.Common.Models.Configuration;
using Binner.Common.Models.Requests;
using Binner.Common.Models.Responses;
using System.Collections.Generic;
using System.Linq;

namespace Binner.Web.Configuration.MappingProfiles
{
    public class SettingsRequestProfile : Profile
    {
        public SettingsRequestProfile()
        {
            CreateMap<SettingsRequest, WebHostServiceConfiguration>(MemberList.None)
                .ForMember(x => x.Integrations, options => options.MapFrom(x => new IntegrationConfiguration
                {
                    Digikey = x.Digikey,
                    Mouser = new MouserConfiguration
                    {
                        ApiKeys = new MouserApiKeys
                        {
                            CartApiKey = x.Mouser.CartApiKey,
                            OrderApiKey = x.Mouser.OrderApiKey,
                            SearchApiKey = x.Mouser.SearchApiKey
                        }
                    },
                    Octopart = x.Octopart
                }))
                .ForMember(x => x.PrinterConfiguration, options => options.MapFrom(x => x.Printer))
                .ForMember(x => x.MaxCacheItems, options => options.Ignore())
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.Integrations, options => options.Ignore())
                .ForMember(x => x.CorsAllowOrigin, options => options.Ignore())
                .ForMember(x => x.Environment, options => options.Ignore())
                .ForMember(x => x.PublicUrl, options => options.Ignore())
                .ForMember(x => x.Port, options => options.Ignore())
                .ForMember(x => x.Name, options => options.Ignore())
                .ForMember(x => x.IP, options => options.Ignore())
                ;

            CreateMap<SettingsRequest, PrinterConfiguration>(MemberList.None)
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.Printer.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.Printer.PartLabelSource))
                .ForMember(x => x.PrinterName, options => options.MapFrom(x => x.Printer.PrinterName))
                // complex mapping situation
                .ForMember(x => x.PartLabelTemplate, options =>
                    options.MapFrom(x => new PartLabelTemplate
                    {
                        Line1 = x.Printer.Lines.Skip(0).Select(z => new LineConfiguration(z, LabelLines.Line1)).First(),
                        Line2 = x.Printer.Lines.Skip(1).Select(z => new LineConfiguration(z, LabelLines.Line2)).First(),
                        Line3 = x.Printer.Lines.Skip(2).Select(z => new LineConfiguration(z, LabelLines.Line3)).First(),
                        Line4 = x.Printer.Lines.Skip(3).Select(z => new LineConfiguration(z, LabelLines.Line4)).First(),
                        Identifier = x.Printer.Lines.Skip(4).Select(z => new LineConfiguration(z, LabelLines.Identifier1)).First(),
                        Identifier2 = x.Printer.Lines.Skip(5).Select(z => new LineConfiguration(z, LabelLines.Identifier2)).First(),
                    })
                );
        }
    }
}
