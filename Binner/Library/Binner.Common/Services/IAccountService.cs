using Binner.Model;
using Binner.Model.Responses;
using System.IO;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public interface IAccountService
    {
        /// <summary>
        /// Get the user's account
        /// </summary>
        /// <returns></returns>
        Task<Account?> GetAccountAsync();

        /// <summary>
        /// Update the user's account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<UpdateAccountResponse> UpdateAccountAsync(Account account);

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
        Task<Token?> CreateKiCadApiTokenAsync();

        /// <summary>
        /// Delete a KiCad api token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> DeleteKiCadApiTokenAsync(string token);

        /// <summary>
        /// Validate a KiCad api token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> ValidateKiCadApiToken(string token);
    }
}