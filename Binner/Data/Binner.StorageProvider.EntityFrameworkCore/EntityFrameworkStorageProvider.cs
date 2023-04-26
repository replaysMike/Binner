using AutoMapper;
using Binner.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Binner.Global.Common;
using TypeSupport.Extensions;
using Binner.Model;
using Binner.Model.Responses;
using DataModel = Binner.Data.Model;
using Binner.LicensedProvider;

namespace Binner.StorageProvider.EntityFrameworkCore
{
    public class EntityFrameworkStorageProvider : IStorageProvider
    {
        public const string ProviderName = "EntityFramework";
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly string _providerName;
        private readonly IDictionary<string, string> _config;
        private readonly IMapper _mapper;
        private readonly ILicensedStorageProvider _licensedStorageProvider;

        public EntityFrameworkStorageProvider(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, string providerName, IDictionary<string, string> config, ILicensedStorageProvider licensedStorageProvider)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _providerName = providerName;
            _config = config;
            _licensedStorageProvider = licensedStorageProvider;
        }

        public async Task<Part> AddPartAsync(Part part, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.Part>(part);
            EnforceIntegrityCreate(entity, userContext);
            context.Parts.Add(entity);
            await context.SaveChangesAsync();
            part.PartId = entity.PartId;
            return part;
        }

        public async Task<Project?> AddProjectAsync(Project project, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.Project>(project);
            EnforceIntegrityCreate(entity, userContext);
            context.Projects.Add(entity);
            await context.SaveChangesAsync();
            project.ProjectId = entity.ProjectId;
            return project;
        }

        public async Task<bool> DeletePartAsync(Part part, IUserContext userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts.FirstOrDefaultAsync(x => x.PartId == part.PartId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return false;
            EnforceIntegrityCreate(entity, userContext);
            context.Parts.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePartTypeAsync(PartType partType, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();

            var entity = await context.PartTypes.FirstOrDefaultAsync(x => x.PartTypeId == partType.PartTypeId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return false;

            // does it have children?
            var hasChildren = await context.PartTypes.AnyAsync(x => x.ParentPartTypeId == partType.PartTypeId && x.OrganizationId == userContext.OrganizationId);
            if (hasChildren)
                throw new InvalidOperationException("Can't delete part type that has children. You must delete the children first.");

            // dereference any parts with this part type
            var partsWithPartType = await context.Parts.Where(x => x.PartTypeId == partType.PartTypeId).ToListAsync();
            // set it to the Other part type if it exists
            var newDefaultPartType = await context.PartTypes
                .Where(x => x.PartTypeId != partType.PartTypeId && x.PartTypeId == (long)SystemDefaults.DefaultPartTypes.Other)
                .FirstOrDefaultAsync();
            if (newDefaultPartType == null)
            {
                // if all else fails, set it to the first part type available
                newDefaultPartType = await context.PartTypes
                    .Where(x => x.PartTypeId != partType.PartTypeId)
                    .OrderBy(x => x.PartTypeId)
                    .FirstAsync();
            }
            foreach (var part in partsWithPartType)
                part.PartTypeId = newDefaultPartType.PartTypeId;

            context.PartTypes.Remove(entity);

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProjectAsync(Project project, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Projects.FirstOrDefaultAsync(x => x.ProjectId == project.ProjectId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return false;
            context.Projects.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            // todo: this search is inefficient in EF but don't know how to convert it yet
            var query = GetFindPartsQueryForProvider(keywords, userContext);
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.Parts.FromSql(query)
                .ToListAsync();
            return entities.Select(x => new SearchResult<Part>(_mapper.Map<Part>(x), 0)).ToList();
        }

        private FormattableString GetFindPartsQueryForProvider(string keywords, IUserContext userContext)
        {
            switch (_providerName.ToLower())
            {
                case "postgresql":
                    return GetPostgresqlFindPartsQuery(keywords, userContext);
                case "mysql":
                    return GetMySqlFindPartsQuery(keywords, userContext);
                case "binner":
                case "sqlite":
                    return GetSqliteFindPartsQuery(keywords, userContext);
                default:
                    return GetDefaultFindPartsQuery(keywords, userContext);
            }
        }

        private FormattableString GetDefaultFindPartsQuery(string keywords, IUserContext userContext)
        {
            // note: this is not injectable, using a formattable string which is a special case for FromSql()
            return FormattableStringFactory.Create(
                $@"WITH PartsExactMatch (PartId, Rank) AS
(
SELECT PartId, 10 as Rank FROM Parts WHERE UserId = {userContext.UserId} AND (
PartNumber = '{keywords}'
 OR DigiKeyPartNumber = '{keywords}'
 OR MouserPartNumber = '{keywords}'
 OR ManufacturerPartNumber = '{keywords}'
 OR Description = '{keywords}'
 OR Keywords = '{keywords}'
 OR Location = '{keywords}'
 OR BinNumber = '{keywords}'
 OR BinNumber2 = '{keywords}')
),
PartsBeginsWith (PartId, Rank) AS
(
SELECT PartId, 100 as Rank FROM Parts WHERE UserId = {userContext.UserId} AND (
PartNumber LIKE '{keywords}'  + '%'
 OR DigiKeyPartNumber LIKE '{keywords}' + '%'
 OR MouserPartNumber LIKE '{keywords}' + '%'
 OR ManufacturerPartNumber LIKE '{keywords}' + '%'
 OR Description LIKE '{keywords}' + '%'
 OR Keywords LIKE '{keywords}' + '%'
 OR Location LIKE '{keywords}' + '%'
 OR BinNumber LIKE '{keywords}' + '%'
 OR BinNumber2 LIKE '{keywords}'+ '%')
),
PartsAny (PartId, Rank) AS
(
SELECT PartId, 200 as Rank FROM Parts WHERE UserId = {userContext.UserId} AND (
PartNumber LIKE '%' + '{keywords}' + '%'
 OR DigiKeyPartNumber LIKE '%' + '{keywords}' + '%'
 OR MouserPartNumber LIKE '%' + '{keywords}' + '%'
 OR ManufacturerPartNumber LIKE '%' + '{keywords}' + '%'
 OR Description LIKE '%' + '{keywords}' + '%'
 OR Keywords LIKE '%' + '{keywords}' + '%'
 OR Location LIKE '%' + '{keywords}' + '%'
 OR BinNumber LIKE '%' + '{keywords}' + '%'
 OR BinNumber2 LIKE '%' + '{keywords}' + '%')
),
PartsMerged (PartId, Rank) AS
(
	SELECT PartId, Rank FROM PartsExactMatch
	UNION
	SELECT PartId, Rank FROM PartsBeginsWith
	UNION
	SELECT PartId, Rank FROM PartsAny
)
SELECT p.* FROM Parts p
INNER JOIN (
  SELECT PartId, MIN(Rank) Rank FROM PartsMerged GROUP BY PartId
) pm ON pm.PartId = p.PartId ORDER BY pm.Rank ASC;
;");
        }

        private FormattableString GetSqliteFindPartsQuery(string keywords, IUserContext userContext)
        {
            // note: this is not injectable, using a formattable string which is a special case for FromSql()
            return FormattableStringFactory.Create(
                $@"WITH PartsExactMatch (PartId, Rank) AS
(
SELECT PartId, 10 as Rank FROM Parts WHERE UserId = {userContext.UserId} AND (
PartNumber = '{keywords}'
 OR DigiKeyPartNumber = '{keywords}'
 OR MouserPartNumber = '{keywords}'
 OR ManufacturerPartNumber = '{keywords}'
 OR Description = '{keywords}'
 OR Keywords = '{keywords}'
 OR Location = '{keywords}'
 OR BinNumber = '{keywords}'
 OR BinNumber2 = '{keywords}')
),
PartsBeginsWith (PartId, Rank) AS
(
SELECT PartId, 100 as Rank FROM Parts WHERE UserId = {userContext.UserId} AND (
PartNumber LIKE '{keywords}' || '%'
 OR DigiKeyPartNumber LIKE '{keywords}' || '%'
 OR MouserPartNumber LIKE '{keywords}' || '%'
 OR ManufacturerPartNumber LIKE '{keywords}' || '%'
 OR Description LIKE '{keywords}' || '%'
 OR Keywords LIKE '{keywords}' || '%'
 OR Location LIKE '{keywords}' || '%'
 OR BinNumber LIKE '{keywords}' || '%'
 OR BinNumber2 LIKE '{keywords}' || '%')
),
PartsAny (PartId, Rank) AS
(
SELECT PartId, 200 as Rank FROM Parts WHERE UserId = {userContext.UserId} AND (
PartNumber LIKE '%' + '{keywords}' || '%'
 OR DigiKeyPartNumber LIKE '%' + '{keywords}' || '%'
 OR MouserPartNumber LIKE '%' || '{keywords}' || '%'
 OR ManufacturerPartNumber LIKE '%' || '{keywords}' || '%'
 OR Description LIKE '%' || '{keywords}' || '%'
 OR Keywords LIKE '%' || '{keywords}' || '%'
 OR Location LIKE '%' || '{keywords}' || '%'
 OR BinNumber LIKE '%' || '{keywords}' || '%'
 OR BinNumber2 LIKE '%' || '{keywords}' || '%')
),
PartsMerged (PartId, Rank) AS
(
	SELECT PartId, Rank FROM PartsExactMatch
	UNION
	SELECT PartId, Rank FROM PartsBeginsWith
	UNION
	SELECT PartId, Rank FROM PartsAny
)
SELECT p.* FROM Parts p
INNER JOIN (
  SELECT PartId, MIN(Rank) Rank FROM PartsMerged GROUP BY PartId
) pm ON pm.PartId = p.PartId ORDER BY pm.Rank ASC;
;");
        }

        private FormattableString GetMySqlFindPartsQuery(string keywords, IUserContext userContext)
        {
            // note: this is not injectable, using a formattable string which is a special case for FromSql()
            return FormattableStringFactory.Create(
                $@"WITH PartsExactMatch (PartId, OrderRank) AS
(
SELECT PartId, 10 as OrderRank FROM Parts WHERE UserId = {userContext.UserId} AND (
PartNumber = '{keywords}'
 OR DigiKeyPartNumber = '{keywords}'
 OR MouserPartNumber = '{keywords}'
 OR ManufacturerPartNumber = '{keywords}'
 OR Description = '{keywords}'
 OR Keywords = '{keywords}'
 OR Location = '{keywords}'
 OR BinNumber = '{keywords}'
 OR BinNumber2 = '{keywords}')
),
PartsBeginsWith (PartId, OrderRank) AS
(
SELECT PartId, 100 as OrderRank FROM Parts WHERE UserId = {userContext.UserId} AND (
PartNumber LIKE CONCAT('{keywords}','%')
 OR DigiKeyPartNumber LIKE CONCAT('{keywords}','%')
 OR MouserPartNumber LIKE CONCAT('{keywords}','%')
 OR ManufacturerPartNumber LIKE CONCAT('{keywords}','%')
 OR Description LIKE CONCAT('{keywords}','%')
 OR Keywords LIKE CONCAT('{keywords}','%')
 OR Location LIKE CONCAT('{keywords}','%')
 OR BinNumber LIKE CONCAT('{keywords}','%')
 OR BinNumber2 LIKE CONCAT('{keywords}','%'))
),
PartsAny (PartId, OrderRank) AS
(
SELECT PartId, 200 as OrderRank FROM Parts WHERE UserId = {userContext.UserId} AND (
PartNumber LIKE CONCAT('%','{keywords}','%')
 OR DigiKeyPartNumber LIKE CONCAT('%','{keywords}','%')
 OR MouserPartNumber LIKE CONCAT('%','{keywords}','%')
 OR ManufacturerPartNumber LIKE CONCAT('%','{keywords}','%')
 OR Description LIKE CONCAT('%','{keywords}','%')
 OR Keywords LIKE CONCAT('%','{keywords}','%')
 OR Location LIKE CONCAT('%','{keywords}','%')
 OR BinNumber LIKE CONCAT('%','{keywords}','%')
 OR BinNumber2 LIKE CONCAT('%','{keywords}','%'))
),
PartsMerged (PartId, OrderRank) AS
(
	SELECT PartId, OrderRank FROM PartsExactMatch
	UNION
	SELECT PartId, OrderRank FROM PartsBeginsWith
	UNION
	SELECT PartId, OrderRank FROM PartsAny
)
SELECT p.* FROM Parts p
INNER JOIN (
  SELECT PartId, MIN(OrderRank) OrderRank FROM PartsMerged GROUP BY PartId
) pm ON pm.PartId = p.PartId ORDER BY pm.OrderRank ASC;
;");
        }

        private FormattableString GetPostgresqlFindPartsQuery(string keywords, IUserContext userContext)
        {
            // note: this is not injectable, using a formattable string which is a special case for FromSql()
            return FormattableStringFactory.Create(
                $@"WITH ""PartsExactMatch"" (""PartId"", ""Rank"") AS
(
SELECT ""PartId"", 10 as ""Rank"" FROM dbo.""Parts"" WHERE ""UserId"" = {userContext.UserId} AND (
""PartNumber"" ILIKE '{keywords}' 
OR ""DigiKeyPartNumber"" ILIKE '{keywords}' 
OR ""MouserPartNumber"" ILIKE '{keywords}'
OR ""ArrowPartNumber"" ILIKE '{keywords}'
OR ""ManufacturerPartNumber"" ILIKE '{keywords}'
OR ""Description"" ILIKE '{keywords}' 
OR ""Keywords"" ILIKE '{keywords}' 
OR ""Location"" ILIKE '{keywords}' 
OR ""BinNumber"" ILIKE '{keywords}' 
OR ""BinNumber2"" ILIKE '{keywords}')
),
""PartsBeginsWith"" (""PartId"", ""Rank"") AS
(
SELECT ""PartId"", 100 as ""Rank"" FROM dbo.""Parts"" WHERE ""UserId"" = {userContext.UserId} AND (
""PartNumber"" ILIKE CONCAT('{keywords}', '%')
OR ""DigiKeyPartNumber"" ILIKE CONCAT('{keywords}', '%')
OR ""MouserPartNumber"" ILIKE CONCAT('{keywords}', '%')
OR ""ArrowPartNumber"" ILIKE CONCAT('{keywords}', '%')
OR ""ManufacturerPartNumber"" ILIKE CONCAT('{keywords}', '%')
OR ""Description"" ILIKE CONCAT('{keywords}', '%')
OR ""Keywords"" ILIKE CONCAT('{keywords}', '%')
OR ""Location"" ILIKE CONCAT('{keywords}', '%')
OR ""BinNumber"" ILIKE CONCAT('{keywords}', '%')
OR ""BinNumber2"" ILIKE CONCAT('{keywords}', '%'))
),
""PartsAny"" (""PartId"", ""Rank"") AS
(
SELECT ""PartId"", 200 as ""Rank"" FROM dbo.""Parts"" WHERE ""UserId"" = {userContext.UserId} AND (
""PartNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR ""DigiKeyPartNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR ""MouserPartNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR ""ArrowPartNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR ""ManufacturerPartNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR ""Description"" ILIKE CONCAT('%', '{keywords}', '%')
OR ""Keywords"" ILIKE CONCAT('%', '{keywords}', '%')
OR ""Location"" ILIKE CONCAT('%', '{keywords}', '%')
OR ""BinNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR ""BinNumber2"" ILIKE CONCAT('%', '{keywords}', '%'))
),
""PartsMerged"" (""PartId"", ""Rank"") AS
(
	SELECT ""PartId"", ""Rank"" FROM ""PartsExactMatch""
	UNION
	SELECT ""PartId"", ""Rank"" FROM ""PartsBeginsWith""
	UNION
	SELECT ""PartId"", ""Rank"" FROM ""PartsAny""
)
SELECT pm.""Rank"", p.* FROM dbo.""Parts"" p
INNER JOIN (
  SELECT ""PartId"", MIN(""Rank"") ""Rank"" FROM ""PartsMerged"" GROUP BY ""PartId""
) pm ON pm.""PartId"" = p.""PartId"" ORDER BY pm.""Rank"" ASC;
;");
        }

        // todo: migrate
        public async Task<IBinnerDb> GetDatabaseAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var entities = await GetPartsAsync(userContext);
            return new LegacyBinnerDb
            {
                OAuthCredentials = await GetOAuthCredentialAsync(userContext),
                Parts = entities,
                PartTypes = await GetPartTypesAsync(userContext),
                Projects = await GetProjectsAsync(userContext),
                Count = entities.Count,
                FirstPartId = entities.OrderBy(x => x.PartId).First().PartId,
                LastPartId = entities.OrderBy(x => x.PartId).Last().PartId,
            };
        }

        public async Task<ConnectionResponse> TestConnectionAsync()
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.PartTypes.FirstOrDefaultAsync();
                if (entity != null)
                    return new ConnectionResponse { IsSuccess = true, DatabaseExists = true, Errors = new List<string>() };
            }
            catch (Exception ex)
            {
                return new ConnectionResponse { IsSuccess = false, Errors = new List<string>() { ex.GetBaseException().Message } };
            }

            return new ConnectionResponse { IsSuccess = false, Errors = new List<string>() { "No data returned! " } };
        }

        public async Task<OAuthAuthorization> CreateOAuthRequestAsync(OAuthAuthorization authRequest, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var entity = new DataModel.OAuthRequest
            {
                AuthorizationCode = authRequest.AuthorizationCode,
                AuthorizationReceived = authRequest.AuthorizationReceived,
                Error = authRequest.Error,
                ErrorDescription = authRequest.ErrorDescription,
                Provider = authRequest.Provider,
                RequestId = authRequest.Id,
                ReturnToUrl = authRequest.ReturnToUrl
            };
            await using var context = await _contextFactory.CreateDbContextAsync();
            EnforceIntegrityCreate(entity, userContext);
            context.OAuthRequests.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<OAuthAuthorization>(entity);
        }

        public async Task<OAuthAuthorization> UpdateOAuthRequestAsync(OAuthAuthorization authRequest, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OAuthRequests
                .FirstOrDefaultAsync(x => x.Provider == authRequest.Provider && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(authRequest, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                return _mapper.Map<OAuthAuthorization>(entity);
            }
            return null;
        }

        public async Task<OAuthAuthorization?> GetOAuthRequestAsync(Guid requestId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OAuthRequests
                .Where(x => x.RequestId == requestId && x.OrganizationId == userContext.OrganizationId)
                .FirstOrDefaultAsync();
            return _mapper.Map<OAuthAuthorization>(entity);
        }

        private async Task<ICollection<Part>> GetPartsAsync(IUserContext userContext)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.Parts
                .Where(x => x.OrganizationId == userContext.OrganizationId)
                .ToListAsync();
            return _mapper.Map<ICollection<Part>>(entities);
        }

        private async Task<ICollection<OAuthCredential>> GetOAuthCredentialAsync(IUserContext userContext)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.OAuthCredentials
                .Where(x => x.OrganizationId == userContext.OrganizationId)
                .ToListAsync();
            return _mapper.Map<ICollection<OAuthCredential>>(entities); ;
        }

        private async Task<ICollection<Project>> GetProjectsAsync(IUserContext userContext)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.Projects
                .Where(x => x.OrganizationId == userContext.OrganizationId)
                .ToListAsync();
            return _mapper.Map<ICollection<Project>>(entities);
        }

        public async Task<PaginatedResponse<Part>> GetLowStockAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var pageRecords = (request.Page - 1) * request.Results;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var totalItems = await context.Parts.CountAsync(x => x.Quantity <= x.LowStockThreshold && x.OrganizationId == userContext.OrganizationId);
            var entitiesQueryable = context.Parts
                .Where(x => x.Quantity <= x.LowStockThreshold && x.OrganizationId == userContext.OrganizationId);
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
            }
            var entities = await entitiesQueryable
                .Skip(pageRecords)
                .Take(request.Results)
                .ToListAsync();
            // map entities to parts
            return new PaginatedResponse<Part>(totalItems, request.Results, request.Page, _mapper.Map<ICollection<Part>>(entities));
        }

        public async Task<StoredFile> AddStoredFileAsync(StoredFile storedFile, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var entity = _mapper.Map<DataModel.StoredFile>(storedFile);
            await using var context = await _contextFactory.CreateDbContextAsync();
            EnforceIntegrityCreate(entity, userContext);
            context.StoredFiles.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<StoredFile>(entity);
        }

        public async Task<StoredFile?> GetStoredFileAsync(long storedFileId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.StoredFiles
                .FirstOrDefaultAsync(x => x.StoredFileId == storedFileId && x.OrganizationId == userContext.OrganizationId);
            return _mapper.Map<StoredFile?>(entity);
        }

        public async Task<StoredFile?> GetStoredFileAsync(string filename, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.StoredFiles
                .FirstOrDefaultAsync(x => x.FileName == filename && x.OrganizationId == userContext.OrganizationId);
            return _mapper.Map<StoredFile?>(entity);
        }

        public async Task<ICollection<StoredFile>> GetStoredFilesAsync(long partId, StoredFileType? fileType, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.StoredFiles
                .Where(x => x.PartId == partId && x.StoredFileType == fileType && x.OrganizationId == userContext.OrganizationId)
                .ToListAsync();
            return _mapper.Map<ICollection<StoredFile>>(entities);
        }

        public async Task<ICollection<StoredFile>> GetStoredFilesAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var pageRecords = (request.Page - 1) * request.Results;
            var entitiesQueryable = context.StoredFiles.Where(x => x.OrganizationId == userContext.OrganizationId);
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
            }

            var entities = await entitiesQueryable.Skip(pageRecords)
                .Take(request.Results)
                .ToListAsync();
            return _mapper.Map<ICollection<StoredFile>>(entities);
        }

        public async Task<StoredFile> UpdateStoredFileAsync(StoredFile storedFile, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.StoredFiles
                .FirstOrDefaultAsync(x => x.StoredFileId == storedFile.StoredFileId && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(storedFile, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                return _mapper.Map<StoredFile>(entity);
            }
            return null;
        }

        public async Task<bool> DeleteStoredFileAsync(StoredFile storedFile, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.StoredFiles.FirstOrDefault(x => x.StoredFileId == storedFile.StoredFileId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return false;
            context.StoredFiles.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Pcb?> GetPcbAsync(long pcbId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Pcbs
                .FirstOrDefaultAsync(x => x.PcbId == pcbId && x.OrganizationId == userContext.OrganizationId);
            return _mapper.Map<Pcb?>(entity);
        }

        public async Task<ICollection<Pcb>> GetPcbsAsync(long projectId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.ProjectPcbAssignments
                .Include(x => x.Pcb)
                .Where(x => x.ProjectId == projectId && x.OrganizationId == userContext.OrganizationId)
                .Select(x => x.Pcb)
                .ToListAsync();
            return _mapper.Map<ICollection<Pcb>>(entities);
        }

        public async Task<Pcb> AddPcbAsync(Pcb pcb, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.Pcb>(pcb);
            EnforceIntegrityCreate(entity, userContext);
            context.Pcbs.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<Pcb>(entity);
        }

        public async Task<Pcb> UpdatePcbAsync(Pcb pcb, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Pcbs
                .FirstOrDefaultAsync(x => x.PcbId == pcb.PcbId && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(pcb, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                return _mapper.Map<Pcb>(entity);
            }
            return null;
        }

        public async Task<bool> DeletePcbAsync(Pcb pcb, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.Pcbs
                .Include(x => x.ProjectPcbProduceHistory)
                .Include(x => x.ProjectPcbAssignments)
                .Include(x => x.PcbStoredFileAssignments)
                .FirstOrDefault(x => x.PcbId == pcb.PcbId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return false;

            context.PcbStoredFileAssignments.RemoveRange(entity.PcbStoredFileAssignments);
            context.ProjectPcbAssignments.RemoveRange(entity.ProjectPcbAssignments);
            context.ProjectPcbProduceHistory.RemoveRange(entity.ProjectPcbProduceHistory);
            context.Pcbs.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<PcbStoredFileAssignment?> GetPcbStoredFileAssignmentAsync(long pcbStoredFileAssignmentId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PcbStoredFileAssignments
                .FirstOrDefaultAsync(x => x.PcbStoredFileAssignmentId == pcbStoredFileAssignmentId && x.OrganizationId == userContext.OrganizationId);
            return _mapper.Map<PcbStoredFileAssignment?>(entity);
        }

        public async Task<ICollection<PcbStoredFileAssignment>> GetPcbStoredFileAssignmentsAsync(long pcbId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.PcbStoredFileAssignments
                .Where(x => x.PcbId == pcbId && x.OrganizationId == userContext.OrganizationId)
                .ToListAsync();
            return _mapper.Map<ICollection<PcbStoredFileAssignment>>(entities);
        }

        public async Task<PcbStoredFileAssignment> AddPcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.PcbStoredFileAssignment>(assignment);
            EnforceIntegrityCreate(entity, userContext);
            context.PcbStoredFileAssignments.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<PcbStoredFileAssignment>(entity);
        }

        public async Task<PcbStoredFileAssignment> UpdatePcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PcbStoredFileAssignments
                .FirstOrDefaultAsync(x => x.PcbStoredFileAssignmentId == assignment.PcbStoredFileAssignmentId && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(assignment, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                return _mapper.Map<PcbStoredFileAssignment>(entity);
            }
            return null;
        }

        public async Task<bool> RemovePcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.PcbStoredFileAssignments.FirstOrDefault(x => x.PcbStoredFileAssignmentId == assignment.PcbStoredFileAssignmentId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return false;
            context.PcbStoredFileAssignments.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<ICollection<ProjectPartAssignment>> GetPartAssignmentsAsync(long partId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.ProjectPartAssignments
                .Where(x => x.PartId == partId && x.OrganizationId == userContext.OrganizationId)
                .ToListAsync();
            return _mapper.Map<ICollection<ProjectPartAssignment>>(entities);
        }

        public async Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectPartAssignmentId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPartAssignments
                .FirstOrDefaultAsync(x => x.ProjectPartAssignmentId == projectPartAssignmentId && x.OrganizationId == userContext.OrganizationId);
            return _mapper.Map<ProjectPartAssignment?>(entity);
        }

        public async Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectId, long partId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPartAssignments
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.PartId == partId && x.OrganizationId == userContext.OrganizationId);
            return _mapper.Map<ProjectPartAssignment?>(entity);
        }

        public async Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectId, string partName, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPartAssignments
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.PartName == partName && x.OrganizationId == userContext.OrganizationId);
            return _mapper.Map<ProjectPartAssignment?>(entity);
        }

        public async Task<ICollection<ProjectPartAssignment>> GetProjectPartAssignmentsAsync(long projectId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.ProjectPartAssignments
                .Where(x => x.ProjectId == projectId && x.OrganizationId == userContext.OrganizationId)
                .ToListAsync();
            return _mapper.Map<ICollection<ProjectPartAssignment>>(entities);
        }

        public async Task<ICollection<ProjectPartAssignment>> GetProjectPartAssignmentsAsync(long projectId, PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var pageRecords = (request.Page - 1) * request.Results;
            var entitiesQueryable = context.ProjectPartAssignments
                .Where(x => x.ProjectId == projectId && x.OrganizationId == userContext.OrganizationId);
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
            }
            var entities = await entitiesQueryable
                .Skip(pageRecords)
                .Take(request.Results)
                .ToListAsync();
            return _mapper.Map<ICollection<ProjectPartAssignment>>(entities);
        }

        public async Task<ProjectPartAssignment> AddProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.ProjectPartAssignment>(assignment);
            EnforceIntegrityCreate(entity, userContext);
            context.ProjectPartAssignments.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<ProjectPartAssignment>(entity);
        }

        public async Task<ProjectPartAssignment> UpdateProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPartAssignments
                .FirstOrDefaultAsync(x => x.ProjectPartAssignmentId == assignment.ProjectPartAssignmentId && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(assignment, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                return _mapper.Map<ProjectPartAssignment>(entity);
            }
            return null;
        }

        public async Task<bool> RemoveProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.ProjectPartAssignments.FirstOrDefault(x => x.ProjectPartAssignmentId == assignment.ProjectPartAssignmentId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return false;
            context.ProjectPartAssignments.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectPcbAssignment?> GetProjectPcbAssignmentAsync(long projectPcbAssignmentId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPcbAssignments
                .FirstOrDefaultAsync(x => x.ProjectPcbAssignmentId == projectPcbAssignmentId && x.OrganizationId == userContext.OrganizationId);
            return _mapper.Map<ProjectPcbAssignment?>(entity);
        }

        public async Task<ICollection<ProjectPcbAssignment>> GetProjectPcbAssignmentsAsync(long projectId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.ProjectPcbAssignments
                .Where(x => x.ProjectId == projectId && x.OrganizationId == userContext.OrganizationId)
                .ToListAsync();
            return _mapper.Map<ICollection<ProjectPcbAssignment>>(entities);
        }

        public async Task<ProjectPcbAssignment> AddProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.ProjectPcbAssignment>(assignment);
            EnforceIntegrityCreate(entity, userContext);
            context.ProjectPcbAssignments.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<ProjectPcbAssignment>(entity);
        }

        public async Task<ProjectPcbAssignment> UpdateProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPcbAssignments
                .FirstOrDefaultAsync(x => x.ProjectPcbAssignmentId == assignment.ProjectPcbAssignmentId && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(assignment, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                return _mapper.Map<ProjectPcbAssignment>(entity);
            }
            return null;
        }

        public async Task<bool> RemoveProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.ProjectPcbAssignments.FirstOrDefault(x => x.ProjectPcbAssignmentId == assignment.ProjectPcbAssignmentId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return false;
            context.ProjectPcbAssignments.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<OAuthCredential?> GetOAuthCredentialAsync(string providerName, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OAuthCredentials
                .Where(x => x.Provider == providerName && x.OrganizationId == userContext.OrganizationId)
                .FirstOrDefaultAsync();
            if (entity == null)
                return null;
            return _mapper.Map<OAuthCredential?>(entity);
        }

        public async Task<PartType> GetOrCreatePartTypeAsync(PartType partType, IUserContext? userContext)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var existingEntity = await context.PartTypes
                .FirstOrDefaultAsync(x => x.Name != null && x.Name == partType.Name && x.OrganizationId == userContext.OrganizationId);
            if (existingEntity == null)
            {
                existingEntity = new DataModel.PartType
                {
                    DateCreatedUtc = DateTime.UtcNow,
                    //DateModifiedUtc = DateTime.UtcNow,
                    Name = partType.Name,
                    ParentPartTypeId = partType.ParentPartTypeId,
                    PartTypeId = partType.PartTypeId,
                    UserId = userContext.UserId,
                    OrganizationId = userContext.OrganizationId
                };
                context.PartTypes.Add(existingEntity);

                await context.SaveChangesAsync();
            }
            return _mapper.Map<PartType>(existingEntity);
        }

        public async Task<Part?> GetPartAsync(long partId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts.FirstOrDefaultAsync(x => x.PartId == partId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return null;
            return _mapper.Map<Part?>(entity);
        }

        public async Task<Part?> GetPartAsync(string partNumber, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts.FirstOrDefaultAsync(x => x.PartNumber == partNumber && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return null;
            return _mapper.Map<Part?>(entity);
        }

        public async Task<PaginatedResponse<Part>> GetPartsAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();

            DataModel.PartType? partType = null;
            // special case for partTypes
            if (request.Value != null && request?.By.Equals("partType", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                partType = await context.PartTypes.Where(x => x.Name == request.Value).FirstOrDefaultAsync();
                if (partType == null) throw new ArgumentException($"Unknown part type: {request.Value}");
                request.Value = partType.PartTypeId.ToString();
                request.By = "partTypeId";
            } else if (request.Value != null && request?.By.Equals("partTypeId", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                partType = await context.PartTypes.Where(x => x.PartTypeId == int.Parse(request.Value)).FirstOrDefaultAsync();
                if (partType == null) throw new ArgumentException($"Unknown part type: {request.Value}");
            }

            var pageRecords = (request.Page - 1) * request.Results;
            var entitiesQueryable = context.Parts
                .Where(x => x.OrganizationId == userContext.OrganizationId);
            var orderingApplied = false;
            if (!string.IsNullOrEmpty(request.By) && (request.By.Equals("PartType", StringComparison.InvariantCultureIgnoreCase) ||
                                                      request.By.Equals("PartTypeId", StringComparison.InvariantCultureIgnoreCase)) 
                                                  && partType != null)
            {
                // recursively get part types and their children
                var allPartTypes = await GetPartsByPartTypeAsync(context, partType, userContext);
                // query all the children using a custom predicate builder
                var predicate = PredicateBuilder.True<DataModel.Part>();
                predicate = predicate.AND(x => x.PartTypeId == partType.PartTypeId);
                foreach (var childPartType in allPartTypes)
                {
                    predicate = predicate.OR(x => x.PartTypeId == childPartType.PartTypeId);
                }
                // apply the predicate
                entitiesQueryable = entitiesQueryable.Where(predicate);
                // apply sorting
                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    orderingApplied = true;
                    entitiesQueryable = entitiesQueryable.OrderBy(p => p.PartTypeId)
                        .ThenByDescending(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
                }
            }
            else
            {
                // build a normal where
                entitiesQueryable = entitiesQueryable.WhereIf(!string.IsNullOrEmpty(request.By), x => EF.Property<string>(x, request.By!.UcFirst()) == request.Value);
            }

            // apply specified sorting
            if (!orderingApplied)
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
            }

            var totalParts = await entitiesQueryable.CountAsync();

            List<DataModel.Part> entities;
            try
            {
                entities = await entitiesQueryable
                    .Skip(pageRecords)
                    .Take(request.Results)
                    .ToListAsync();
                return new PaginatedResponse<Part>(totalParts, pageRecords, request.Page, _mapper.Map<ICollection<Part>>(entities));
            }
            catch (InvalidOperationException)
            {
                // return empty result set, unknown sort by column
                return new PaginatedResponse<Part>(totalParts, pageRecords, request.Page, new List<Part>());
            }
        }

        private async Task<ICollection<DataModel.PartType>> GetPartsByPartTypeAsync(BinnerContext context, DataModel.PartType partType, IUserContext? userContext)
        {
            var allPartTypes = new List<DataModel.PartType>();
            var childPartTypes = await context.PartTypes
                .Where(x => x.ParentPartTypeId == partType.PartTypeId)
                .ToListAsync();
            allPartTypes.AddRange(childPartTypes);
            foreach (var child in childPartTypes)
            {
                var children = await GetPartsByPartTypeAsync(context, child, userContext);
                if (children.Any())
                    allPartTypes.AddRange(children);
            }
            return allPartTypes;
        }

        public async Task<ICollection<Part>> GetPartsAsync(Expression<Func<Part, bool>> predicate, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var mappings = new List<Mapping<Part, DataModel.Part>>
            {
                new (x => x.PartId, x => x.PartId),
                new (x => x.BinNumber, x => x.BinNumber),
                new (x => x.BinNumber2, x => x.BinNumber2),
                new (x => x.Cost, x => x.Cost),
                new (x => x.DatasheetUrl, x => x.DatasheetUrl),
                new (x => x.DateCreatedUtc, x => x.DateCreatedUtc),
                new (x => x.Description, x => x.Description),
                new (x => x.DigiKeyPartNumber, x => x.DigiKeyPartNumber),
                new (x => x.ImageUrl, x => x.ImageUrl),
                new (x => x.Keywords, x => x.Keywords),
                new (x => x.Location, x => x.Location),
                new (x => x.LowestCostSupplier, x => x.LowestCostSupplier),
                new (x => x.LowestCostSupplierUrl, x => x.LowestCostSupplierUrl),
                new (x => x.LowStockThreshold, x => x.LowStockThreshold),
                new (x => x.Manufacturer, x => x.Manufacturer),
                new (x => x.ManufacturerPartNumber, x => x.ManufacturerPartNumber),
                new (x => x.MountingTypeId, x => x.MountingTypeId),
                new (x => x.MouserPartNumber, x => x.MouserPartNumber),
                new (x => x.ArrowPartNumber, x => x.ArrowPartNumber),
                new (x => x.PackageType, x => x.PackageType),
                new (x => x.PartId, x => x.PartId),
                new (x => x.PartNumber, x => x.PartNumber),
                new (x => x.PartTypeId, x => x.PartTypeId),
                new (x => x.ProductUrl, x => x.ProductUrl),
                new (x => x.ProjectId, x => x.ProjectId),
                new (x => x.Quantity, x => x.Quantity),
                // todo: migrate
                // new (x => x.SwarmPartNumberManufacturerId, x => x.SwarmPartNumberManufacturerId),
                new (x => x.UserId, x => x.UserId),
            };
            var mappedConverter = new MappedConverter<Part, DataModel.Part>(mappings.ToArray());
            var expressionConverter = new ExpressionConverter<Part, DataModel.Part>(mappedConverter);
            var newPredicate = expressionConverter.Visit(predicate);
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.Parts
                .Where(x => x.OrganizationId == userContext.OrganizationId)
                .Where(newPredicate as Expression<Func<DataModel.Part, bool>>)
                .ToListAsync();

            return _mapper.Map<ICollection<Part>>(entities);
        }

        public async Task<long> GetPartsCountAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Parts
                .Where(x => x.OrganizationId == userContext.OrganizationId)
                .SumAsync(x => x.Quantity);
        }

        public async Task<long> GetUniquePartsCountAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Parts
                .CountAsync(x => x.OrganizationId == userContext.OrganizationId);
        }

        public async Task<decimal> GetPartsValueAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            return (decimal)await context.Parts
                .Where(x => x.OrganizationId == userContext.OrganizationId)
                .SumAsync(x => x.Cost * x.Quantity);
        }

        public async Task<PartType?> GetPartTypeAsync(long partTypeId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PartTypes
                .FirstOrDefaultAsync(x => x.PartTypeId == partTypeId && (x.OrganizationId == userContext.OrganizationId || x.OrganizationId == null));
            if (entity == null)
                return null;
            return _mapper.Map<PartType?>(entity);

        }

        public async Task<PartType?> GetPartTypeAsync(string name, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PartTypes
                .FirstOrDefaultAsync(x => x.Name == name && (x.OrganizationId == userContext.OrganizationId || x.OrganizationId == null));
            if (entity == null)
                return null;
            return _mapper.Map<PartType?>(entity);

        }

        public async Task<ICollection<PartType>> GetPartTypesAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.PartTypes
                .Where(x => x.OrganizationId == userContext.OrganizationId || x.OrganizationId == null)
                .OrderBy(x => x.OrganizationId == null)
                .ThenBy(x => x.ParentPartType.Name)
                .ThenBy(x => x.Name)
                .ToListAsync();
            return _mapper.Map<ICollection<PartType>>(entities);
        }

        public async Task<Project?> GetProjectAsync(long projectId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Projects
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return null;
            return _mapper.Map<Project?>(entity);
        }

        public async Task<Project?> GetProjectAsync(string projectName, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Projects
                .FirstOrDefaultAsync(x => x.Name == projectName && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return null;
            return _mapper.Map<Project?>(entity);
        }

        public async Task<ICollection<Project>> GetProjectsAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var pageRecords = (request.Page - 1) * request.Results;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entitiesQueryable = context.Projects
                .Where(x => x.OrganizationId == userContext.OrganizationId)
                .WhereIf(!string.IsNullOrEmpty(request.By), x => x.GetPropertyValue(request.By.UcFirst()).ToString() == request.Value);
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
            }
            var entities = await entitiesQueryable
                .Skip(pageRecords)
                .Take(request.Results)
                .ToListAsync();
            return _mapper.Map<ICollection<Project>>(entities);
        }

        public async Task RemoveOAuthCredentialAsync(string providerName, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OAuthCredentials
                .FirstOrDefaultAsync(x => x.Provider == providerName && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
                context.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task<OAuthCredential?> SaveOAuthCredentialAsync(OAuthCredential credential, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            // insert or update
            var entity = await context.OAuthCredentials
                .FirstOrDefaultAsync(x => x.Provider == credential.Provider 
                    && (x.OrganizationId == userContext.OrganizationId || x.OrganizationId == null || x.OrganizationId == 0)
                    && (x.UserId == userContext.UserId || x.UserId == null || x.UserId == 0));
            if (entity != null)
            {
                // update
                entity.AccessToken = credential.AccessToken;
                entity.RefreshToken = credential.RefreshToken;
                entity.DateCreatedUtc = credential.DateCreatedUtc;
                entity.DateExpiresUtc = credential.DateExpiresUtc;
                EnforceIntegrityModify(entity, userContext);
            }
            else
            {
                // insert
                entity = _mapper.Map<DataModel.OAuthCredential>(credential);
                EnforceIntegrityCreate(entity, userContext);
                context.OAuthCredentials.Add(entity);
            }
            await context.SaveChangesAsync();
            return _mapper.Map<OAuthCredential?>(entity);
        }

        public async Task<Part> UpdatePartAsync(Part part, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts
                .FirstOrDefaultAsync(x => x.PartId == part.PartId && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(part, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                return _mapper.Map<Part>(entity);
            }
            return null;
        }

        public async Task<PartType?> UpdatePartTypeAsync(PartType partType, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PartTypes
                .FirstOrDefaultAsync(x => x.PartTypeId == partType.PartTypeId && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(partType, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                return _mapper.Map<PartType?>(entity);
            }
            return null;
        }

        public async Task<Project?> UpdateProjectAsync(Project project, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Projects
                .Include(x => x.ProjectPartAssignments)
                .Include(x => x.ProjectPcbAssignments)
                .Include(x => x.Parts)
                .FirstOrDefaultAsync(x => x.ProjectId == project.ProjectId && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(project, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                return _mapper.Map<Project?>(entity);
            }
            return null;
        }

        public async Task<PartSupplier> AddPartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.PartSupplier>(partSupplier);
            EnforceIntegrityCreate(entity, userContext);
            context.PartSuppliers.Add(entity);
            await context.SaveChangesAsync();
            partSupplier.PartSupplierId = entity.PartSupplierId;
            return partSupplier;
        }

        public async Task<PartSupplier?> GetPartSupplierAsync(long partSupplierId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PartSuppliers
                .Where(x => x.PartSupplierId == partSupplierId && x.OrganizationId == userContext.OrganizationId)
                .FirstOrDefaultAsync();
            return _mapper.Map<PartSupplier?>(entity);
        }

        public async Task<ICollection<PartSupplier>> GetPartSuppliersAsync(long partId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.PartSuppliers
                .Include(x => x.Part)
                .Where(x => x.PartId == partId && x.OrganizationId == userContext.OrganizationId)
                .ToListAsync();
            return _mapper.Map<ICollection<PartSupplier>>(entities);
        }

        public async Task<PartSupplier> UpdatePartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PartSuppliers
                .FirstOrDefaultAsync(x => x.PartSupplierId == partSupplier.PartSupplierId && x.OrganizationId == userContext.OrganizationId);
            if (entity != null)
            {
                entity = _mapper.Map(partSupplier, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                return _mapper.Map<PartSupplier>(entity);
            }
            return null;
        }

        public async Task<bool> DeletePartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.PartSuppliers.FirstOrDefault(x => x.PartSupplierId == partSupplier.PartSupplierId && x.OrganizationId == userContext.OrganizationId);
            if (entity == null)
                return false;
            context.PartSuppliers.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }



        public void EnforceIntegrityCreate<T>(T entity, IUserContext userContext)
            where T : DataModel.IEntity, DataModel.IUserData
        {
            entity.UserId = userContext.UserId;
            entity.OrganizationId = userContext.OrganizationId;
            entity.DateCreatedUtc = DateTime.UtcNow;
            entity.DateModifiedUtc = DateTime.UtcNow;
        }

        public void EnforceIntegrityModify<T>(T entity, IUserContext userContext)
            where T : DataModel.IEntity, DataModel.IUserData
        {
            entity.UserId = userContext.UserId;
            entity.OrganizationId = userContext.OrganizationId;
            entity.DateModifiedUtc = DateTime.UtcNow;
        }

        public void Dispose()
        {
            _config.Clear();
        }
    }

    // <summary>
/// Enables the efficient, dynamic composition of query predicates.
/// </summary>
public static class PredicateBuilder
{
    /// <summary>
    /// Creates a predicate that evaluates to true.
    /// </summary>
    public static Expression<Func<T, bool>> True<T>() { return param => true; }

    /// <summary>
    /// Creates a predicate that evaluates to false.
    /// </summary>
    public static Expression<Func<T, bool>> False<T>() { return param => false; }

    /// <summary>
    /// Creates a predicate expression from the specified lambda expression.
    /// </summary>
    public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate) { return predicate; }

    /// <summary>
    /// Combines the first predicate with the second using the logical "and".
    /// </summary>
    public static Expression<Func<T, bool>> AND<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.AndAlso);
    }

    /// <summary>
    /// Combines the first predicate with the second using the logical "or".
    /// </summary>
    public static Expression<Func<T, bool>> OR<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
    {
        return first.Compose(second, Expression.OrElse);
    }

    /// <summary>
    /// Negates the predicate.
    /// </summary>
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
    {
        var negated = Expression.Not(expression.Body);
        return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
    }

    /// <summary>
    /// Combines the first expression with the second using the specified merge function.
    /// </summary>
    static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
    {
        // zip parameters (map from parameters of second to parameters of first)
        var map = first.Parameters
            .Select((f, i) => new { f, s = second.Parameters[i] })
            .ToDictionary(p => p.s, p => p.f);

        // replace parameters in the second lambda expression with the parameters in the first
        var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

        // create a merged lambda expression with parameters from the first expression
        return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
    }

    class ParameterRebinder : ExpressionVisitor
    {
        readonly Dictionary<ParameterExpression, ParameterExpression> map;

        ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;

            if (map.TryGetValue(p, out replacement))
            {
                p = replacement;
            }

            return base.VisitParameter(p);
        }
    }
}
}