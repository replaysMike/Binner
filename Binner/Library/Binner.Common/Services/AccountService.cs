using AutoMapper;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataModel = Binner.Data.Model;

namespace Binner.Common.Services
{
    /// <summary>
    /// Manage users
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly RequestContextAccessor _requestContext;
        private readonly IConfiguration _configuration;

        public AccountService(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, RequestContextAccessor requestContext, IConfigurationRoot configuration)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContext;
            _configuration = configuration;
        }

        private IQueryable<DataModel.User> GetAccountQueryable(BinnerContext context)
            => context.Users
                .AsQueryable();

        public async Task<Account> GetAccountAsync()
        {
            var userContext = _requestContext.GetUserContext();
            var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetAccountQueryable(context)
                .Where(x => x.UserId == userContext.UserId && x.OrganizationId == userContext.OrganizationId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            var model = _mapper.Map<Account>(entity);
            if (entity != null)
            {
                model.PartsInventoryCount = await context.Parts.CountAsync(x => x.OrganizationId == userContext.OrganizationId);
                model.PartTypesCount = await context.PartTypes.CountAsync(x => x.OrganizationId == userContext.OrganizationId);
            }

            return model;
        }

        public async Task<UpdateAccountResponse> UpdateAccountAsync(Account account)
        {
            var userContext = _requestContext.GetUserContext();
            var context = await _contextFactory.CreateDbContextAsync();
            var entity = await GetAccountQueryable(context)
                .Where(x => x.UserId == userContext.UserId)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return new UpdateAccountResponse
                {
                    Account = _mapper.Map<Account>(entity),
                    IsSuccessful = false,
                    Message = $"Could not find user with id '{userContext.UserId}'",
                };
            }

            if (!string.IsNullOrEmpty(account.NewPassword))
            {
                if (!PasswordHasher.Verify(account.Password, entity.PasswordHash))
                {
                    return new UpdateAccountResponse
                    {
                        Account = _mapper.Map<Account>(entity),
                        IsSuccessful = false,
                        Message = "Incorrect password.",
                    };
                }

                if (!account.NewPassword.Equals(account.ConfirmNewPassword))
                    return new UpdateAccountResponse
                    {
                        Account = _mapper.Map<Account>(entity),
                        IsSuccessful = false,
                        Message = "Passwords do not match.",
                    };

                // change user's password
                entity.PasswordHash = PasswordHasher.GeneratePasswordHash(account.NewPassword).ToString();
            }

            entity = _mapper.Map(account, entity);

            await context.SaveChangesAsync();

            return new UpdateAccountResponse
            {
                Account = _mapper.Map<Account>(entity),
                IsSuccessful = true
            };
        }

        public async Task UploadProfileImageAsync(MemoryStream stream, string originalFilename, string contentType, long length)
        {
            var userContext = _requestContext.GetUserContext();
            var context = await _contextFactory.CreateDbContextAsync();
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
    }
}
