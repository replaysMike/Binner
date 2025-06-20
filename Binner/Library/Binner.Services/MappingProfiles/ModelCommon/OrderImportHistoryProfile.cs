using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class OrderImportHistoryProfile : Profile
    {
        public OrderImportHistoryProfile()
        {
            CreateMap<DataModel.OrderImportHistory, OrderImportHistory>()
                .ForMember(x => x.OrderImportHistoryId, options => options.MapFrom(x => x.OrderImportHistoryId))
                .ForMember(x => x.Supplier, options => options.MapFrom(x => x.Supplier))
                .ForMember(x => x.Invoice, options => options.MapFrom(x => x.Invoice))
                .ForMember(x => x.Packlist, options => options.MapFrom(x => x.Packlist))
                .ForMember(x => x.SalesOrder, options => options.MapFrom(x => x.SalesOrder))
                .ForMember(x => x.Packlist, options => options.MapFrom(x => x.Packlist))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.OrganizationId, options => options.MapFrom(x => x.OrganizationId))
                .ForMember(x => x.OrderImportHistoryLineItems, options => options.MapFrom(x => x.OrderImportHistoryLineItems))
                .ReverseMap();
        }
    }
}
