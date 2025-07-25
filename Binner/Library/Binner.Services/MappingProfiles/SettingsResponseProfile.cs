﻿using AutoMapper;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Binner.Model.IO.Printing;
using Binner.Model.Responses;
using Newtonsoft.Json;

namespace Binner.Services.MappingProfiles
{
    public class SettingsResponseProfile : Profile
    {
        public SettingsResponseProfile()
        {
            CreateMap<WebHostServiceConfiguration, SettingsResponse>()
                .ForMember(x => x.Binner, options => options.MapFrom(x => x.Integrations.Swarm))
                .ForMember(x => x.Digikey, options => options.MapFrom(x => x.Integrations.Digikey))
                .ForMember(x => x.Mouser, options => options.MapFrom(x => x.Integrations.Mouser))
                .ForMember(x => x.Arrow, options => options.MapFrom(x => x.Integrations.Arrow))
                .ForMember(x => x.Octopart, options => options.MapFrom(x => x.Integrations.Nexar))
                .ForMember(x => x.Tme, options => options.MapFrom(x => x.Integrations.Tme))
                .ForMember(x => x.LicenseKey, options => options.MapFrom(x => x.Licensing != null ? x.Licensing.LicenseKey : ""))
                .ForMember(x => x.MaxCacheItems, options => options.MapFrom(x => x.MaxCacheItems))
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.MapFrom(x => x.CacheSlidingExpirationMinutes))
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.MapFrom(x => x.CacheAbsoluteExpirationMinutes))

                .ForMember(x => x.UseModule, options => options.Ignore())
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.EnableCheckNewVersion, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.Locale, options => options.Ignore())
                .ForMember(x => x.Printer, options => options.Ignore())
                .ForMember(x => x.Barcode, options => options.Ignore())
                .ForMember(x => x.KiCad, options => options.Ignore())
                .ReverseMap();

            CreateMap<UserLocaleConfiguration, SettingsResponse>()
                .ForMember(x => x.Binner, options => options.Ignore())
                .ForMember(x => x.Digikey, options => options.Ignore())
                .ForMember(x => x.Mouser, options => options.Ignore())
                .ForMember(x => x.Arrow, options => options.Ignore())
                .ForMember(x => x.Octopart, options => options.Ignore())
                .ForMember(x => x.Tme, options => options.Ignore())
                .ForMember(x => x.Barcode, options => options.Ignore())
                .ForMember(x => x.KiCad, options => options.Ignore())
                .ForMember(x => x.UseModule, options => options.Ignore())
                .ForMember(x => x.LicenseKey, options => options.Ignore())
                .ForMember(x => x.MaxCacheItems, options => options.Ignore())
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.Printer, options => options.Ignore())
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.EnableCheckNewVersion, options => options.Ignore())
                .ForMember(x => x.Locale, options => options.MapFrom(x => new LocaleSettingsResponse
                {
                    Language = x.Language,
                    Currency = x.Currency
                }))
                .ReverseMap();

            CreateMap<UserConfiguration, SettingsResponse>()
                .ForMember(x => x.EnableAutoPartSearch, options => options.MapFrom(x => x.EnableAutoPartSearch))
                .ForMember(x => x.EnableDarkMode, options => options.MapFrom(x => x.EnableDarkMode))
                .ForMember(x => x.EnableCheckNewVersion, options => options.MapFrom(x => x.EnableCheckNewVersion))
                .ForMember(x => x.Barcode, options => options.MapFrom(x => new BarcodeConfiguration
                {
                    BufferTime = x.BarcodeBufferTime,
                    Enabled = x.BarcodeEnabled,
                    IsDebug = x.BarcodeIsDebug,
                    MaxKeystrokeThresholdMs = x.BarcodeMaxKeystrokeThresholdMs,
                    Prefix2D = x.BarcodePrefix2D,
                    Profile = x.BarcodeProfile
                }))
                .ForMember(x => x.Locale, options => options.MapFrom(x => new LocaleSettingsResponse
                {
                    Language = x.Language,
                    Currency = x.Currency
                }))

                .ForMember(x => x.UseModule, options => options.Ignore())
                .ForMember(x => x.LicenseKey, options => options.Ignore())
                .ForMember(x => x.Binner, options => options.Ignore())
                .ForMember(x => x.Digikey, options => options.Ignore())
                .ForMember(x => x.Mouser, options => options.Ignore())
                .ForMember(x => x.Arrow, options => options.Ignore())
                .ForMember(x => x.Octopart, options => options.Ignore())
                .ForMember(x => x.Tme, options => options.Ignore())
                .ForMember(x => x.MaxCacheItems, options => options.Ignore())
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.Printer, options => options.Ignore())
                .ForMember(x => x.KiCad, options => options.Ignore())
                .ReverseMap();

            CreateMap<OrganizationConfiguration, SettingsResponse>()
                .ForMember(x => x.LicenseKey, options => options.MapFrom(x => x.LicenseKey))
                .ForMember(x => x.MaxCacheItems, options => options.MapFrom(x => x.MaxCacheItems))
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.MapFrom(x => x.CacheSlidingExpirationMinutes))
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.MapFrom(x => x.CacheAbsoluteExpirationMinutes))
                .ForMember(x => x.KiCad, options => options.MapFrom(x => x.KiCad))

                .ForMember(x => x.UseModule, options => options.Ignore())
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.EnableCheckNewVersion, options => options.Ignore())
                .ForMember(x => x.Barcode, options => options.Ignore())
                .ForMember(x => x.Locale, options => options.Ignore())
                .ForMember(x => x.Binner, options => options.Ignore())
                .ForMember(x => x.Digikey, options => options.Ignore())
                .ForMember(x => x.Mouser, options => options.Ignore())
                .ForMember(x => x.Arrow, options => options.Ignore())
                .ForMember(x => x.Octopart, options => options.Ignore())
                .ForMember(x => x.Tme, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.Printer, options => options.Ignore())
                .ReverseMap();

            CreateMap<UserPrinterConfiguration, SettingsResponse>()
                .ForMember(x => x.UseModule, options => options.Ignore())
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.EnableCheckNewVersion, options => options.Ignore())
                .ForMember(x => x.Binner, options => options.Ignore())
                .ForMember(x => x.Digikey, options => options.Ignore())
                .ForMember(x => x.Mouser, options => options.Ignore())
                .ForMember(x => x.Arrow, options => options.Ignore())
                .ForMember(x => x.Octopart, options => options.Ignore())
                .ForMember(x => x.Tme, options => options.Ignore())
                .ForMember(x => x.Barcode, options => options.Ignore())
                .ForMember(x => x.Locale, options => options.Ignore())
                .ForMember(x => x.UseModule, options => options.Ignore())
                .ForMember(x => x.LicenseKey, options => options.Ignore())
                .ForMember(x => x.MaxCacheItems, options => options.Ignore())
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.KiCad, options => options.Ignore())

                .ForMember(x => x.Printer, options => options.MapFrom(x => new PrinterSettingsResponse
                {
                    Identifiers = x.UserPrinterTemplateConfigurations.Select(y => new Model.IO.Printing.LineConfiguration
                    {
                        AutoSize = y.AutoSize,
                        Barcode = y.Barcode,
                        Color = y.Color,
                        Content = y.Content,
                        FontName = y.FontName,
                        FontSize = y.FontSize,
                        Label = y.Label,
                        Line = y.Line,
                        LowerCase = y.LowerCase,
                        Margin = new Model.IO.Printing.Margin(y.MarginLeft, y.MarginRight, y.MarginTop, y.MarginBottom),
                        Position = y.Position,
                        Rotate = y.Rotate,
                        UpperCase = y.UpperCase,
                    }),
                    PrintMode = x.PrintMode,
                    PartLabelName = x.PartLabelName,
                    PartLabelSource = x.PartLabelSource,
                    PrinterName = x.PrinterName,
                    RemoteAddressUrl = x.RemoteAddressUrl,
                }))
                .ReverseMap();

            CreateMap<UserBarcodeConfiguration, SettingsResponse>()
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.EnableCheckNewVersion, options => options.Ignore())
                .ForMember(x => x.Binner, options => options.Ignore())
                .ForMember(x => x.Digikey, options => options.Ignore())
                .ForMember(x => x.Mouser, options => options.Ignore())
                .ForMember(x => x.Arrow, options => options.Ignore())
                .ForMember(x => x.Octopart, options => options.Ignore())
                .ForMember(x => x.Tme, options => options.Ignore())
                .ForMember(x => x.UseModule, options => options.Ignore())
                .ForMember(x => x.LicenseKey, options => options.Ignore())
                .ForMember(x => x.MaxCacheItems, options => options.Ignore())
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.Printer, options => options.Ignore())
                .ForMember(x => x.Locale, options => options.Ignore())
                .ForMember(x => x.KiCad, options => options.Ignore())

                .ForMember(x => x.Barcode, options => options.MapFrom(x => new BarcodeSettingsResponse
                {
                    BufferTime = x.BufferTime,
                    Enabled = x.Enabled,
                    IsDebug = x.IsDebug,
                    MaxKeystrokeThresholdMs = x.MaxKeystrokeThresholdMs,
                    Prefix2D = x.Prefix2D,
                    Profile = x.Profile
                }))
                .ReverseMap();

            CreateMap<UserIntegrationConfiguration, SettingsResponse>()
                .ForMember(x => x.Binner, options => options.MapFrom(x => new SwarmUserConfiguration
                {
                    Enabled = x.SwarmEnabled,
                    ApiKey = x.SwarmApiKey,
                    ApiUrl = x.SwarmApiUrl,
                    Timeout = x.SwarmTimeout
                }))
                .ForMember(x => x.Digikey, options => options.MapFrom(x => new DigiKeyUserConfiguration
                {
                    Enabled = x.DigiKeyEnabled,
                    Site = x.DigiKeySite,
                    ClientId = x.DigiKeyClientId,
                    ClientSecret = x.DigiKeyClientSecret,
                    ApiUrl = x.DigiKeyApiUrl,
                    oAuthPostbackUrl = x.DigiKeyOAuthPostbackUrl
                }))
                .ForMember(x => x.Mouser, options => options.MapFrom(x => new MouserUserConfiguration
                {
                    Enabled = x.MouserEnabled,
                    SearchApiKey = x.MouserSearchApiKey,
                    CartApiKey = x.MouserCartApiKey,
                    OrderApiKey = x.MouserOrderApiKey,
                    ApiUrl = x.MouserApiUrl
                }))
                .ForMember(x => x.Arrow, options => options.MapFrom(x => new ArrowUserConfiguration
                {
                    Enabled = x.ArrowEnabled,
                    Username = x.ArrowUsername,
                    ApiUrl = x.ArrowApiUrl,
                    ApiKey = x.ArrowApiKey
                }))
                .ForMember(x => x.Octopart, options => options.MapFrom(x => new OctopartUserConfiguration
                {
                    Enabled = x.NexarEnabled,
                    ClientId = x.NexarClientId,
                    ClientSecret = x.NexarClientSecret
                }))
                .ForMember(x => x.Tme, options => options.MapFrom(x => new TmeUserConfiguration
                {
                    Enabled = x.TmeEnabled,
                    Country = x.TmeCountry,
                    ApplicationSecret = x.TmeApplicationSecret,
                    ApiUrl = x.TmeApiUrl,
                    ApiKey = x.TmeApiKey,
                    ResolveExternalLinks = x.TmeResolveExternalLinks
                }))
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.EnableCheckNewVersion, options => options.Ignore())
                .ForMember(x => x.Printer, options => options.Ignore())
                .ForMember(x => x.Barcode, options => options.Ignore())
                .ForMember(x => x.Locale, options => options.Ignore())
                .ForMember(x => x.UseModule, options => options.Ignore())
                .ForMember(x => x.LicenseKey, options => options.Ignore())
                .ForMember(x => x.MaxCacheItems, options => options.Ignore())
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.KiCad, options => options.Ignore())
                .ReverseMap();

            CreateMap<OrganizationIntegrationConfiguration, SettingsResponse>()
                .ForMember(x => x.Binner, options => options.MapFrom(x => new SwarmUserConfiguration
                {
                    Enabled = x.SwarmEnabled,
                    ApiKey = x.SwarmApiKey,
                    ApiUrl = x.SwarmApiUrl,
                    Timeout = x.SwarmTimeout
                }))
                .ForMember(x => x.Digikey, options => options.MapFrom(x => new DigiKeyUserConfiguration
                {
                    Enabled = x.DigiKeyEnabled,
                    Site = x.DigiKeySite,
                    ClientId = x.DigiKeyClientId,
                    ClientSecret = x.DigiKeyClientSecret,
                    ApiUrl = x.DigiKeyApiUrl,
                    oAuthPostbackUrl = x.DigiKeyOAuthPostbackUrl
                }))
                .ForMember(x => x.Mouser, options => options.MapFrom(x => new MouserUserConfiguration
                {
                    Enabled = x.MouserEnabled,
                    SearchApiKey = x.MouserSearchApiKey,
                    CartApiKey = x.MouserCartApiKey,
                    OrderApiKey = x.MouserOrderApiKey,
                    ApiUrl = x.MouserApiUrl
                }))
                .ForMember(x => x.Arrow, options => options.MapFrom(x => new ArrowUserConfiguration
                {
                    Enabled = x.ArrowEnabled,
                    Username = x.ArrowUsername,
                    ApiUrl = x.ArrowApiUrl,
                    ApiKey = x.ArrowApiKey
                }))
                .ForMember(x => x.Octopart, options => options.MapFrom(x => new OctopartUserConfiguration
                {
                    Enabled = x.NexarEnabled,
                    ClientId = x.NexarClientId,
                    ClientSecret = x.NexarClientSecret
                }))
                .ForMember(x => x.Tme, options => options.MapFrom(x => new TmeUserConfiguration
                {
                    Enabled = x.TmeEnabled,
                    Country = x.TmeCountry,
                    ApplicationSecret = x.TmeApplicationSecret,
                    ApiUrl = x.TmeApiUrl,
                    ApiKey = x.TmeApiKey,
                    ResolveExternalLinks = x.TmeResolveExternalLinks
                }))
                .ForMember(x => x.EnableAutoPartSearch, options => options.Ignore())
                .ForMember(x => x.EnableDarkMode, options => options.Ignore())
                .ForMember(x => x.EnableCheckNewVersion, options => options.Ignore())
                .ForMember(x => x.Printer, options => options.Ignore())
                .ForMember(x => x.Barcode, options => options.Ignore())
                .ForMember(x => x.Locale, options => options.Ignore())
                .ForMember(x => x.UseModule, options => options.Ignore())
                .ForMember(x => x.LicenseKey, options => options.Ignore())
                .ForMember(x => x.MaxCacheItems, options => options.Ignore())
                .ForMember(x => x.CacheSlidingExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CacheAbsoluteExpirationMinutes, options => options.Ignore())
                .ForMember(x => x.CustomFields, options => options.Ignore())
                .ForMember(x => x.KiCad, options => options.Ignore())
                .ReverseMap();

            CreateMap<MouserConfiguration, MouserConfigurationResponse>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.ApiUrl))
                .ForMember(x => x.OrderApiKey, options => options.MapFrom(x => x.ApiKeys.OrderApiKey))
                .ForMember(x => x.SearchApiKey, options => options.MapFrom(x => x.ApiKeys.SearchApiKey))
                .ForMember(x => x.CartApiKey, options => options.MapFrom(x => x.ApiKeys.CartApiKey))
                .ReverseMap();

            CreateMap<BarcodeConfiguration, BarcodeSettingsResponse>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.Prefix2D, options => options.MapFrom(x => x.Prefix2D))
                .ForMember(x => x.Profile, options => options.MapFrom(x => x.Profile))
                .ForMember(x => x.MaxKeystrokeThresholdMs, options => options.MapFrom(x => x.MaxKeystrokeThresholdMs))
                .ForMember(x => x.IsDebug, options => options.MapFrom(x => x.IsDebug))
                .ForMember(x => x.Profile, options => options.MapFrom(x => x.Profile))
                .ForMember(x => x.BufferTime, options => options.MapFrom(x => x.BufferTime));

            CreateMap<PrinterConfiguration, PrinterSettingsResponse>(MemberList.None)
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.PartLabelSource))
                .ForMember(x => x.PrintMode, options => options.MapFrom(x => x.PrintMode))
                .ForMember(x => x.PrinterName, options => options.MapFrom(x => x.PrinterName))
                // complex mapping situation
                .ForMember(x => x.Lines, options => options.MapFrom(x => new List<LineConfiguration> {
                        x.PartLabelTemplate.Line1 ?? new LineConfiguration(),
                        x.PartLabelTemplate.Line2 ?? new LineConfiguration(),
                        x.PartLabelTemplate.Line3 ?? new LineConfiguration(),
                        x.PartLabelTemplate.Line4 ?? new LineConfiguration()
                    })

                )
                .ForMember(x => x.Identifiers, options => options.MapFrom(x => new List<LineConfiguration> {
                        x.PartLabelTemplate.Identifier ?? new LineConfiguration(),
                        x.PartLabelTemplate.Identifier2 ?? new LineConfiguration()
                    })
                );

            CreateMap<PrinterSettingsResponse, PrinterConfiguration>(MemberList.None)
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.PartLabelSource))
                .ForMember(x => x.PrintMode, options => options.MapFrom(x => x.PrintMode))
                .ForMember(x => x.PrinterName, options => options.MapFrom(x => x.PrinterName))
                .ForMember(x => x.LabelDefinitions, options => options.Ignore())
                // complex mapping situation
                .ForMember(x => x.PartLabelTemplate, options => options.MapFrom(x => new PartLabelTemplate
                {
                    Line1 = x.Lines != null ? x.Lines.Skip(0).FirstOrDefault() : null,
                    Line2 = x.Lines != null ? x.Lines.Skip(1).FirstOrDefault() : null,
                    Line3 = x.Lines != null ? x.Lines.Skip(2).FirstOrDefault() : null,
                    Line4 = x.Lines != null ? x.Lines.Skip(3).FirstOrDefault() : null,
                    Identifier = x.Identifiers != null ? x.Identifiers.Skip(0).FirstOrDefault() : null,
                    Identifier2 = x.Identifiers != null ? x.Identifiers.Skip(1).FirstOrDefault() : null
                })
            );

            CreateMap<SwarmConfiguration, SwarmUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.ApiKey, options => options.MapFrom(x => x.ApiKey))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.ApiUrl))
                .ForMember(x => x.Timeout, options => options.MapFrom(x => x.Timeout))
                .ReverseMap();
            CreateMap<DigikeyConfiguration, DigiKeyUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.Site, options => options.MapFrom(x => x.Site))
                .ForMember(x => x.ClientId, options => options.MapFrom(x => x.ClientId))
                .ForMember(x => x.ClientSecret, options => options.MapFrom(x => x.ClientSecret))
                .ForMember(x => x.oAuthPostbackUrl, options => options.MapFrom(x => x.oAuthPostbackUrl))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.ApiUrl))
                .ReverseMap();
            CreateMap<MouserConfiguration, MouserUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.OrderApiKey, options => options.MapFrom(x => x.ApiKeys.OrderApiKey))
                .ForMember(x => x.CartApiKey, options => options.MapFrom(x => x.ApiKeys.CartApiKey))
                .ForMember(x => x.SearchApiKey, options => options.MapFrom(x => x.ApiKeys.SearchApiKey))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.ApiUrl))
                .ReverseMap();
            CreateMap<ArrowConfiguration, ArrowUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.Username, options => options.MapFrom(x => x.Username))
                .ForMember(x => x.ApiKey, options => options.MapFrom(x => x.ApiKey))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.ApiUrl))
                .ReverseMap();
            CreateMap<OctopartConfiguration, OctopartUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.ClientId, options => options.MapFrom(x => x.ClientId))
                .ForMember(x => x.ClientSecret, options => options.MapFrom(x => x.ClientSecret))
                .ReverseMap();
            CreateMap<TmeConfiguration, TmeUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.Enabled))
                .ForMember(x => x.Country, options => options.MapFrom(x => x.Country))
                .ForMember(x => x.ApplicationSecret, options => options.MapFrom(x => x.ApplicationSecret))
                .ForMember(x => x.ApiKey, options => options.MapFrom(x => x.ApiKey))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.ApiUrl))
                .ForMember(x => x.ResolveExternalLinks, options => options.MapFrom(x => x.ResolveExternalLinks))
                .ReverseMap();
            CreateMap<UserIntegrationConfiguration, SwarmUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.SwarmEnabled))
                .ForMember(x => x.ApiKey, options => options.MapFrom(x => x.SwarmApiKey))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.SwarmApiUrl))
                .ForMember(x => x.Timeout, options => options.MapFrom(x => x.SwarmTimeout))
                .ReverseMap();

            CreateMap<UserIntegrationConfiguration, DigiKeyUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.DigiKeyEnabled))
                .ForMember(x => x.Site, options => options.MapFrom(x => x.DigiKeySite))
                .ForMember(x => x.ClientId, options => options.MapFrom(x => x.DigiKeyClientId))
                .ForMember(x => x.ClientSecret, options => options.MapFrom(x => x.DigiKeyClientSecret))
                .ForMember(x => x.oAuthPostbackUrl, options => options.MapFrom(x => x.DigiKeyOAuthPostbackUrl))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.DigiKeyApiUrl))
                .ReverseMap();

            CreateMap<UserIntegrationConfiguration, MouserUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.MouserEnabled))
                .ForMember(x => x.CartApiKey, options => options.MapFrom(x => x.MouserCartApiKey))
                .ForMember(x => x.OrderApiKey, options => options.MapFrom(x => x.MouserOrderApiKey))
                .ForMember(x => x.SearchApiKey, options => options.MapFrom(x => x.MouserSearchApiKey))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.MouserApiUrl))
                .ReverseMap();

            CreateMap<UserIntegrationConfiguration, ArrowUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.ArrowEnabled))
                .ForMember(x => x.Username, options => options.MapFrom(x => x.ArrowUsername))
                .ForMember(x => x.ApiKey, options => options.MapFrom(x => x.ArrowApiKey))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.ArrowApiUrl))
                .ReverseMap();

            CreateMap<UserIntegrationConfiguration, OctopartUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.NexarEnabled))
                .ForMember(x => x.ClientId, options => options.MapFrom(x => x.NexarClientId))
                .ForMember(x => x.ClientSecret, options => options.MapFrom(x => x.NexarClientSecret))
                .ReverseMap();

            CreateMap<UserIntegrationConfiguration, TmeUserConfiguration>()
                .ForMember(x => x.Enabled, options => options.MapFrom(x => x.TmeEnabled))
                .ForMember(x => x.Country, options => options.MapFrom(x => x.TmeCountry))
                .ForMember(x => x.ApplicationSecret, options => options.MapFrom(x => x.TmeApplicationSecret))
                .ForMember(x => x.ApiKey, options => options.MapFrom(x => x.TmeApiKey))
                .ForMember(x => x.ApiUrl, options => options.MapFrom(x => x.TmeApiUrl))
                .ForMember(x => x.ResolveExternalLinks, options => options.MapFrom(x => x.TmeResolveExternalLinks))
                .ReverseMap();
        }
    }
}
