using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class StoredFileProfile : Profile
    {
        public StoredFileProfile()
        {
            CreateMap<StoredFile, DataModel.StoredFile>()
                .ForMember(x => x.StoredFileId, options => options.MapFrom(x => x.StoredFileId))
                .ForMember(x => x.Crc32, options => options.MapFrom(x => x.Crc32))
                .ForMember(x => x.FileLength, options => options.MapFrom(x => x.FileLength))
                .ForMember(x => x.FileName, options => options.MapFrom(x => x.FileName))
                .ForMember(x => x.OriginalFileName, options => options.MapFrom(x => x.OriginalFileName))
                .ForMember(x => x.PartId, options => options.MapFrom(x => x.PartId))
                .ForMember(x => x.StoredFileType, options => options.MapFrom(x => x.StoredFileType))
                .ForMember(x => x.DateCreatedUtc, options => options.MapFrom(x => x.DateCreatedUtc))
                .ForMember(x => x.UserId, options => options.Ignore())
                .ForMember(x => x.Part, options => options.Ignore())
                .ForMember(x => x.PcbStoredFileAssignments, options => options.Ignore())
                .ForMember(x => x.OrganizationId, options => options.Ignore())
#if INITIALCREATE
                .ForMember(x => x.User, options => options.Ignore())
                .ForMember(x => x.DateModifiedUtc, options => options.Ignore())
#endif
                .ForMember(x => x.RecordId, options => options.Ignore())
                .ForMember(x => x.RecordType, options => options.Ignore())
                .ForMember(x => x.GlobalId, options => options.MapFrom(x => x.GlobalId))
                .ReverseMap();
        }
    }
}
