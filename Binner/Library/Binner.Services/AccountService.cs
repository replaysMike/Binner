﻿using AutoMapper;
using Binner.Common.Extensions;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using DataModel = Binner.Data.Model;

namespace Binner.Services
{
    /// <summary>
    /// Manage users
    /// </summary>
    public class AccountService<TAccount> : IAccountService<TAccount>
        where TAccount : Account, new()
    {
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly IRequestContextAccessor _requestContext;
        private readonly IConfiguration _configuration;

        public AccountService(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, IRequestContextAccessor requestContext, IConfigurationRoot configuration)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContext;
            _configuration = configuration;
        }

        protected virtual IQueryable<DataModel.User> GetAccountQueryable(BinnerContext context)
            => context.Users
                .AsQueryable();

        private bool IsUserTokenType(Model.Authentication.TokenTypes tokenType)
        {
            switch (tokenType) {
                case Model.Authentication.TokenTypes.KiCadApiToken:
                case Model.Authentication.TokenTypes.BinnerBinApiToken:
                    return true;
                default:
                    return false;
            }
        }

        public virtual async Task<TAccount?> GetAccountAsync()
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetAccountQueryable(context)
                .Include(x => x.UserTokens)
                .Where(x => x.UserId == userContext.UserId && x.OrganizationId == userContext.OrganizationId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity != null)
            {
                // filter out tokens we want
                entity.UserTokens = entity.UserTokens?.Where(x => IsUserTokenType(x.TokenTypeId)).ToList();
                var model = _mapper.Map<TAccount>(entity);
                model.PartsInventoryCount = await context.Parts.CountAsync(x => x.OrganizationId == userContext.OrganizationId);
                model.PartTypesCount = await context.PartTypes.CountAsync(x => x.OrganizationId == userContext.OrganizationId);
                return model;
            }

            return null;
        }

        public virtual async Task<UpdateAccountResponse> UpdateAccountAsync(TAccount account)
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetAccountQueryable(context)
                .Where(x => x.UserId == userContext.UserId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return new UpdateAccountResponse
                {
                    Account = _mapper.Map<TAccount>(entity),
                    IsSuccessful = false,
                    Message = $"Could not find user with id '{userContext.UserId}'",
                };
            }

            if (entity.EmailAddress != account.EmailAddress)
            {
                // user wants to change login email/username. Make sure it's not already in the system.
                var existingEmails = await GetAccountQueryable(context)
                .Where(x => x.EmailAddress == account.EmailAddress && x.OrganizationId == userContext.OrganizationId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();
                if (existingEmails != null)
                {
                    return new UpdateAccountResponse
                    {
                        Account = _mapper.Map<TAccount>(entity),
                        IsSuccessful = false,
                        Message = $"A user with this email already exists.",
                    };
                }
            }

            if (!string.IsNullOrEmpty(account.NewPassword))
            {
                if (!PasswordHasher.Verify(account.Password, entity.PasswordHash))
                {
                    return new UpdateAccountResponse
                    {
                        Account = _mapper.Map<TAccount>(entity),
                        IsSuccessful = false,
                        Message = "Incorrect password.",
                    };
                }

                if (!account.NewPassword.Equals(account.ConfirmNewPassword))
                    return new UpdateAccountResponse
                    {
                        Account = _mapper.Map<TAccount>(entity),
                        IsSuccessful = false,
                        Message = "Passwords do not match.",
                    };

                // change user's password
                entity.PasswordHash = PasswordHasher.GeneratePasswordHash(account.NewPassword).ToString();
            }

            account.Tokens = null; // ensure no tokens can be updated
            entity = _mapper.Map(account, entity);

            await context.SaveChangesAsync();

            return new UpdateAccountResponse
            {
                Account = _mapper.Map<TAccount>(entity),
                IsSuccessful = true
            };
        }

        public virtual async Task UploadProfileImageAsync(MemoryStream stream, string originalFilename, string contentType, long length)
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetAccountQueryable(context)
                .Where(x => x.UserId == userContext.UserId && x.OrganizationId == userContext.OrganizationId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null)
                throw new KeyNotFoundException($"Could not find user with id '{userContext.UserId}'");

            // convert image to png
            try
            {
                var image = Bitmap.FromStream(stream);
                using var outputStream = new MemoryStream();
                image.Save(outputStream, ImageFormat.Png);

                entity.ProfileImage = outputStream.ToArray();
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to save profile image", ex);
            }
        }

        public virtual async Task<Token?> CreateApiTokenAsync(Model.Authentication.TokenTypes tokenType, string? tokenConfig)
        {
            switch (tokenType) {
                case Model.Authentication.TokenTypes.KiCadApiToken:
                case Model.Authentication.TokenTypes.BinnerBinApiToken:
                    break;
                default:
                    throw new Exception($"Could not create token with type {tokenType}");
            }

            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();

            // check for limits for the kicad token
            if (tokenType == Model.Authentication.TokenTypes.KiCadApiToken) {
                var entity = await context.UserTokens
                    .Where(x => x.UserId == userContext.UserId
                        && x.OrganizationId == userContext.OrganizationId
                        && x.TokenTypeId == tokenType)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync();

                // delete any existing api token. user can only have 1
                if (entity != null)
                {
                    context.UserTokens.Remove(entity);
                }
            }

            var newToken = new DataModel.UserToken()
            {
                Token = TokenGenerator.NewToken(),
                TokenConfig = tokenConfig,
                DateCreatedUtc = DateTime.UtcNow,
                DateExpiredUtc = null,
                TokenTypeId = tokenType,
                UserId = userContext.UserId,
                OrganizationId = userContext.OrganizationId,
                Ip = _requestContext.GetIp()
            };
            context.UserTokens.Add(newToken);
            await context.SaveChangesAsync();

            return _mapper.Map<DataModel.UserToken, Token>(newToken);
        }

        public virtual async Task<bool> DeleteApiTokenAsync(Model.Authentication.TokenTypes tokenType, string token)
        {
            switch (tokenType) {
                case Model.Authentication.TokenTypes.KiCadApiToken:
                case Model.Authentication.TokenTypes.BinnerBinApiToken:
                    break;
                default:
                    throw new Exception($"Could not delete token with type {tokenType}");
            }

            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserTokens
                .Where(x => x.UserId == userContext.UserId
                    && x.OrganizationId == userContext.OrganizationId
                    && x.TokenTypeId == tokenType
                    && x.Token == token)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null) return false;

            context.UserTokens.Remove(entity);
            await context.SaveChangesAsync();

            return true;
        }

        public virtual async Task<Token?> GetTokenAsync(string token, Model.Authentication.TokenTypes? tokenType = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var apiToken = await context.UserTokens
                .WhereIf(tokenType != null, x => x.TokenTypeId == tokenType)
                .Where(x =>
                    x.DateRevokedUtc == null
                    && x.Token == token
                    && (x.DateExpiredUtc == null || x.DateExpiredUtc > DateTime.UtcNow))
                .FirstOrDefaultAsync();

            if (apiToken != null)
                return _mapper.Map<Token>(apiToken);

            return null;
        }

        public virtual async Task<bool> ValidateApiToken(Model.Authentication.TokenTypes tokenType, string token)
        {
            switch (tokenType) {
                case Model.Authentication.TokenTypes.KiCadApiToken:
                case Model.Authentication.TokenTypes.BinnerBinApiToken:
                    break;
                default:
                    throw new Exception($"Could not validate token with type {tokenType}");
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            var apiToken = await context.UserTokens
                .FirstOrDefaultAsync(x =>
                    x.TokenTypeId == tokenType
                    && x.DateRevokedUtc == null
                    && x.Token == token
                    && (x.DateExpiredUtc == null || x.DateExpiredUtc > DateTime.UtcNow));

            if (apiToken != null)
                return true;

            return false;
        }
    }
}
