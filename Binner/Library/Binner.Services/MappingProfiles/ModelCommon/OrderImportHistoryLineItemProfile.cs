using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class OrderImportHistoryLineItemProfile : Profile
    {
        public OrderImportHistoryLineItemProfile()
        {
            CreateMap<DataModel.OrderImportHistoryLineItem, OrderImportHistoryLineItem>()
                .ForMember(x => x.OrderImportHistoryLineItemId, options => options.MapFrom(x => x.OrderImportHistoryLineItemId))
                .ForMember(x => x.OrderImportHistoryId, options => options.MapFrom(x => x.OrderImportHistoryId))
                .ForMember(x => x.PartId, options => options.MapFrom(x => x.PartId))
                .ForMember(x => x.Cost, options => options.MapFrom(x => x.Cost))
                .ForMember(x => x.CustomerReference, options => options.MapFrom(x => x.CustomerReference))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.Manufacturer, options => options.MapFrom(x => x.Manufacturer))
                .ForMember(x => x.ManufacturerPartNumber, options => options.MapFrom(x => x.ManufacturerPartNumber))
                .ForMember(x => x.PartNumber, options => options.MapFrom(x => x.PartNumber))
                .ForMember(x => x.Quantity, options => options.MapFrom(x => x.Quantity))
                .ForMember(x => x.Supplier, options => options.MapFrom(x => x.Supplier))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.OrganizationId, options => options.MapFrom(x => x.OrganizationId))
                .ReverseMap();
        }
    }
}
