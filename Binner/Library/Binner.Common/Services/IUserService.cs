using System.Collections.Generic;
using System.Threading.Tasks;
using Binner.Common.Models;
using Binner.Model.Common;

namespace Binner.Common.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<User> CreateUserAsync(User user);

        /// <summary>
        /// Delete an existing user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> DeleteUserAsync(int userId);

        /// <summary>
        /// Get a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<User> GetUserAsync(User user);

        /// <summary>
        /// Get a list of users
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ICollection<User>> GetUsersAsync(PaginatedRequest request);

        /// <summary>
        /// Update existing user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<User> UpdateUserAsync(User user);

        /// <summary>
        /// Validate a user image token.
        /// </summary>
        /// <param name="token">Token to validate</param>
        /// <returns>null if token is invalid</returns>
        Task<IUserContext?> ValidateUserImageToken(string token);
    }
}