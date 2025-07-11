﻿using Binner.Model;
using Binner.Model.Responses;

namespace Binner.Services
{
    public interface IAccountService<TAccount>
        where TAccount : Account, new()
    {
        /// <summary>
        /// Get the user's account
        /// </summary>
        /// <returns></returns>
        Task<TAccount?> GetAccountAsync();

        /// <summary>
        /// Update the user's account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<UpdateAccountResponse> UpdateAccountAsync(TAccount account);

        /// <summary>
        /// Upload a new datasheet
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="originalFilename"></param>
        /// <param name="contentType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        Task UploadProfileImageAsync(MemoryStream stream, string originalFilename, string contentType, long length);

        /// <summary>
        /// Create a KiCad api token
        /// </summary>
        /// <returns></returns>
        Task<Token?> CreateKiCadApiTokenAsync(string? tokenConfig);

        /// <summary>
        /// Delete a KiCad api token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> DeleteKiCadApiTokenAsync(string token);

        /// <summary>
        /// Get a user token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Token?> GetTokenAsync(string token, Model.Authentication.TokenTypes? tokenType = null);

        /// <summary>
        /// Validate a KiCad api token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> ValidateKiCadApiToken(string token);
    }
}