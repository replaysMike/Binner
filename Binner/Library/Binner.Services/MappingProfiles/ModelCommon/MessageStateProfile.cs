using AutoMapper;
using Binner.Model;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class MessageStateProfile : Profile
    {
        public MessageStateProfile()
        {
            CreateMap<DataModel.MessageState, MessageState>()
                .ForMember(x => x.MessageStateId, options => options.MapFrom(x => x.MessageStateId))
                .ForMember(x => x.MessageId, options => options.MapFrom(x => x.MessageId))
                .ForMember(x => x.ReadDateUtc, options => options.MapFrom(x => x.ReadDateUtc))

                .ForMember(x => x.Title, options => options.Ignore())
                .ForMember(x => x.Message, options => options.Ignore())
                .ForMember(x => x.DateCreatedUtc, options => options.Ignore())
                .ReverseMap()
                ;
        }
    }
}
