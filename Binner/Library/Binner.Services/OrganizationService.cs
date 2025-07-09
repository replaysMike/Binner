using AutoMapper;
using Binner.Common.Extensions;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Responses;
using Microsoft.EntityFrameworkCore;
using System.Data;
using DataModel = Binner.Data.Model;

namespace Binner.Services
{
    /// <summary>
    /// Manage users
    /// </summary>
    public class OrganizationService<TOrg> : IOrganizationService<TOrg>
        where TOrg : class, IOrganization
    {
        protected readonly IDbContextFactory<BinnerContext> _contextFactory;
        protected readonly IMapper _mapper;
        protected readonly IRequestContextAccessor _requestContext;

        public OrganizationService(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, IRequestContextAccessor requestContext)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContext;
        }

        protected virtual IQueryable<DataModel.Organization> GetOrganizationQueryable(BinnerContext context, IUserContext userContext)
            => context.Organizations
                .Include(x => x.OrganizationIntegrationConfigurations)
                .Include(x => x.OrganizationConfigurations)
                .AsQueryable();

        public virtual async Task<TOrg> CreateOrganizationAsync(TOrg organization)
        {
            var userContext = _requestContext.GetUserContext();
            if (userContext == null) throw new UserContextUnauthorizedException();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.Organization>(organization);
            context.Organizations.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<TOrg>(entity);
        }

        public virtual async Task<TOrg?> GetOrganizationAsync(TOrg organization)
        {
            var userContext = _requestContext.GetUserContext();
            if (userContext == null) throw new UserContextUnauthorizedException();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Organizations
                .Where(x => x.OrganizationId == organization.OrganizationId)
                .FirstOrDefaultAsync();
            return _mapper.Map<TOrg>(entity);
        }

        public virtual async Task<PaginatedResponse<TOrg>> GetOrganizationsAsync(PaginatedRequest request)
        {
            var userContext = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                // ensure camel case names, EF properties are case sensitive
                request.OrderBy = request.OrderBy.UcFirst();
            }

            var pageRecords = (request.Page - 1) * request.Results;
            var entitiesQueryable = context.Organizations
                .Include(x => x.OrganizationConfigurations)
                .Include(x => x.OrganizationIntegrationConfigurations)
                .AsQueryable();
            var totalItems = await entitiesQueryable.CountAsync();
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
            }

            var entities = await entitiesQueryable.Skip(pageRecords)
                .Take(request.Results)
                .ToListAsync();

            // map entities to parts
            return new PaginatedResponse<TOrg>(totalItems, request.Results, request.Page, _mapper.Map<ICollection<TOrg>>(entities));
        }

        public virtual async Task<TOrg?> UpdateOrganizationAsync(TOrg organization)
        {
            var userContext = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Organizations
                .Include(x => x.OrganizationConfigurations)
                .Include(x => x.OrganizationIntegrationConfigurations)
                .FirstOrDefaultAsync(x => x.OrganizationId == organization.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(organization, entity);
                await context.SaveChangesAsync();

                return _mapper.Map(entity, organization);
            }
            return null;
        }

        public virtual async Task<bool> DeleteOrganizationAsync(int organizationId)
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetOrganizationQueryable(context, userContext)
                .Where(x => x.OrganizationId == organizationId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null)
                throw new KeyNotFoundException($"Could not find organization with id '{organizationId}'");

            await context.OrganizationConfigurations.Where(x => x.OrganizationId == organizationId).ExecuteDeleteAsync();
            await context.OrganizationIntegrationConfigurations.Where(x => x.OrganizationId == organizationId).ExecuteDeleteAsync();

            context.Organizations.Remove(entity);

            return await context.SaveChangesAsync() > 0;
        }
    }
}
