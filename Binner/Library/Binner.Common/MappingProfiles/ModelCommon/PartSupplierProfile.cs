using AutoMapper;
using Binner.Model.Common;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class PartSupplierProfile : Profile
    {
        public PartSupplierProfile()
        {
            CreateMap<PartSupplier, DataModel.PartSupplier>()
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.ImageUrl, options => options.MapFrom(x => x.ImageUrl))
                .ForMember(x => x.MinimumOrderQuantity, options => options.MapFrom(x => x.MinimumOrderQuantity))
                .ForMember(x => x.PartId, options => options.MapFrom(x => x.PartId))
                .ForMember(x => x.PartSupplierId, options => options.MapFrom(x => x.PartSupplierId))
                .ForMember(x => x.ProductUrl, options => options.MapFrom(x => x.ProductUrl))
                .ForMember(x => x.QuantityAvailable, options => options.MapFrom(x => x.QuantityAvailable))
                .ForMember(x => x.SupplierPartNumber, options => options.MapFrom(x => x.SupplierPartNumber))
                .ForMember(x => x.Cost, options => options.MapFrom(x => x.Cost))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.Part, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
#if INITIALCREATE
                .ForMember(x => x.User, options => options.Ignore())
#endif
                .ReverseMap();
        }
    }
}
