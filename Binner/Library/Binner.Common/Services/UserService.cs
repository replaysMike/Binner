using AutoMapper;
using Binner.Common.Authentication;
using Binner.Common.Extensions;
using Binner.Common.Services.Authentication;
using Binner.Data;
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
        private const int MinPasswordLength = 6;
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly RequestContextAccessor _requestContext;
        private readonly IConfiguration _configuration;

        public UserService(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, RequestContextAccessor requestContext, IConfigurationRoot configuration)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContext;
            _configuration = configuration;
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

        public async Task<ICollection<User>> GetUsersAsync(PaginatedRequest request)
        {
            var userContext = _requestContext.GetUserContext();
            var context = await _contextFactory.CreateDbContextAsync();
            var pageRecords = (request.Page - 1) * request.Results;
            var entities = await GetUserQueryable(context, userContext)
                // .OrderBy(request.OrderBy, request.Direction) // todo: ordering
                .Skip(pageRecords)
                .Take(request.Results)
                .AsSplitQuery()
                .ToListAsync();
            return _mapper.Map<ICollection<User>>(entities);
        }

        public async Task<User> GetUserAsync(User user)
        {
            var userContext = _requestContext.GetUserContext();
            var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetUserQueryable(context, userContext)
                .WhereIf(user.UserId > 0, x => x.UserId == user.UserId)
                .WhereIf(!string.IsNullOrEmpty(user.EmailAddress), x => x.EmailAddress == user.EmailAddress)
                .WhereIf(!string.IsNullOrEmpty(user.Name), x => x.Name == user.Name)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            var model = _mapper.Map<User>(entity);
            if (entity != null)
            {
                model.PartsInventoryCount = await context.Parts.CountAsync(x => x.UserId == user.UserId);
                model.PartTypesCount = await context.PartTypes.CountAsync(x => x.UserId == user.UserId);
            }

            return model;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Password))
                throw new InvalidOperationException("Password can not be empty!");
            if (user.Password.Length < MinPasswordLength)
                throw new ArgumentException($"Password must be at least {MinPasswordLength} characters in length!");
            var userContext = _requestContext.GetUserContext();
            var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetUserQueryable(context, userContext)
                .Where(x => x.EmailAddress == user.EmailAddress)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity != null)
                throw new KeyNotFoundException($"User already exists '{user.EmailAddress}'");

            entity = _mapper.Map(user, entity);
            if (string.IsNullOrEmpty(entity.EmailConfirmationToken))
                entity.EmailConfirmationToken = ConfirmationTokenGenerator.NewToken();
            entity.IsEmailConfirmed = true;
            entity.PasswordHash = PasswordHasher.GeneratePasswordHash(user.Password).ToString();
            entity.OrganizationId = userContext.OrganizationId;
            context.Users.Add(entity);

            await context.SaveChangesAsync();

            return _mapper.Map<User>(entity);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var userContext = _requestContext.GetUserContext();
            var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetUserQueryable(context, userContext)
                .Where(x => x.UserId == user.UserId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null)
                throw new KeyNotFoundException($"Could not find user with id '{user.UserId}'");

            entity = _mapper.Map(user, entity);
            if (string.IsNullOrEmpty(entity.EmailConfirmationToken))
                entity.EmailConfirmationToken = ConfirmationTokenGenerator.NewToken();

            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                if (user.Password.Length < MinPasswordLength)
                {
                    throw new ArgumentException($"Password must be at least {MinPasswordLength} characters in length!");
                }
                // reset password
                entity.PasswordHash = PasswordHasher.GeneratePasswordHash(user.Password).ToString();
            }
            entity.IsEmailConfirmed = true;
            entity.OrganizationId = userContext.OrganizationId;
            await context.SaveChangesAsync();

            return _mapper.Map<User>(entity);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            if (userId == 1)
                throw new SecurityException($"The root admin user cannot be deleted.");
            var userContext = _requestContext.GetUserContext();
            var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetUserQueryable(context, userContext)
                .Where(x => x.UserId == userId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null)
                throw new KeyNotFoundException($"Could not find user with id '{userId}'");

            context.UserTokens.RemoveRange(await context.UserTokens.Where(x => x.UserId == userId).ToListAsync());
            context.UserLoginHistory.RemoveRange(await context.UserLoginHistory.Where(x => x.UserId == userId).ToListAsync());
            context.UserPrinterTemplateConfigurations.RemoveRange(await context.UserPrinterTemplateConfigurations.Where(x => x.UserId == userId).ToListAsync());
            context.UserPrinterConfigurations.RemoveRange(await context.UserPrinterConfigurations.Where(x => x.UserId == userId).ToListAsync());
            context.OAuthRequests.RemoveRange(await context.OAuthRequests.Where(x => x.UserId == userId).ToListAsync());
            context.OAuthCredentials.RemoveRange(await context.OAuthCredentials.Where(x => x.UserId == userId).ToListAsync());

            context.Users.Remove(entity);

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<IUserContext?> ValidateUserImageToken(string token)
        {
            var context = await _contextFactory.CreateDbContextAsync();
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
