using Binner.Global.Common;
using Binner.Model.Requests;
using Binner.Model.Responses;
using System.Linq.Expressions;

namespace Binner.Model
{
    /// <summary>
    /// A part storage provider
    /// </summary>
    public interface IStorageProvider : IDisposable
    {
        /// <summary>
        /// Get an instance of the entire database
        /// </summary>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<IBinnerDb> GetDatabaseAsync(IUserContext? userContext);

        /// <summary>
        /// Test the database connection configuration
        /// </summary>
        /// <returns></returns>
        Task<ConnectionResponse> TestConnectionAsync();

        /// <summary>
        /// Create an (pending) oAuth request
        /// </summary>
        /// <param name="authRequest"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<OAuthAuthorization> CreateOAuthRequestAsync(OAuthAuthorization authRequest, IUserContext? userContext);

        /// <summary>
        /// Update a oAuth request
        /// </summary>
        /// <param name="authRequest"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<OAuthAuthorization?> UpdateOAuthRequestAsync(OAuthAuthorization authRequest, IUserContext? userContext);

        /// <summary>
        /// Get an existing (pending) oAuth request
        /// </summary>
        /// <param name="requestId">The request Id initiated the request</param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<OAuthAuthorization?> GetOAuthRequestAsync(Guid requestId, IUserContext? userContext);

        /// <summary>
        /// Get an oAuth Credential
        /// </summary>
        /// <param name="providerName">The provider name to fetch</param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<OAuthCredential?> GetOAuthCredentialAsync(string providerName, IUserContext? userContext);

        /// <summary>
        /// Save an oAuth Credential
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<OAuthCredential?> SaveOAuthCredentialAsync(OAuthCredential credential, IUserContext? userContext);

        /// <summary>
        /// Remove an oAuth Credential
        /// </summary>
        /// <param name="providerName">The provider name to fetch</param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task RemoveOAuthCredentialAsync(string providerName, IUserContext? userContext);

        /// <summary>
        /// Add a new part
        /// </summary>
        /// <param name="part"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<Part> AddPartAsync(Part part, IUserContext? userContext);

        /// <summary>
        /// Update an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<Part?> UpdatePartAsync(Part part, IUserContext? userContext);

        /// <summary>
        /// Get a part by its internal id
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<Part?> GetPartAsync(long partId, IUserContext? userContext);

        /// <summary>
        /// Get a part by part number
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<Part?> GetPartAsync(string partNumber, IUserContext? userContext);

        /// <summary>
        /// Get a part by its short id
        /// </summary>
        /// <param name="shortId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<Part?> GetPartByShortIdAsync(string shortId, IUserContext? userContext);

        /// <summary>
        /// Get all parts
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<PaginatedResponse<Part>> GetPartsAsync(PaginatedRequest request, IUserContext? userContext);

        /// <summary>
        /// Get all parts
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<ICollection<Part>> GetPartsAsync(Expression<Func<Part, bool>> predicate, IUserContext? userContext);

        /// <summary>
        /// Find a part that matches any keyword
        /// </summary>
        /// <param name="keywords">Space separated keywords</param>
        /// <param name="userContext">The user performing the operation</param>

        /// <returns></returns>
        Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords, IUserContext? userContext);

        /// <summary>
        /// Delete an existing part
        /// </summary>
        /// <param name="part"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<bool> DeletePartAsync(Part part, IUserContext? userContext);

        /// <summary>
        /// Get a partType object, or create it if it doesn't exist
        /// </summary>
        /// <param name="partType"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<PartType?> GetOrCreatePartTypeAsync(PartType partType, IUserContext? userContext);

        /// <summary>
        /// Get an existing part type
        /// </summary>
        /// <param name="partTypeId"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<PartType?> GetPartTypeAsync(long partTypeId, IUserContext? userContext);

        /// <summary>
        /// Get an existing part type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<PartType?> GetPartTypeAsync(string name, IUserContext? userContext);

        /// <summary>
        /// Update an existing part type
        /// </summary>
        /// <param name="partType"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<PartType?> UpdatePartTypeAsync(PartType partType, IUserContext? userContext);

        /// <summary>
        /// Delete an existing partType
        /// </summary>
        /// <param name="partType"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<bool> DeletePartTypeAsync(PartType partType, IUserContext? userContext);

        /// <summary>
        /// Get all of the part types
        /// </summary>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<ICollection<PartType>> GetPartTypesAsync(IUserContext? userContext);

        /// <summary>
        /// Get all of the part types
        /// </summary>
        /// <param name="filterEmpty">True to filter empty part type categories (no parts assigned)</param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<ICollection<PartType>> GetPartTypesAsync(bool filterEmpty, IUserContext? userContext);

        /// <summary>
        /// Create a new user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<Project?> AddProjectAsync(Project project, IUserContext? userContext);

        /// <summary>
        /// Get an existing user defined project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<Project?> GetProjectAsync(long projectId, IUserContext? userContext);

        /// <summary>
        /// Get an existing user defined project
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<Project?> GetProjectAsync(string projectName, IUserContext? userContext);

        /// <summary>
        /// Get an existing user defined project
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<ICollection<Project>> GetProjectsAsync(PaginatedRequest request, IUserContext? userContext);

        /// <summary>
        /// Update an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<Project?> UpdateProjectAsync(Project project, IUserContext? userContext);

        /// <summary>
        /// Update an existing user defined project
        /// </summary>
        /// <param name="project"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<bool> DeleteProjectAsync(Project project, IUserContext? userContext);

        /// <summary>
        /// Increment the quantity of a part by a specified amount
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<Part?> IncrementQuantityAsync(PartQuantityRequest request, IUserContext? userContext);

        /// <summary>
        /// Decrement the quantity of a part by a specified amount
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<Part?> DecrementQuantityAsync(PartQuantityRequest request, IUserContext? userContext);

        /// <summary>
        /// Get total parts count including quantities
        /// </summary>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<long> GetPartsCountAsync(IUserContext? userContext);

        /// <summary>
        /// Get total number of unique parts
        /// </summary>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<long> GetUniquePartsCountAsync(IUserContext? userContext);

        /// <summary>
        /// Get financial value/cost of all parts
        /// </summary>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<decimal> GetPartsValueAsync(IUserContext? userContext);

        /// <summary>
        /// Get low stock
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<PaginatedResponse<Part>> GetLowStockAsync(PaginatedRequest request, IUserContext? userContext);

        /// <summary>
        /// Create a new user uploaded file
        /// </summary>
        /// <param name="storedFile"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<StoredFile> AddStoredFileAsync(StoredFile storedFile, IUserContext? userContext);

        /// <summary>
        /// Get an existing user uploaded file
        /// </summary>
        /// <param name="storedFileId"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<StoredFile?> GetStoredFileAsync(long storedFileId, IUserContext? userContext);

        /// <summary>
        /// Get an existing user uploaded file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<StoredFile?> GetStoredFileAsync(string filename, IUserContext? userContext);

        /// <summary>
        /// Get existing user uploaded files
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="fileType"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<ICollection<StoredFile>> GetStoredFilesAsync(long partId, StoredFileType? fileType, IUserContext? userContext);

        /// <summary>
        /// Get existing user uploaded files
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<ICollection<StoredFile>> GetStoredFilesAsync(PaginatedRequest request, IUserContext? userContext);

        /// <summary>
        /// Update existing user uploaded files
        /// </summary>
        /// <param name="storedFile"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<StoredFile?> UpdateStoredFileAsync(StoredFile storedFile, IUserContext? userContext);

        /// <summary>
        /// Delete existing user uploaded file
        /// </summary>
        /// <param name="storedFile"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<bool> DeleteStoredFileAsync(StoredFile storedFile, IUserContext? userContext);

        /// <summary>
        /// Get a pcb BOM
        /// </summary>
        /// <param name="pcbId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<Pcb?> GetPcbAsync(long pcbId, IUserContext? userContext);

        /// <summary>
        /// Get a list of pcb's for a project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ICollection<Pcb>> GetPcbsAsync(long projectId, IUserContext? userContext);

        /// <summary>
        /// Add a pcb BOM
        /// </summary>
        /// <param name="pcb"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<Pcb> AddPcbAsync(Pcb pcb, IUserContext? userContext);

        /// <summary>
        /// Update pcb in a Project BOM
        /// </summary>
        /// <param name="pcb"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<Pcb?> UpdatePcbAsync(Pcb pcb, IUserContext? userContext);

        /// <summary>
        /// Remove a pcb from a Project BOM
        /// </summary>
        /// <param name="pcb"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<bool> DeletePcbAsync(Pcb pcb, IUserContext? userContext);

        /// <summary>
        /// Get a stored file assignment BOM
        /// </summary>
        /// <param name="pcbStoredFileAssignmentId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<PcbStoredFileAssignment?> GetPcbStoredFileAssignmentAsync(long pcbStoredFileAssignmentId, IUserContext? userContext);

        /// <summary>
        /// Get a list of stored file assignments for a pcb BOM
        /// </summary>
        /// <param name="pcbId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ICollection<PcbStoredFileAssignment>> GetPcbStoredFileAssignmentsAsync(long pcbId, IUserContext? userContext);

        /// <summary>
        /// Add a stored file to a Pcb BOM
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<PcbStoredFileAssignment> AddPcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext);

        /// <summary>
        /// Update pcb stored file assignment  BOM
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<PcbStoredFileAssignment?> UpdatePcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext);

        /// <summary>
        /// Remove a pcb stored file assignment BOM
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<bool> RemovePcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext);

        /// <summary>
        /// Get a project part assignment BOM based on the part only
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ICollection<ProjectPartAssignment>> GetPartAssignmentsAsync(long partId, IUserContext? userContext);

        /// <summary>
        /// Get a project part assignment BOM
        /// </summary>
        /// <param name="projectPartAssignmentId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectPartAssignmentId, IUserContext? userContext);

        /// <summary>
        /// Get a project part assignment BOM
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="partId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectId, long partId, IUserContext? userContext);

        /// <summary>
        /// Get a project part assignment BOM
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="partName"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectId, string partName, IUserContext? userContext);

        /// <summary>
        /// Get a list of project part assignments for a project BOM
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ICollection<ProjectPartAssignment>> GetProjectPartAssignmentsAsync(long projectId, IUserContext? userContext);

        /// <summary>
        /// Get a list of project part assignments for a project BOM
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="request"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<ICollection<ProjectPartAssignment>> GetProjectPartAssignmentsAsync(long projectId, PaginatedRequest request, IUserContext? userContext);

        /// <summary>
        /// Add a part to a Project BOM
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ProjectPartAssignment> AddProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext);

        /// <summary>
        /// Update part in a Project BOM
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ProjectPartAssignment?> UpdateProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext);

        /// <summary>
        /// Remove a part from a Project BOM
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<bool> RemoveProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext);

        /// <summary>
        /// Get a project pcb assignment BOM
        /// </summary>
        /// <param name="projectPcbAssignmentId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ProjectPcbAssignment?> GetProjectPcbAssignmentAsync(long projectPcbAssignmentId, IUserContext? userContext);

        /// <summary>
        /// Get a list of project pcb assignments for a project BOM
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ICollection<ProjectPcbAssignment>> GetProjectPcbAssignmentsAsync(long projectId, IUserContext? userContext);

        /// <summary>
        /// Add a pcb to a Project BOM
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ProjectPcbAssignment> AddProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext);

        /// <summary>
        /// Update pcb in a Project BOM
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ProjectPcbAssignment?> UpdateProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext);

        /// <summary>
        /// Remove a pcb from a Project BOM
        /// </summary>
        /// <param name="assignment"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<bool> RemoveProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext);

        /// <summary>
        /// Get a part scan history record
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<PartScanHistory?> GetPartScanHistoryAsync(PartScanHistory partScanHistory, IUserContext? userContext);

        /// <summary>
        /// Get a part scan history record by it's rawscan field
        /// </summary>
        /// <param name="rawScan"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<PartScanHistory?> GetPartScanHistoryAsync(string rawScan, IUserContext? userContext);

        /// <summary>
        /// Get a part scan history record by its CRC of the RawScan field
        /// </summary>
        /// <param name="rawScanCrc"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<PartScanHistory?> GetPartScanHistoryAsync(int rawScanCrc, IUserContext? userContext);

        /// <summary>
        /// Get a part scan history record
        /// </summary>
        /// <param name="partScanHistoryId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<PartScanHistory?> GetPartScanHistoryAsync(long partScanHistoryId, IUserContext? userContext);

        /// <summary>
        /// Add a new part scan history record
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<PartScanHistory> AddPartScanHistoryAsync(PartScanHistory partScanHistory, IUserContext? userContext);

        /// <summary>
        /// Update an existing part scan history record
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<PartScanHistory?> UpdatePartScanHistoryAsync(PartScanHistory partScanHistory, IUserContext? userContext);

        /// <summary>
        /// Delete an existing part scan history record
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<bool> DeletePartScanHistoryAsync(PartScanHistory partScanHistory, IUserContext? userContext);

        /// <summary>
        /// Create a new part supplier
        /// </summary>
        /// <param name="partSupplier"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<PartSupplier> AddPartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext);

        /// <summary>
        /// Get an existing part supplier
        /// </summary>
        /// <param name="partSupplierId"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<PartSupplier?> GetPartSupplierAsync(long partSupplierId, IUserContext? userContext);

        /// <summary>
        /// Get existing part suppliers for a part
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<ICollection<PartSupplier>> GetPartSuppliersAsync(long partId, IUserContext? userContext);

        /// <summary>
        /// Update an existing part supplier
        /// </summary>
        /// <param name="partSupplier"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<PartSupplier?> UpdatePartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext);

        /// <summary>
        /// Delete existing part supplier
        /// </summary>
        /// <param name="partSupplier"></param>
        /// <param name="userContext">The user performing the operation</param>
        /// <returns></returns>
        Task<bool> DeletePartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext);

        /// <summary>
        /// Get an order import history record
        /// </summary>
        /// <param name="orderImportHistory"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<OrderImportHistory?> GetOrderImportHistoryAsync(OrderImportHistory orderImportHistory, bool includeChildren, IUserContext? userContext);

        /// <summary>
        /// Get an order import history record
        /// </summary>
        /// <param name="orderImportHistoryId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<OrderImportHistory?> GetOrderImportHistoryAsync(long orderImportHistoryId, bool includeChildren, IUserContext? userContext);

        /// <summary>
        /// Get an order import history record
        /// </summary>
        /// <param name="orderImportHistoryId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<OrderImportHistory?> GetOrderImportHistoryAsync(string orderNumber, string supplier, bool includeChildren, IUserContext? userContext);

        /// <summary>
        /// Add a new order import history record
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<OrderImportHistory> AddOrderImportHistoryAsync(OrderImportHistory partScanHistory, IUserContext? userContext);

        /// <summary>
        /// Add a new order import history line item
        /// </summary>
        /// <param name="orrderImportHistoryLineItem"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<OrderImportHistoryLineItem> AddOrderImportHistoryLineItemAsync(OrderImportHistoryLineItem orrderImportHistoryLineItem, IUserContext? userContext);

        /// <summary>
        /// Update an existing order import history record
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<OrderImportHistory?> UpdateOrderImportHistoryAsync(OrderImportHistory partScanHistory, IUserContext? userContext);

        /// <summary>
        /// Delete an existing order import history record
        /// </summary>
        /// <param name="partScanHistory"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<bool> DeleteOrderImportHistoryAsync(OrderImportHistory partScanHistory, IUserContext? userContext);

        /// <summary>
        /// Given a list of part numbers, get the local part ids if they match
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<IDictionary<string, long>> GetPartIdsFromManufacturerPartNumbersAsync(ICollection<string> partNumbers, IUserContext? userContext);

        /// <summary>
        /// Get all parts that belong to a part type
        /// </summary>
        /// <param name="partType"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ICollection<Part>> GetPartsByPartTypeAsync(PartType partType, IUserContext? userContext);

        /// <summary>
        /// Get the custom fields defined
        /// </summary>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ICollection<CustomField>> GetCustomFieldsAsync(IUserContext? userContext);

        /// <summary>
        /// Get the custom field values for a record
        /// </summary>
        /// <param name="customFieldType"></param>
        /// <param name="recordId"></param>
        /// <param name="userContext"></param>
        /// <returns></returns>
        Task<ICollection<CustomValue>> GetCustomFieldsAsync(CustomFieldTypes customFieldType, long recordId, IUserContext? userContext);

        /// <summary>
        /// Perform a save of custom fields. All items will be added/updated/removed based on the values passed in.
        /// </summary>
        /// <param name="customFields"></param>
        /// <returns></returns>
        Task<ICollection<CustomField>> SaveCustomFieldsAsync(ICollection<CustomField> customFields, IUserContext? userContext);

        /// <summary>
        /// Reset user credentials to empty password (Admin only)
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<bool> ResetUserCredentialsAsync(string username);

        /// <summary>
        /// Get the number of users
        /// </summary>
        /// <returns></returns>
        Task<int> GetUserCountAsync();

        /// <summary>
        /// Get the number of admin users
        /// </summary>
        /// <returns></returns>
        Task<int> GetUserAdminCountAsync();

        /// <summary>
        /// Ping the database to ensure it is reachable and operational
        /// </summary>
        /// <returns></returns>
        Task<bool> PingDatabaseAsync();
    }
}
