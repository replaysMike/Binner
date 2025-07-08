using AutoMapper;
using Binner.Model.Configuration;
using Binner.Model.IO.Printing;
using Binner.Model.Requests;
using Binner.Model.Responses;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles
{
    public class UserPrinterConfigurationProfile : Profile
    {
        public UserPrinterConfigurationProfile()
        {
            CreateMap<DataModel.UserPrinterConfiguration, UserPrinterConfiguration>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.PartLabelSource))
                .ForMember(x => x.PrinterName, options => options.MapFrom(x => x.PrinterName))
                .ForMember(x => x.RemoteAddressUrl, options => options.MapFrom(x => x.RemoteAddressUrl))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.UserPrinterTemplateConfigurations, options => options.MapFrom(x => x.UserPrinterTemplateConfigurations))
                .ForMember(x => x.PrintMode, options => options.MapFrom(x => x.PrintMode))
            ;

            CreateMap<UserPrinterConfiguration, DataModel.UserPrinterConfiguration>()
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.PartLabelSource))
                .ForMember(x => x.PrinterName, options => options.MapFrom(x => x.PrinterName))
                .ForMember(x => x.RemoteAddressUrl, options => options.MapFrom(x => x.RemoteAddressUrl))
                .ForMember(x => x.PrintMode, options => options.MapFrom(x => x.PrintMode))

                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.UserPrinterTemplateConfigurations, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
                .ForMember(x => x.UserPrinterConfigurationId, options => options.Ignore())
            ;

            CreateMap<UserPrinterConfiguration, PrinterSettings>()
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.PartLabelSource))
                .ForMember(x => x.PrinterName, options => options.MapFrom(x => x.PrinterName))
                .ForMember(x => x.PrintMode, options => options.MapFrom(x => x.PrintMode))

                .ForMember(x => x.LabelDefinitions, options => options.Ignore())
                .ForMember(x => x.PartLabelTemplate, options => options.Ignore())
            ;

            CreateMap<DataModel.UserPrinterTemplateConfiguration, UserPrinterTemplateConfiguration>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.AutoSize, options => options.MapFrom(x => x.AutoSize))
                .ForMember(x => x.Barcode, options => options.MapFrom(x => x.Barcode))
                .ForMember(x => x.Color, options => options.MapFrom(x => x.Color))
                .ForMember(x => x.Content, options => options.MapFrom(x => x.Content))
                .ForMember(x => x.FontName, options => options.MapFrom(x => x.FontName))
                .ForMember(x => x.FontSize, options => options.MapFrom(x => x.FontSize))
                .ForMember(x => x.Label, options => options.MapFrom(x => x.Label))
                .ForMember(x => x.Line, options => options.MapFrom(x => x.Line))
                .ForMember(x => x.LowerCase, options => options.MapFrom(x => x.LowerCase))
                .ForMember(x => x.MarginBottom, options => options.MapFrom(x => x.MarginBottom))
                .ForMember(x => x.MarginLeft, options => options.MapFrom(x => x.MarginLeft))
                .ForMember(x => x.MarginRight, options => options.MapFrom(x => x.MarginRight))
                .ForMember(x => x.MarginTop, options => options.MapFrom(x => x.MarginTop))
                .ForMember(x => x.Position, options => options.MapFrom(x => x.Position))
                .ForMember(x => x.Rotate, options => options.MapFrom(x => x.Rotate))
                .ForMember(x => x.UpperCase, options => options.MapFrom(x => x.UpperCase))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                ;

            CreateMap<UserPrinterTemplateConfiguration, DataModel.UserPrinterTemplateConfiguration>()
                .ForMember(x => x.AutoSize, options => options.MapFrom(x => x.AutoSize))
                .ForMember(x => x.Barcode, options => options.MapFrom(x => x.Barcode))
                .ForMember(x => x.Color, options => options.MapFrom(x => x.Color))
                .ForMember(x => x.Content, options => options.MapFrom(x => x.Content))
                .ForMember(x => x.FontName, options => options.MapFrom(x => x.FontName))
                .ForMember(x => x.FontSize, options => options.MapFrom(x => x.FontSize))
                .ForMember(x => x.Label, options => options.MapFrom(x => x.Label))
                .ForMember(x => x.Line, options => options.MapFrom(x => x.Line))
                .ForMember(x => x.LowerCase, options => options.MapFrom(x => x.LowerCase))
                .ForMember(x => x.MarginBottom, options => options.MapFrom(x => x.MarginBottom))
                .ForMember(x => x.MarginLeft, options => options.MapFrom(x => x.MarginLeft))
                .ForMember(x => x.MarginRight, options => options.MapFrom(x => x.MarginRight))
                .ForMember(x => x.MarginTop, options => options.MapFrom(x => x.MarginTop))
                .ForMember(x => x.Position, options => options.MapFrom(x => x.Position))
                .ForMember(x => x.Rotate, options => options.MapFrom(x => x.Rotate))
                .ForMember(x => x.UpperCase, options => options.MapFrom(x => x.UpperCase))

                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.User, options => options.Ignore())
                .ForMember(x => x.UserPrinterConfiguration, options => options.Ignore())
                .ForMember(x => x.UserPrinterConfigurationId, options => options.Ignore())
                .ForMember(x => x.UserPrinterTemplateConfigurationId, options => options.Ignore())
                ;

            CreateMap<PrinterConfiguration, UserPrinterConfiguration>()
                .ForMember(x => x.PartLabelName, options => options.MapFrom(x => x.PartLabelName))
                .ForMember(x => x.PartLabelSource, options => options.MapFrom(x => x.PartLabelSource))
                .ForMember(x => x.PrinterName, options => options.MapFrom(x => x.PrinterName))
                .ForMember(x => x.RemoteAddressUrl, options => options.MapFrom(x => x.RemoteAddressUrl))
                .ForMember(x => x.PrintMode, options => options.MapFrom(x => x.PrintMode))

                .ForMember(x => x.UserPrinterTemplateConfigurations, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
                .ForMember(x => x.UserPrinterTemplateConfigurations, options => options.Ignore())
                .ForMember(x => x.UserId, options => options.Ignore())
            ;
        }

        public class UserPrinterConfigurationMappingAction : BaseCollectionMapperAction<UserPrinterConfiguration, DataModel.UserPrinterConfiguration>
        {
            public override void Process(UserPrinterConfiguration source, DataModel.UserPrinterConfiguration destination, ResolutionContext context)
            {
                MapCollection(source.UserPrinterTemplateConfigurations, destination.UserPrinterTemplateConfigurations, context);
            }
        }
    }
}
