using AutoMapper;
using Binner.Model.Configuration;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V3 = Binner.Model.Integrations.DigiKey.V3;
using V4 = Binner.Model.Integrations.DigiKey.V4;

namespace Binner.Services.MappingProfiles
{
    public class CategoriesResponseProfile : Profile
    {
        public CategoriesResponseProfile()
        {
            CreateMap<V3.CategoriesResponse, CategoriesResponse>()
                .ForMember(x => x.ProductCount, options => options.MapFrom(x => x.ProductCount))
                .ForMember(x => x.Categories, options => options.MapFrom(x => x.Categories))
                .ReverseMap();
            CreateMap<V4.CategoriesResponse, CategoriesResponse>()
                .ForMember(x => x.ProductCount, options => options.MapFrom(x => x.ProductCount))
                .ForMember(x => x.Categories, options => options.MapFrom(x => x.Categories))
                .ReverseMap();

            CreateMap<V3.FullCategory, Category>()
                .ForMember(x => x.CategoryId, options => options.MapFrom(x => x.CategoryId))
                .ForMember(x => x.ParentId, options => options.MapFrom(x => x.ParentId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.ProductCount, options => options.MapFrom(x => x.ProductCount))
                .ForMember(x => x.Children, options => options.MapFrom(x => x.Children))
                .ReverseMap();

            CreateMap<V4.FullCategory, Category>()
                .ForMember(x => x.CategoryId, options => options.MapFrom(x => x.CategoryId))
                .ForMember(x => x.ParentId, options => options.MapFrom(x => x.ParentId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.ProductCount, options => options.MapFrom(x => x.ProductCount))
                .ForMember(x => x.Children, options => options.MapFrom(x => x.Children))
                .ReverseMap();
        }
    }
}
