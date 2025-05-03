using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class PartScanHistoryProfile : Profile
    {
        public PartScanHistoryProfile()
        {
            CreateMap<DataModel.PartScanHistory, PartScanHistory>()
                .ForMember(x => x.PartScanHistoryId, options => options.MapFrom(x => x.PartScanHistoryId))
                .ForMember(x => x.PartId, options => options.MapFrom(x => x.PartId))
                .ForMember(x => x.RawScan, options => options.MapFrom(x => x.RawScan))
                .ForMember(x => x.BarcodeType, options => options.MapFrom(x => x.BarcodeType))
                .ForMember(x => x.CountryOfOrigin, options => options.MapFrom(x => x.CountryOfOrigin))
                .ForMember(x => x.Crc, options => options.MapFrom(x => x.Crc))
                .ForMember(x => x.Invoice, options => options.MapFrom(x => x.Invoice))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.LotCode, options => options.MapFrom(x => x.LotCode))
                .ForMember(x => x.ManufacturerPartNumber, options => options.MapFrom(x => x.ManufacturerPartNumber))
                .ForMember(x => x.Mid, options => options.MapFrom(x => x.Mid))
                .ForMember(x => x.Packlist, options => options.MapFrom(x => x.Packlist))
                .ForMember(x => x.Quantity, options => options.MapFrom(x => x.Quantity))
                .ForMember(x => x.SalesOrder, options => options.MapFrom(x => x.SalesOrder))
                .ForMember(x => x.ScannedLabelType, options => options.MapFrom(x => x.ScannedLabelType))
                .ForMember(x => x.Supplier, options => options.MapFrom(x => x.Supplier))
                .ForMember(x => x.SupplierPartNumber, options => options.MapFrom(x => x.SupplierPartNumber))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.OrganizationId, options => options.MapFrom(x => x.OrganizationId))
                .ReverseMap();
        }
    }
}
