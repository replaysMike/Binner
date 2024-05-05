using Binner.Model;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Requests;
using Binner.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public interface IPartService
    {
        /// <summary>
        /// Add a new part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<Part> AddPartAsync(Part part);

        /// <summary>
        /// Update an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<Part> UpdatePartAsync(Part part);

        /// <summary>
        /// Delete an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<bool> DeletePartAsync(Part part);

        /// <summary>
        /// Find parts
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords);

        /// <summary>
        /// List all DigiKey categories
        /// </summary>
        /// <returns></returns>
        Task<IServiceResult<CategoriesResponse?>> GetCategoriesAsync();

        /// <summary>
        /// Get a part by part number
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Part?> GetPartAsync(GetPartRequest request);

        /// <summary>
        /// Get a part with its associated stored files
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<(Part? Part, ICollection<StoredFile> StoredFiles)> GetPartWithStoredFilesAsync(GetPartRequest request);

        /// <summary>
        /// Get all parts
        /// </summary>
        /// <returns></returns>
        Task<PaginatedResponse<Part>> GetPartsAsync(PaginatedRequest request);

        /// <summary>
        /// Get parts based on a condition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        Task<ICollection<Part>> GetPartsAsync(Expression<Func<Part, bool>> condition);

        /// <summary>
        /// Get a partType object, or create it if it doesn't exist
        /// </summary>
        /// <param name="partType"></param>
        /// <returns></returns>
        Task<PartType?> GetOrCreatePartTypeAsync(PartType partType);

        /// <summary>
        /// Get a partType by its id
        /// </summary>
        /// <param name="partTypeId"></param>
        /// <returns></returns>
        Task<PartType?> GetPartTypeAsync(int partTypeId);

        /// <summary>
        /// Get information about a barcode
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="barcodeType"></param>
        /// <returns></returns>
        Task<IServiceResult<PartResults?>> GetBarcodeInfoAsync(string barcode, ScannedBarcodeType barcodeType);

        /// <summary>
        /// Get metadata about a part number
        /// </summary>
        /// <param name="partNumber">Part number</param>
        /// <param name="partType">Part type</param>
        /// <param name="mountingType">Mounting type</param>
        /// <param name="supplierPartNumbers">Supplier's part number if known</param>
        /// <returns></returns>
        Task<IServiceResult<PartResults?>> GetPartInformationAsync(string partNumber, string partType = "", string mountingType = "", string supplierPartNumbers = "");

        /// <summary>
        /// Determine the part type
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<string> DeterminePartTypeAsync(CommonPart part);

        /// <summary>
        /// Determine the part type
        /// </summary>
        /// <param name="part"></param>
        /// <param name="partTypes"></param>
        /// <returns></returns>
        string DeterminePartType(CommonPart part, ICollection<PartType> partTypes);

        /// <summary>
        /// Get an external order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IServiceResult<ExternalOrderResponse?>> GetExternalOrderAsync(OrderImportRequest request);

        /// <summary>
        /// Get all part types
        /// </summary>
        /// <returns></returns>
        Task<ICollection<PartType>> GetPartTypesAsync();

        /// <summary>
        /// Get all part types and the number of parts in each type
        /// </summary>
        /// <returns></returns>
        Task<ICollection<PartTypeResponse>> GetPartTypesWithPartCountsAsync();

        /// <summary>
        /// Get count of all unique parts
        /// </summary>
        /// <returns></returns>
        Task<long> GetUniquePartsCountAsync();

        /// <summary>
        /// Get maximum number of parts allowed for the current user
        /// </summary>
        /// <returns></returns>
        public long GetUniquePartsMax();

        /// <summary>
        /// Get count/quantity of all parts
        /// </summary>
        /// <returns></returns>
        Task<long> GetPartsCountAsync();

        /// <summary>
        /// Get financial value/cost of all parts
        /// </summary>
        /// <returns></returns>
        Task<decimal> GetPartsValueAsync();

        /// <summary>
        /// Get count of all parts
        /// </summary>
        /// <returns></returns>
        Task<PaginatedResponse<Part>> GetLowStockAsync(PaginatedRequest request);

        /// <summary>
        /// Get a list of part suppliers for a part
        /// </summary>
        /// <param name="partId"></param>
        /// <returns></returns>
        Task<ICollection<PartSupplier>> GetPartSuppliersAsync(long partId);

        /// <summary>
        /// Add a new part supplier
        /// </summary>
        /// <param name="partSupplier"></param>
        /// <returns></returns>
        Task<PartSupplier> AddPartSupplierAsync(PartSupplier partSupplier);

        /// <summary>
        /// Update an existing part supplier
        /// </summary>
        /// <param name="partSupplier"></param>
        /// <returns></returns>
        Task<PartSupplier> UpdatePartSupplierAsync(PartSupplier partSupplier);

        /// <summary>
        /// Delete an existing part supplier
        /// </summary>
        /// <param name="partSupplier"></param>
        /// <returns></returns>
        Task<bool> DeletePartSupplierAsync(PartSupplier partSupplier);
    }
}
 