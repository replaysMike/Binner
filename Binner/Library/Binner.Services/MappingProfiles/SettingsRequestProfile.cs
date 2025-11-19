using AutoMapper;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Binner.Model.IO.Printing;
using Binner.Model.Requests;
using Newtonsoft.Json;

namespace Binner.Services.MappingProfiles
{
    public class SettingsRequestProfile : Profile
    {
        public SettingsRequestProfile()
        {
            CreateMap<WebHostServiceConfiguration, SettingsRequest>()
                .ForMember(x => x.Binner, options => options.MapFrom(x => x.Integrations.Swarm))
                .ForMember(x => x.Digikey, options => options.MapFrom(x => x.Integrations.Digikey))
                .ForMember(x => x.Mouser, options => options.MapFrom(x => x.Integrations.Mouser))
                .ForMember(x => x.Arrow, options => options.MapFrom(x => x.Integrations.Arrow))
                .ForMember(x => x.Octopart, options => options.MapFrom(x => x.Integrations.Nexar))
                .ForMember(x => x.Tme, options => options.MapFrom(x => x.Integrations.Tme))
                .ForMember(x => x.Element14, options => options.MapFrom(x => x.Integrations.Element14))
                .ForMember(x => x.Printer, options => options.MapFrom(x => x.PrinterConfiguration))
                .ForMember(x => x.Barcode, options => options.MapFrom(x => x.Barcode))
                .ForMember(x => x.Locale, options => options.MapFrom(x => x.Locale))
                .ForMember(x => x.LicenseKey, options => options.MapFrom(x => x.Licensing != null ? x.Licensing.LicenseKey : ""))
                .ForMember(x => x.MaxCacheItems, options => options.MapFrom(x => x.MaxCacheItems))
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.MapFrom(x => x.CacheSlidingExpirationMinutes))
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.MapFrom(x => x.CacheAbsoluteExpirationMinutes))

                .ForMember(x => x.UseModule, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.KiCad, options => options.Ignore())
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.EnableCheckNewVersion, options => options.Ignore())
                .ReverseMap();

            CreateMap<SettingsRequest, UserIntegrationConfiguration>(MemberList.None)
                .ForMember(x => x.SwarmEnabled, options => options.MapFrom(x => x.Binner.Enabled))
                .ForMember(x => x.SwarmApiKey, options => options.MapFrom(x => x.Binner.ApiKey))
                .ForMember(x => x.SwarmApiUrl, options => options.MapFrom(x => x.Binner.ApiUrl))
                .ForMember(x => x.SwarmTimeout, options => options.MapFrom(x => x.Binner.Timeout))
                .ForMember(x => x.DigiKeyEnabled, options => options.MapFrom(x => x.Digikey.Enabled))
                .ForMember(x => x.DigiKeySite, options => options.MapFrom(x => x.Digikey.Site))
                .ForMember(x => x.DigiKeyClientId, options => options.MapFrom(x => x.Digikey.ClientId))
                .ForMember(x => x.DigiKeyClientSecret, options => options.MapFrom(x => x.Digikey.ClientSecret))
                .ForMember(x => x.DigiKeyOAuthPostbackUrl, options => options.MapFrom(x => x.Digikey.oAuthPostbackUrl))
                .ForMember(x => x.DigiKeyApiUrl, options => options.MapFrom(x => x.Digikey.ApiUrl))
                .ForMember(x => x.MouserEnabled, options => options.MapFrom(x => x.Mouser.Enabled))
                .ForMember(x => x.MouserSearchApiKey, options => options.MapFrom(x => x.Mouser.SearchApiKey))
                .ForMember(x => x.MouserCartApiKey, options => options.MapFrom(x => x.Mouser.CartApiKey))
                .ForMember(x => x.MouserOrderApiKey, options => options.MapFrom(x => x.Mouser.OrderApiKey))
                .ForMember(x => x.MouserApiUrl, options => options.MapFrom(x => x.Mouser.ApiUrl))
                .ForMember(x => x.ArrowEnabled, options => options.MapFrom(x => x.Arrow.Enabled))
                .ForMember(x => x.ArrowUsername, options => options.MapFrom(x => x.Arrow.Username))
                .ForMember(x => x.ArrowApiKey, options => options.MapFrom(x => x.Arrow.ApiKey))
                .ForMember(x => x.ArrowApiUrl, options => options.MapFrom(x => x.Arrow.ApiUrl))
                .ForMember(x => x.NexarEnabled, options => options.MapFrom(x => x.Octopart.Enabled))
                .ForMember(x => x.NexarClientId, options => options.MapFrom(x => x.Octopart.ClientId))
                .ForMember(x => x.NexarClientSecret, options => options.MapFrom(x => x.Octopart.ClientSecret))
                .ForMember(x => x.TmeEnabled, options => options.MapFrom(x => x.Tme.Enabled))
                .ForMember(x => x.TmeCountry, options => options.MapFrom(x => x.Tme.Country))
                .ForMember(x => x.TmeApplicationSecret, options => options.MapFrom(x => x.Tme.ApplicationSecret))
                .ForMember(x => x.TmeApiKey, options => options.MapFrom(x => x.Tme.ApiKey))
                .ForMember(x => x.TmeApiUrl, options => options.MapFrom(x => x.Tme.ApiUrl))
                .ForMember(x => x.TmeResolveExternalLinks, options => options.MapFrom(x => x.Tme.ResolveExternalLinks))
                .ForMember(x => x.Element14Enabled, options => options.MapFrom(x => x.Element14.Enabled))
                .ForMember(x => x.Element14Country, options => options.MapFrom(x => x.Element14.Country))
                .ForMember(x => x.Element14ApiKey, options => options.MapFrom(x => x.Element14.ApiKey))
                .ForMember(x => x.Element14ApiUrl, options => options.MapFrom(x => x.Element14.ApiUrl))

                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ReverseMap();

            CreateMap<SettingsRequest, UserPrinterConfiguration>()
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.Printer.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.Printer.PartLabelSource))
                .ForMember(x => x.PrinterName, options => options.MapFrom(x => x.Printer.PrinterName))
                .ForMember(x => x.RemoteAddressUrl, options => options.MapFrom(x => x.Printer.RemoteAddressUrl))
                .ForMember(x => x.PrintMode, options => options.MapFrom(x => x.Printer.PrintMode))

                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.UserPrinterTemplateConfigurations, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ReverseMap();

            CreateMap<SettingsRequest, OrganizationIntegrationConfiguration>()
                .ForMember(x => x.SwarmEnabled, options => options.MapFrom(x => x.Binner.Enabled))
                .ForMember(x => x.SwarmApiKey, options => options.MapFrom(x => x.Binner.ApiKey))
                .ForMember(x => x.SwarmApiUrl, options => options.MapFrom(x => x.Binner.ApiUrl))
                .ForMember(x => x.SwarmTimeout, options => options.MapFrom(x => x.Binner.Timeout))
                .ForMember(x => x.DigiKeyEnabled, options => options.MapFrom(x => x.Digikey.Enabled))
                .ForMember(x => x.DigiKeySite, options => options.MapFrom(x => x.Digikey.Site))
                .ForMember(x => x.DigiKeyClientId, options => options.MapFrom(x => x.Digikey.ClientId))
                .ForMember(x => x.DigiKeyClientSecret, options => options.MapFrom(x => x.Digikey.ClientSecret))
                .ForMember(x => x.DigiKeyOAuthPostbackUrl, options => options.MapFrom(x => x.Digikey.oAuthPostbackUrl))
                .ForMember(x => x.DigiKeyApiUrl, options => options.MapFrom(x => x.Digikey.ApiUrl))
                .ForMember(x => x.MouserEnabled, options => options.MapFrom(x => x.Mouser.Enabled))
                .ForMember(x => x.MouserSearchApiKey, options => options.MapFrom(x => x.Mouser.SearchApiKey))
                .ForMember(x => x.MouserCartApiKey, options => options.MapFrom(x => x.Mouser.CartApiKey))
                .ForMember(x => x.MouserOrderApiKey, options => options.MapFrom(x => x.Mouser.OrderApiKey))
                .ForMember(x => x.MouserApiUrl, options => options.MapFrom(x => x.Mouser.ApiUrl))
                .ForMember(x => x.ArrowEnabled, options => options.MapFrom(x => x.Arrow.Enabled))
                .ForMember(x => x.ArrowUsername, options => options.MapFrom(x => x.Arrow.Username))
                .ForMember(x => x.ArrowApiKey, options => options.MapFrom(x => x.Arrow.ApiKey))
                .ForMember(x => x.ArrowApiUrl, options => options.MapFrom(x => x.Arrow.ApiUrl))
                .ForMember(x => x.NexarEnabled, options => options.MapFrom(x => x.Octopart.Enabled))
                .ForMember(x => x.NexarClientId, options => options.MapFrom(x => x.Octopart.ClientId))
                .ForMember(x => x.NexarClientSecret, options => options.MapFrom(x => x.Octopart.ClientSecret))
                .ForMember(x => x.TmeEnabled, options => options.MapFrom(x => x.Tme.Enabled))
                .ForMember(x => x.TmeCountry, options => options.MapFrom(x => x.Tme.Country))
                .ForMember(x => x.TmeApplicationSecret, options => options.MapFrom(x => x.Tme.ApplicationSecret))
                .ForMember(x => x.TmeApiKey, options => options.MapFrom(x => x.Tme.ApiKey))
                .ForMember(x => x.TmeApiUrl, options => options.MapFrom(x => x.Tme.ApiUrl))
                .ForMember(x => x.TmeResolveExternalLinks, options => options.MapFrom(x => x.Tme.ResolveExternalLinks))
                .ForMember(x => x.Element14Enabled, options => options.MapFrom(x => x.Element14.Enabled))
                .ForMember(x => x.Element14Country, options => options.MapFrom(x => x.Element14.Country))
                .ForMember(x => x.Element14ApiKey, options => options.MapFrom(x => x.Element14.ApiKey))
                .ForMember(x => x.Element14ApiUrl, options => options.MapFrom(x => x.Element14.ApiUrl))

                .ReverseMap();

            CreateMap<SettingsRequest, UserConfiguration>()
                .ForMember(x => x.BarcodeBufferTime, options => options.MapFrom(x => x.Barcode.BufferTime))
                .ForMember(x => x.BarcodeEnabled, options => options.MapFrom(x => x.Barcode.Enabled))
                .ForMember(x => x.BarcodeIsDebug, options => options.MapFrom(x => x.Barcode.IsDebug))
                .ForMember(x => x.BarcodeMaxKeystrokeThresholdMs, options => options.MapFrom(x => x.Barcode.MaxKeystrokeThresholdMs))
                .ForMember(x => x.BarcodePrefix2D, options => options.MapFrom(x => x.Barcode.Prefix2D))
                .ForMember(x => x.BarcodeProfile, options => options.MapFrom(x => x.Barcode.Profile))
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Locale.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Locale.Language))
                .ForMember(x => x.EnableAutoPartSearch, options => options.MapFrom(x => x.EnableAutoPartSearch))
                .ForMember(x => x.EnableDarkMode, options => options.MapFrom(x => x.EnableDarkMode))
                .ForMember(x => x.EnableCheckNewVersion, options => options.MapFrom(x => x.EnableCheckNewVersion))

                .ForMember(x => x.DefaultPartLabelId, options => options.Ignore())
                .ReverseMap();

            CreateMap<SettingsRequest, OrganizationConfiguration>()
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.MapFrom(x => x.CacheAbsoluteExpirationMinutes))
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.MapFrom(x => x.CacheSlidingExpirationMinutes))
                .ForMember(x => x.LicenseKey, options => options.MapFrom(x => x.LicenseKey))
                .ForMember(x => x.MaxCacheItems, options => options.MapFrom(x => x.MaxCacheItems))
                .ForMember(x => x.UseModule, options => options.MapFrom(x => x.UseModule))
                .ForMember(x => x.KiCad, options => options.MapFrom(x => x.KiCad))
            ;

            CreateMap<OrganizationConfiguration, SettingsRequest>()
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.MapFrom(x => x.CacheAbsoluteExpirationMinutes))
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.MapFrom(x => x.CacheSlidingExpirationMinutes))
                .ForMember(x => x.LicenseKey, options => options.MapFrom(x => x.LicenseKey))
                .ForMember(x => x.MaxCacheItems, options => options.MapFrom(x => x.MaxCacheItems))
                .ForMember(x => x.UseModule, options => options.MapFrom(x => x.UseModule))
                .ForMember(x => x.KiCad, options => options.MapFrom(x => x.KiCad))

                .ForMember(x => x.Octopart, options => options.Ignore())
                .ForMember(x => x.Digikey, options => options.Ignore())
                .ForMember(x => x.Mouser, options => options.Ignore())
                .ForMember(x => x.Arrow, options => options.Ignore())
                .ForMember(x => x.Tme, options => options.Ignore())
                .ForMember(x => x.Element14, options => options.Ignore())
                .ForMember(x => x.Binner, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.Printer, options => options.Ignore())
                .ForMember(x => x.Barcode, options => options.Ignore())
                .ForMember(x => x.Locale, options => options.Ignore())
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.EnableCheckNewVersion, options => options.Ignore())
            ;

            CreateMap<SettingsRequest, UserBarcodeConfiguration>(MemberList.None)
                .ForMember(x => x.BufferTime, options => options.MapFrom(x => x.Barcode.BufferTime))
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Barcode.Enabled))
                .ForMember(x => x.IsDebug, options => options.MapFrom(x => x.Barcode.IsDebug))
                .ForMember(x => x.MaxKeystrokeThresholdMs, options => options.MapFrom(x => x.Barcode.MaxKeystrokeThresholdMs))
                .ForMember(x => x.Prefix2D, options => options.MapFrom(x => x.Barcode.Prefix2D))
                .ForMember(x => x.Profile, options => options.MapFrom(x => x.Barcode.Profile))

                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ReverseMap();

            CreateMap<SettingsRequest, UserLocaleConfiguration>(MemberList.None)
                .ForMember(x => x.Currency, options => options.MapFrom(x => x.Locale.Currency))
                .ForMember(x => x.Language, options => options.MapFrom(x => x.Locale.Language))

                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ReverseMap();

            CreateMap<SettingsRequest, KiCadSettings>(MemberList.None)
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.KiCad.Enabled))
                .ForMember(x => x.ExportFields, options => options.MapFrom(x => x.KiCad.ExportFields))
                .ReverseMap();

            CreateMap<SettingsRequest, PrinterConfiguration>(MemberList.None)
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.Printer.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.Printer.PartLabelSource))
                .ForMember(x => x.PrintMode, options => options.MapFrom(x => x.Printer.PrintMode))
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
                )
                .ReverseMap();
        }
    }
}
