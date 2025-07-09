using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Responses;

namespace Binner.Services
{
    public interface IUserService<TUser>
        where TUser : class, IUser
    {
        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<TUser> CreateUserAsync(TUser user);

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
        Task<TUser?> GetUserAsync(TUser user);

        /// <summary>
        /// Get a list of users
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PaginatedResponse<TUser>> GetUsersAsync(PaginatedRequest request);

        /// <summary>
        /// Update existing user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<TUser?> UpdateUserAsync(TUser user);

        /// <summary>
        /// Validate a user image token.
        /// </summary>
        /// <param name="token">Token to validate</param>
        /// <returns>null if token is invalid</returns>
        Task<IUserContext?> ValidateUserImageToken(string token);

        /// <summary>
        /// Get the global user context for a user id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserContext?> GetGlobalUserContextAsync(int userId);
    }
}