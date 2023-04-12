using AutoMapper;
using Binner.Model.Common;
using DataModel = Binner.Data.Model;

namespace Binner.Common.MappingProfiles.ModelCommon
{
    public class PcbStoredFileAssignmentProfile : Profile
    {
        public PcbStoredFileAssignmentProfile()
        {
            CreateMap<PcbStoredFileAssignment, DataModel.PcbStoredFileAssignment>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.Notes, options => options.MapFrom(x => x.Notes))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.PcbStoredFileAssignmentId, options => options.MapFrom(x => x.PcbStoredFileAssignmentId))
                .ForMember(x => x.StoredFileId, options => options.MapFrom(x => x.StoredFileId))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                .ForMember(x => x.StoredFile, options => options.Ignore())
                .ForMember(x => x.Pcb, options => options.Ignore())
#if INITIALCREATE
                .ForMember(x => x.User, options => options.Ignore())
#endif
                ;

            CreateMap<DataModel.PcbStoredFileAssignment, PcbStoredFileAssignment>()
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.DateModifiedUtc, options => options.MapFrom(x => x.DateModifiedUtc))
                .ForMember(x => x.Notes, options => options.MapFrom(x => x.Notes))
                .ForMember(x => x.Name, options => options.MapFrom(x => x.Name))
                .ForMember(x => x.PcbId, options => options.MapFrom(x => x.PcbId))
                .ForMember(x => x.PcbStoredFileAssignmentId, options => options.MapFrom(x => x.PcbStoredFileAssignmentId))
                .ForMember(x => x.StoredFileId, options => options.MapFrom(x => x.StoredFileId))
                .ForMember(x => x.UserId, options => options.MapFrom(x => x.UserId))
                ;
        }
    }
}
