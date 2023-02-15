using AutoMapper;
using Binner.Common.Configuration;
using Binner.Common.IO.Printing;
using Binner.Common.Models;
using Binner.Common.Models.Configuration;
using Binner.Common.Models.Configuration.Integrations;
using Binner.Common.Models.Requests;
using Binner.Common.Models.Responses;
using System.Linq;

namespace Binner.Common.MappingProfiles
{
    public class SettingsRequestProfile : Profile
    {
        public SettingsRequestProfile()
        {
            CreateMap<WebHostServiceConfiguration, SettingsRequest>()
                .ForMember(x => x.Binner, options => options.MapFrom(x => x.Integrations.Swarm))
                .ForMember(x => x.Digikey, options => options.MapFrom(x => x.Integrations.Digikey))
                .ForMember(x => x.Mouser, options => options.MapFrom(x => x.Integrations.Mouser))
                .ForMember(x => x.Octopart, options => options.MapFrom(x => x.Integrations.Octopart))
                .ForMember(x => x.Printer, options => options.MapFrom(x => x.PrinterConfiguration))
                .ReverseMap();

            CreateMap<SettingsRequest, UserIntegrationConfiguration>(MemberList.None)
                .ForMember(x => x.SwarmEnabled, options => options.MapFrom(x => x.Binner.Enabled))
                .ForMember(x => x.SwarmApiKey, options => options.MapFrom(x => x.Binner.ApiKey))
                .ForMember(x => x.SwarmApiUrl, options => options.MapFrom(x => x.Binner.ApiUrl))
                .ForMember(x => x.DigiKeyEnabled, options => options.MapFrom(x => x.Digikey.Enabled))
                .ForMember(x => x.DigiKeyClientId, options => options.MapFrom(x => x.Digikey.ClientId))
                .ForMember(x => x.DigiKeyClientSecret, options => options.MapFrom(x => x.Digikey.ClientSecret))
                .ForMember(x => x.DigiKeyApiUrl, options => options.MapFrom(x => x.Digikey.ApiUrl))
                .ForMember(x => x.MouserEnabled, options => options.MapFrom(x => x.Mouser.Enabled))
                .ForMember(x => x.MouserCartApiKey, options => options.MapFrom(x => x.Mouser.CartApiKey))
                .ForMember(x => x.MouserOrderApiKey, options => options.MapFrom(x => x.Mouser.OrderApiKey))
                .ForMember(x => x.MouserSearchApiKey, options => options.MapFrom(x => x.Mouser.SearchApiKey))
                .ForMember(x => x.MouserApiUrl, options => options.MapFrom(x => x.Mouser.ApiUrl))
                .ForMember(x => x.OctopartEnabled, options => options.MapFrom(x => x.Octopart.Enabled))
                .ForMember(x => x.OctopartApiKey, options => options.MapFrom(x => x.Octopart.ApiKey))
                .ForMember(x => x.OctopartApiUrl, options => options.MapFrom(x => x.Octopart.ApiUrl))
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore());

            CreateMap<SettingsRequest, PrinterConfiguration>(MemberList.None)
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.Printer.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.Printer.PartLabelSource))
                .ForMember(x => x.PrinterName, options => options.MapFrom(x => x.Printer.PrinterName))
                // complex mapping situation
                .ForMember(x => x.PartLabelTemplate, options =>
                    options.MapFrom(x => new PartLabelTemplate
                    {
                        Line1 = x.Printer.Lines != null ? x.Printer.Lines.Skip(0).Select(z => new LineConfiguration(z, LabelLines.Line1)).First() : new LineConfiguration(),
                        Line2 = x.Printer.Lines != null ? x.Printer.Lines.Skip(1).Select(z => new LineConfiguration(z, LabelLines.Line2)).First() : new LineConfiguration(),
                        Line3 = x.Printer.Lines != null ? x.Printer.Lines.Skip(2).Select(z => new LineConfiguration(z, LabelLines.Line3)).First() : new LineConfiguration(),
                        Line4 = x.Printer.Lines != null ? x.Printer.Lines.Skip(3).Select(z => new LineConfiguration(z, LabelLines.Line4)).First() : new LineConfiguration(),
                        Identifier = x.Printer.Lines != null ? x.Printer.Lines.Skip(4).Select(z => new LineConfiguration(z, LabelLines.Identifier1)).First() : new LineConfiguration(),
                        Identifier2 = x.Printer.Lines != null ? x.Printer.Lines.Skip(5).Select(z => new LineConfiguration(z, LabelLines.Identifier2)).First() : new LineConfiguration(),
                    })
                );
        }
    }
}
