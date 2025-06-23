using AutoMapper;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Responses;
using Binner.StorageProvider.EntityFrameworkCore;
using System.Security;
using TypeSupport.Extensions;
using DataModel = Binner.Data.Model;

namespace Binner.Services.MappingProfiles.ModelCommon
{
    public class PartTypeMappingAction<TSource, TDestination> : IMappingAction<TSource, TDestination>
    {
        private readonly IPartTypesCache _partTypesCache;
        private readonly IRequestContextAccessor _requestContext;

        public PartTypeMappingAction(IPartTypesCache partTypesCache, IRequestContextAccessor requestContext)
        {
            _partTypesCache = partTypesCache;
            _requestContext = requestContext;
        }

        public void Process(TSource source, TDestination destination, ResolutionContext context)
        {
            var userContext = _requestContext.GetUserContext() ?? throw new SecurityException("No user context available!");
            var partTypeId = (long)source.GetPropertyValue("PartTypeId");
            var value = _partTypesCache.Cache
                .Where(x => x.OrganizationId == null || x.OrganizationId == userContext.OrganizationId)
                .Where(x => x.PartTypeId == partTypeId)
                .Select(x => x.Name)
                .FirstOrDefault();

            destination.SetPropertyValue("PartType", value);
        }
    }

    public class PartTypeMappingAction1 : IMappingAction<Part, PartResponse>
    {
        private readonly IPartTypesCache _partTypesCache;
        private readonly IRequestContextAccessor _requestContext;

        public PartTypeMappingAction1(IPartTypesCache partTypesCache, IRequestContextAccessor requestContext)
        {
            _partTypesCache = partTypesCache;
            _requestContext = requestContext;
        }

        public void Process(Part source, PartResponse destination, ResolutionContext context)
        {
            var userContext = _requestContext.GetUserContext() ?? throw new SecurityException("No user context available!");
            destination.PartType = _partTypesCache.Cache
                .Where(x => x.OrganizationId == null || x.OrganizationId == userContext.OrganizationId)
                .Where(x => x.PartTypeId == source.PartTypeId)
                .Select(x => x.Name)
                .FirstOrDefault();
        }
    }

    public class PartTypeMappingAction2 : IMappingAction<Part, CommonPart>
    {
        private readonly IPartTypesCache _partTypesCache;
        private readonly IRequestContextAccessor _requestContext;

        public PartTypeMappingAction2(IPartTypesCache partTypesCache, IRequestContextAccessor requestContext)
        {
            _partTypesCache = partTypesCache;
            _requestContext = requestContext;
        }

        public void Process(Part source, CommonPart destination, ResolutionContext context)
        {
            var userContext = _requestContext.GetUserContext() ?? throw new SecurityException("No user context available!");
            destination.PartType = _partTypesCache.Cache
                .Where(x => x.OrganizationId == null || x.OrganizationId == userContext.OrganizationId)
                .Where(x => x.PartTypeId == source.PartTypeId)
                .Select(x => x.Name)
                .FirstOrDefault();
        }
    }

    public class PartTypeMappingAction3 : IMappingAction<DataModel.Part, PartResponse>
    {
        private readonly IPartTypesCache _partTypesCache;
        private readonly IRequestContextAccessor _requestContext;

        public PartTypeMappingAction3(IPartTypesCache partTypesCache, IRequestContextAccessor requestContext)
        {
            _partTypesCache = partTypesCache;
            _requestContext = requestContext;
        }

        public void Process(DataModel.Part source, PartResponse destination, ResolutionContext context)
        {
            var userContext = _requestContext.GetUserContext() ?? throw new SecurityException("No user context available!");
            destination.PartType = _partTypesCache.Cache
                .Where(x => x.OrganizationId == null || x.OrganizationId == userContext.OrganizationId)
                .Where(x => x.PartTypeId == source.PartTypeId)
                .Select(x => x.Name)
                .FirstOrDefault();
        }
    }
}
