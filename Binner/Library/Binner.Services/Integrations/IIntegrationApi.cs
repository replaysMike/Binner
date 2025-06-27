﻿using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;

namespace Binner.Services.Integrations
{
    /// <summary>
    /// An integrated Api
    /// </summary>
    public interface IIntegrationApi : IDisposable
    {
        /// <summary>
        /// True if api is enabled
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>
        /// Api name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Api configuration
        /// </summary>
        public IApiConfiguration Configuration { get; }

        /// <summary>
        /// Search for a part
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="recordCount"></param>
        /// <param name="additionalOptions"></param>
        /// <returns></returns>
        Task<IApiResponse> SearchAsync(string partNumber, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null);

        /// <summary>
        /// Search for a part
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="partType"></param>
        /// <param name="recordCount"></param>
        /// <param name="additionalOptions"></param>
        /// <returns></returns>
        Task<IApiResponse> SearchAsync(string partNumber, string? partType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null);
        
        /// <summary>
        /// Search for a part
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="partType"></param>
        /// <param name="mountingType"></param>
        /// <param name="recordCount"></param>
        /// <param name="additionalOptions"></param>
        /// <returns></returns>
        Task<IApiResponse> SearchAsync(string partNumber, string? partType, string? mountingType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null);

        /// <summary>
        /// Get an order by it's orderId
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="additionalOptions"></param>
        /// <returns></returns>
        Task<IApiResponse> GetOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null);

        /// <summary>
        /// Get details about a part
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="additionalOptions"></param>
        /// <returns></returns>
        Task<IApiResponse> GetProductDetailsAsync(string partNumber, Dictionary<string, string>? additionalOptions = null);
    }
}
