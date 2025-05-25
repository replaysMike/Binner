﻿using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class PartParametricProfile : Profile
    {
        public PartParametricProfile()
        {
            CreateMap<DataModel.PartParametric, PartParametric>()
                .ForMember(x => x.PartParametricId, options => options.MapFrom(x => x.PartParametricId))
                .ForMember(x => x.PartId, options => options.MapFrom(x => x.PartId))
                .ForMember(x => x.DigiKeyParameterId, options => options.MapFrom(x => x.DigiKeyParameterId))
                .ForMember(x => x.DigiKeyParameterType, options => options.MapFrom(x => x.DigiKeyParameterType))
                .ForMember(x => x.DigiKeyValueId, options => options.MapFrom(x => x.DigiKeyValueId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Units, options => options.MapFrom(x => x.Units))
                .ForMember(x => x.Value, options => options.MapFrom(x => x.Value))
                .ForMember(x => x.ValueNumber, options => options.MapFrom(x => x.ValueNumber))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ReverseMap()
                ;
        }
    }
}
