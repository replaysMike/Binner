using AutoMapper;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security;
using DataModel = Binner.Data.Model;

namespace Binner.Services
{
    /// <summary>
    /// Manage users
    /// </summary>
    public class UserService<TUser> : IUserService<TUser>
        where TUser : User, new()
    {
        protected readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IMapper _mapper;
        protected readonly IRequestContextAccessor _requestContext;
        private readonly ILicensedService<TUser> _licensedService;

        public UserService(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, IRequestContextAccessor requestContext, ILicensedService<TUser> licensedService)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContext;
            _licensedService = licensedService;
        }

        protected virtual IQueryable<DataModel.User> GetUserQueryable(BinnerContext context, IUserContext userContext)
            => context.Users
                .Include(x => x.OAuthCredentials)
                .Include(x => x.OAuthRequests)
                .Include(x => x.Projects).ThenInclude(x => x.ProjectPartAssignments)
                .Include(x => x.Projects).ThenInclude(x => x.ProjectPcbAssignments)
                .Include(x => x.UserIntegrationConfigurations)
                .Include(x => x.UserPrinterConfigurations)
                .Include(x => x.UserPrinterTemplateConfigurations)
                .Where(x => x.OrganizationId == userContext.OrganizationId)
                .AsQueryable();

        public virtual Task<TUser> CreateUserAsync(TUser user) => _licensedService.CreateUserAsync(user);

        public virtual Task<TUser?> GetUserAsync(TUser user) => _licensedService.GetUserAsync(user);

        public virtual Task<ICollection<TUser>> GetUsersAsync(PaginatedRequest request) => _licensedService.GetUsersAsync(request);

        public virtual Task<TUser?> UpdateUserAsync(TUser user) => _licensedService.UpdateUserAsync(user);

        public virtual async Task<bool> DeleteUserAsync(int userId)
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetUserQueryable(context, userContext)
                .Where(x => x.UserId == userId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null)
                throw new KeyNotFoundException($"Could not find user with id '{userId}'");

            if (entity.IsAdmin)
            {
                var anyOtherAdminUser = await GetUserQueryable(context, userContext)
                    .Where(x => x.IsAdmin && x.UserId != entity.UserId)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync();
                if (anyOtherAdminUser == null)
                    throw new SecurityException($"Your server must have at least one admin user.");
            }

            await context.UserTokens.Where(x => x.UserId == userId && x.OrganizationId == userContext.OrganizationId).ExecuteDeleteAsync();
            await context.UserLoginHistory.Where(x => x.UserId == userId && x.OrganizationId == userContext.OrganizationId).ExecuteDeleteAsync();
            await context.UserPrinterTemplateConfigurations.Where(x => x.UserId == userId && x.OrganizationId == userContext.OrganizationId).ExecuteDeleteAsync();
            await context.UserPrinterConfigurations.Where(x => x.UserId == userId && x.OrganizationId == userContext.OrganizationId).ExecuteDeleteAsync();
            await context.OAuthRequests.Where(x => x.UserId == userId && x.OrganizationId == userContext.OrganizationId).ExecuteDeleteAsync();
            await context.OAuthCredentials.Where(x => x.UserId == userId && x.OrganizationId == userContext.OrganizationId).ExecuteDeleteAsync();
            await context.CustomFieldValues.Where(x => x.RecordId == userId && x.CustomFieldTypeId == CustomFieldTypes.User && x.OrganizationId == userContext.OrganizationId).ExecuteDeleteAsync();

            context.Users.Remove(entity);

            return await context.SaveChangesAsync() > 0;
        }

        public virtual async Task<UserContext?> GetGlobalUserContextAsync(int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Users
                .Where(x => x.UserId == userId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();
            if (entity == null) return null;

            return new UserContext
            {
                EmailAddress = entity.EmailAddress,
                UserId = entity.UserId,
                OrganizationId = entity.OrganizationId,
                Name = entity.Name
            };
        }

        public virtual async Task<IUserContext?> ValidateUserImageToken(string token)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var userToken = await context.UserTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.TokenTypeId == Model.Authentication.TokenTypes.ImagesToken
                    && x.DateRevokedUtc == null
                    && x.Token == token
                    && x.DateExpiredUtc > DateTime.UtcNow);

            if (userToken != null && userToken.User != null)
            {
                return new UserContext
                {
                    UserId = userToken.User.UserId,
                    OrganizationId = userToken.User.OrganizationId,
                    EmailAddress = userToken.User.EmailAddress,
                    Name = userToken.User.Name,
                    PhoneNumber = userToken.User.PhoneNumber,
                    //SubscriptionLevel = null,
                    IsAdmin = false,
                    CanLogin = true
                };
            }

            return null;
        }
    }
}
