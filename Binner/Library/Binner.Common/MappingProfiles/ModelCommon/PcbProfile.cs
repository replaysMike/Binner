﻿using AutoMapper;
using Binner.Model;
using Binner.Model.Responses;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class PcbProfile : Profile
    {
        public PcbProfile()
        {
            CreateMap<Pcb, DataModel.Pcb>()
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.SerialNumberFormat, options => options.MapFrom(x => x.SerialNumberFormat))
                .ForMember(x => x.LastSerialNumber, options => options.MapFrom(x => x.LastSerialNumber))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.Cost, options => options.MapFrom(x => x.Cost))
                .ForMember(x => x.Quantity, options => options.MapFrom(x => x.Quantity))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.ProjectPcbAssignments, options => options.Ignore())
                .ForMember(x => x.PcbStoredFileAssignments, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
                .ForMember(x => x.ProjectPcbProduceHistory, options => options.Ignore())
#if INITIALCREATE
                .ForMember(x => x.User, options => options.Ignore())
#endif
                ;

            CreateMap<DataModel.Pcb, Pcb>()
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.SerialNumberFormat, options => options.MapFrom(x => x.SerialNumberFormat))
                .ForMember(x => x.LastSerialNumber, options => options.MapFrom(x => x.LastSerialNumber))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.Cost, options => options.MapFrom(x => x.Cost))
                .ForMember(x => x.Quantity, options => options.MapFrom(x => x.Quantity))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.CustomFields, options => options.Ignore()) // mapped manually
                ;

            CreateMap<DataModel.Pcb, ProjectPcb>()
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.Description, options => options.MapFrom(x => x.Description))
                .ForMember(x => x.SerialNumberFormat, options => options.MapFrom(x => x.SerialNumberFormat))
                .ForMember(x => x.LastSerialNumber, options => options.MapFrom(x => x.LastSerialNumber))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.Cost, options => options.MapFrom(x => x.Cost))
                .ForMember(x => x.Quantity, options => options.MapFrom(x => x.Quantity))
                .ForMember(x => x.CustomFields, options => options.Ignore()) // mapped manually
                .ForMember(x => x.StoredFile, options => options.Ignore())
                .ForMember(x => x.PartsCount, options => options.Ignore())
                ;
        }
    }
}
