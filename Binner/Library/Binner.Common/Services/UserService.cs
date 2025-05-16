using AutoMapper;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using DataModel = Binner.Data.Model;

namespace Binner.Common.Services
{
    /// <summary>
    /// Manage users
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly IRequestContextAccessor _requestContext;
        private readonly IConfiguration _configuration;
        private readonly ILicensedService _licensedService;

        public UserService(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, IRequestContextAccessor requestContext, IConfigurationRoot configuration, ILicensedService licensedService)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContext;
            _configuration = configuration;
            _licensedService = licensedService;
        }

        private IQueryable<DataModel.User> GetUserQueryable(BinnerContext context, IUserContext userContext)
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

        public Task<User> CreateUserAsync(User user) => _licensedService.CreateUserAsync(user);

        public Task<User?> GetUserAsync(User user) => _licensedService.GetUserAsync(user);

        public Task<ICollection<User>> GetUsersAsync(PaginatedRequest request) => _licensedService.GetUsersAsync(request);

        public Task<User?> UpdateUserAsync(User user) => _licensedService.UpdateUserAsync(user);

        public async Task<bool> DeleteUserAsync(int userId)
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

        public async Task<UserContext?> GetGlobalUserContextAsync(int userId)
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

        public async Task<IUserContext?> ValidateUserImageToken(string token)
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
