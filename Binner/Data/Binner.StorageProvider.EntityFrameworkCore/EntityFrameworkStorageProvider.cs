using AutoMapper;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TypeSupport.Extensions;
using DataModel = Binner.Data.Model;

namespace Binner.StorageProvider.EntityFrameworkCore
{
    public class EntityFrameworkStorageProvider : IStorageProvider
    {
        public const string ProviderName = "EntityFramework";
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly string _providerName;
        private readonly IDictionary<string, string> _config;
        private readonly IMapper _mapper;
        private readonly IPartTypesCache _partTypesCache;
        private readonly ILicensedStorageProvider _licensedStorageProvider;

        public EntityFrameworkStorageProvider(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, string providerName, IDictionary<string, string> config, IPartTypesCache partTypesCache, ILicensedStorageProvider licensedStorageProvider)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _providerName = providerName;
            _config = config;
            _partTypesCache = partTypesCache;
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
            _partTypesCache.InvalidateCache();
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
            await context.PartSuppliers.Where(x => x.PartId == part.PartId).ExecuteDeleteAsync();
            await context.ProjectPartAssignments.Where(x => x.PartId == part.PartId).ExecuteDeleteAsync();
            await context.StoredFiles.Where(x => x.PartId == part.PartId).ExecuteDeleteAsync();
            context.Parts.Remove(entity);
            await context.SaveChangesAsync();
            _partTypesCache.InvalidateCache();
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
            _partTypesCache.InvalidateCache();
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
SELECT p.PartId, 10 as Rank FROM Parts p LEFT JOIN PartSuppliers ps ON p.PartId=ps.PartId WHERE p.UserId = {userContext.UserId} AND (
 p.PartNumber = '{keywords}'
 OR p.DigiKeyPartNumber = '{keywords}'
 OR p.MouserPartNumber = '{keywords}'
 OR p.ManufacturerPartNumber = '{keywords}'
 OR p.Description = '{keywords}'
 OR p.Keywords = '{keywords}'
 OR p.Location = '{keywords}'
 OR p.BinNumber = '{keywords}'
 OR p.BinNumber2 = '{keywords}'
 OR p.SymbolName = '{keywords}'
 OR p.FootprintName = '{keywords}'
 OR p.ExtensionValue1 = '{keywords}'
 OR p.ExtensionValue2 = '{keywords}'
 OR ps.SupplierPartNumber = '{keywords}')
),
PartsBeginsWith (PartId, Rank) AS
(
SELECT p.PartId, 100 as Rank FROM Parts p LEFT JOIN PartSuppliers ps ON p.PartId=ps.PartId WHERE p.UserId = {userContext.UserId} AND (
 p.PartNumber LIKE '{keywords}'  + '%'
 OR p.DigiKeyPartNumber LIKE '{keywords}' + '%'
 OR p.MouserPartNumber LIKE '{keywords}' + '%'
 OR p.ManufacturerPartNumber LIKE '{keywords}' + '%'
 OR p.Description LIKE '{keywords}' + '%'
 OR p.Keywords LIKE '{keywords}' + '%'
 OR p.Location LIKE '{keywords}' + '%'
 OR p.BinNumber LIKE '{keywords}' + '%'
 OR p.BinNumber2 LIKE '{keywords}'+ '%'
 OR p.SymbolName LIKE '{keywords}' + '%'
 OR p.FootprintName LIKE '{keywords}' + '%'
 OR p.ExtensionValue1 LIKE '{keywords}' + '%'
 OR p.ExtensionValue2 LIKE '{keywords}' + '%'
 OR ps.SupplierPartNumber LIKE '{keywords}' + '%')
),
PartsAny (PartId, Rank) AS
(
SELECT p.PartId, 200 as Rank FROM Parts p LEFT JOIN PartSuppliers ps ON p.PartId=ps.PartId WHERE p.UserId = {userContext.UserId} AND (
 p.PartNumber LIKE '%' + '{keywords}' + '%'
 OR p.DigiKeyPartNumber LIKE '%' + '{keywords}' + '%'
 OR p.MouserPartNumber LIKE '%' + '{keywords}' + '%'
 OR p.ManufacturerPartNumber LIKE '%' + '{keywords}' + '%'
 OR p.Description LIKE '%' + '{keywords}' + '%'
 OR p.Keywords LIKE '%' + '{keywords}' + '%'
 OR p.Location LIKE '%' + '{keywords}' + '%'
 OR p.BinNumber LIKE '%' + '{keywords}' + '%'
 OR p.BinNumber2 LIKE '%' + '{keywords}' + '%'
 OR p.SymbolName LIKE '%' + '{keywords}' + '%'
 OR p.FootprintName LIKE '%' + '{keywords}' + '%'
 OR p.ExtensionValue1 LIKE '%' + '{keywords}' + '%'
 OR p.ExtensionValue2 LIKE '%' + '{keywords}' + '%'
 OR ps.SupplierPartNumber LIKE '%' + '{keywords}' + '%')
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
SELECT p.PartId, 10 as Rank FROM Parts p LEFT JOIN PartSuppliers ps ON p.PartId=ps.PartId WHERE p.UserId = {userContext.UserId} AND (
 p.PartNumber = '{keywords}'
 OR p.DigiKeyPartNumber = '{keywords}'
 OR p.MouserPartNumber = '{keywords}'
 OR p.ManufacturerPartNumber = '{keywords}'
 OR p.Description = '{keywords}'
 OR p.Keywords = '{keywords}'
 OR p.Location = '{keywords}'
 OR p.BinNumber = '{keywords}'
 OR p.BinNumber2 = '{keywords}'
 OR p.SymbolName = '{keywords}'
 OR p.FootprintName = '{keywords}'
 OR p.ExtensionValue1 = '{keywords}'
 OR p.ExtensionValue2 = '{keywords}'
 OR ps.SupplierPartNumber = '{keywords}')
),
PartsBeginsWith (PartId, Rank) AS
(
SELECT p.PartId, 100 as Rank FROM Parts p LEFT JOIN PartSuppliers ps ON p.PartId=ps.PartId WHERE p.UserId = {userContext.UserId} AND (
 p.PartNumber LIKE '{keywords}' || '%'
 OR p.DigiKeyPartNumber LIKE '{keywords}' || '%'
 OR p.MouserPartNumber LIKE '{keywords}' || '%'
 OR p.ManufacturerPartNumber LIKE '{keywords}' || '%'
 OR p.Description LIKE '{keywords}' || '%'
 OR p.Keywords LIKE '{keywords}' || '%'
 OR p.Location LIKE '{keywords}' || '%'
 OR p.BinNumber LIKE '{keywords}' || '%'
 OR p.BinNumber2 LIKE '{keywords}' || '%'
 OR p.SymbolName LIKE '{keywords}' || '%'
 OR p.FootprintName LIKE '{keywords}' || '%'
 OR p.ExtensionValue1 LIKE '{keywords}' || '%'
 OR p.ExtensionValue2 LIKE '{keywords}' || '%'
 OR ps.SupplierPartNumber LIKE '{keywords}' || '%')
),
PartsAny (PartId, Rank) AS
(
SELECT p.PartId, 200 as Rank FROM Parts p LEFT JOIN PartSuppliers ps ON p.PartId=ps.PartId WHERE p.UserId = {userContext.UserId} AND (
 p.PartNumber LIKE '%' + '{keywords}' || '%'
 OR p.DigiKeyPartNumber LIKE '%' + '{keywords}' || '%'
 OR p.MouserPartNumber LIKE '%' || '{keywords}' || '%'
 OR p.ManufacturerPartNumber LIKE '%' || '{keywords}' || '%'
 OR p.Description LIKE '%' || '{keywords}' || '%'
 OR p.Keywords LIKE '%' || '{keywords}' || '%'
 OR p.Location LIKE '%' || '{keywords}' || '%'
 OR p.BinNumber LIKE '%' || '{keywords}' || '%'
 OR p.BinNumber2 LIKE '%' || '{keywords}' || '%'
 OR p.SymbolName LIKE '%' || '{keywords}' || '%'
 OR p.FootprintName LIKE '%' || '{keywords}' || '%'
 OR p.ExtensionValue1 LIKE '%' || '{keywords}' || '%'
 OR p.ExtensionValue2 LIKE '%' || '{keywords}' || '%'
 OR ps.SupplierPartNumber LIKE '%' || '{keywords}' || '%')
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
SELECT p.PartId, 10 as OrderRank FROM Parts p LEFT JOIN PartSuppliers ps ON p.PartId=ps.PartId WHERE p.UserId = {userContext.UserId} AND (
 p.PartNumber = '{keywords}'
 OR p.DigiKeyPartNumber = '{keywords}'
 OR p.MouserPartNumber = '{keywords}'
 OR p.ManufacturerPartNumber = '{keywords}'
 OR p.Description = '{keywords}'
 OR p.Keywords = '{keywords}'
 OR p.Location = '{keywords}'
 OR p.BinNumber = '{keywords}'
 OR p.BinNumber2 = '{keywords}'
 OR p.SymbolName = '{keywords}'
 OR p.FootprintName = '{keywords}'
 OR p.ExtensionValue1 = '{keywords}'
 OR p.ExtensionValue2 = '{keywords}'
 OR ps.SupplierPartNumber = '{keywords}')
),
PartsBeginsWith (PartId, OrderRank) AS
(
SELECT p.PartId, 100 as OrderRank FROM Parts p LEFT JOIN PartSuppliers ps ON p.PartId=ps.PartId WHERE p.UserId = {userContext.UserId} AND (
 p.PartNumber LIKE CONCAT('{keywords}','%')
 OR p.DigiKeyPartNumber LIKE CONCAT('{keywords}','%')
 OR p.MouserPartNumber LIKE CONCAT('{keywords}','%')
 OR p.ManufacturerPartNumber LIKE CONCAT('{keywords}','%')
 OR p.Description LIKE CONCAT('{keywords}','%')
 OR p.Keywords LIKE CONCAT('{keywords}','%')
 OR p.Location LIKE CONCAT('{keywords}','%')
 OR p.BinNumber LIKE CONCAT('{keywords}','%')
 OR p.BinNumber2 LIKE CONCAT('{keywords}','%')
 OR p.SymbolName LIKE CONCAT('{keywords}','%')
 OR p.FootprintName LIKE CONCAT('{keywords}','%')
 OR p.ExtensionValue1 LIKE CONCAT('{keywords}','%')
 OR p.ExtensionValue2 LIKE CONCAT('{keywords}','%')
 OR ps.SupplierPartNumber LIKE CONCAT('{keywords}','%'))
),
PartsAny (PartId, OrderRank) AS
(
SELECT p.PartId, 200 as OrderRank FROM Parts p LEFT JOIN PartSuppliers ps ON p.PartId=ps.PartId WHERE p.UserId = {userContext.UserId} AND (
 p.PartNumber LIKE CONCAT('%','{keywords}','%')
 OR p.DigiKeyPartNumber LIKE CONCAT('%','{keywords}','%')
 OR p.MouserPartNumber LIKE CONCAT('%','{keywords}','%')
 OR p.ManufacturerPartNumber LIKE CONCAT('%','{keywords}','%')
 OR p.Description LIKE CONCAT('%','{keywords}','%')
 OR p.Keywords LIKE CONCAT('%','{keywords}','%')
 OR p.Location LIKE CONCAT('%','{keywords}','%')
 OR p.BinNumber LIKE CONCAT('%','{keywords}','%')
 OR p.BinNumber2 LIKE CONCAT('%','{keywords}','%')
 OR p.SymbolName LIKE CONCAT('%','{keywords}','%')
 OR p.FootprintName LIKE CONCAT('%','{keywords}','%')
 OR p.ExtensionValue1 LIKE CONCAT('%','{keywords}','%')
 OR p.ExtensionValue2 LIKE CONCAT('%','{keywords}','%')
 OR ps.SupplierPartNumber LIKE CONCAT('%','{keywords}','%'))
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
SELECT p.""PartId"", 10 as ""Rank"" FROM dbo.""Parts"" p LEFT JOIN dbo.""PartSuppliers"" ps ON p.""PartId""=ps.""PartId"" WHERE p.""UserId"" = {userContext.UserId} AND (
p.""PartNumber"" ILIKE '{keywords}' 
OR p.""DigiKeyPartNumber"" ILIKE '{keywords}' 
OR p.""MouserPartNumber"" ILIKE '{keywords}'
OR p.""ArrowPartNumber"" ILIKE '{keywords}'
OR p.""ManufacturerPartNumber"" ILIKE '{keywords}'
OR p.""Description"" ILIKE '{keywords}' 
OR p.""Keywords"" ILIKE '{keywords}' 
OR p.""Location"" ILIKE '{keywords}' 
OR p.""BinNumber"" ILIKE '{keywords}' 
OR p.""BinNumber2"" ILIKE '{keywords}'
OR p.""SymbolName"" ILIKE '{keywords}'
OR p.""FootprintName"" ILIKE '{keywords}'
OR p.""ExtensionValue1"" ILIKE '{keywords}'
OR p.""ExtensionValue2"" ILIKE '{keywords}'
OR ps.""SupplierPartNumber"" ILIKE '{keywords}')
),
""PartsBeginsWith"" (""PartId"", ""Rank"") AS
(
SELECT p.""PartId"", 100 as ""Rank"" FROM dbo.""Parts"" p LEFT JOIN dbo.""PartSuppliers"" ps ON p.""PartId""=ps.""PartId"" WHERE p.""UserId"" = {userContext.UserId} AND (
p.""PartNumber"" ILIKE CONCAT('{keywords}', '%')
OR p.""DigiKeyPartNumber"" ILIKE CONCAT('{keywords}', '%')
OR p.""MouserPartNumber"" ILIKE CONCAT('{keywords}', '%')
OR p.""ArrowPartNumber"" ILIKE CONCAT('{keywords}', '%')
OR p.""ManufacturerPartNumber"" ILIKE CONCAT('{keywords}', '%')
OR p.""Description"" ILIKE CONCAT('{keywords}', '%')
OR p.""Keywords"" ILIKE CONCAT('{keywords}', '%')
OR p.""Location"" ILIKE CONCAT('{keywords}', '%')
OR p.""BinNumber"" ILIKE CONCAT('{keywords}', '%')
OR p.""BinNumber2"" ILIKE CONCAT('{keywords}', '%')
OR p.""SymbolName"" ILIKE CONCAT('{keywords}', '%')
OR p.""FootprintName"" ILIKE CONCAT('{keywords}', '%')
OR p.""ExtensionValue1"" ILIKE CONCAT('{keywords}', '%')
OR p.""ExtensionValue2"" ILIKE CONCAT('{keywords}', '%')
OR ps.""SupplierPartNumber"" ILIKE CONCAT('{keywords}', '%'))
),
""PartsAny"" (""PartId"", ""Rank"") AS
(
SELECT p.""PartId"", 200 as ""Rank"" FROM dbo.""Parts"" p LEFT JOIN dbo.""PartSuppliers"" ps ON p.""PartId""=ps.""PartId"" WHERE p.""UserId"" = {userContext.UserId} AND (
""PartNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""DigiKeyPartNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""MouserPartNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""ArrowPartNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""ManufacturerPartNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""Description"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""Keywords"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""Location"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""BinNumber"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""BinNumber2"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""SymbolName"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""FootprintName"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""ExtensionValue1"" ILIKE CONCAT('%', '{keywords}', '%')
OR p.""ExtensionValue2"" ILIKE CONCAT('%', '{keywords}', '%')
OR ps.""SupplierPartNumber"" ILIKE CONCAT('%', '{keywords}', '%'))
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
            return _mapper.Map<ICollection<OAuthCredential>>(entities);
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
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                // ensure camel case names, EF properties are case sensitive
                request.OrderBy = request.OrderBy.UcFirst();
            }

            var entitiesQueryable = GetPartsQueryable(context, request, userContext, x => x.Quantity <= x.LowStockThreshold);

            var pageRecords = (request.Page - 1) * request.Results;
            var totalItems = await entitiesQueryable.CountAsync();

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
                .Where(x => x.PartId == partId && (fileType == null || x.StoredFileType == fileType) && x.OrganizationId == userContext.OrganizationId)
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
                .Where(x => x.Provider == providerName && x.UserId == userContext.UserId)
                .FirstOrDefaultAsync();
            if (entity == null)
                return null;
            return _mapper.Map<OAuthCredential?>(entity);
        }

        public async Task<PartType?> GetOrCreatePartTypeAsync(PartType partType, IUserContext? userContext)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var existingEntity = _partTypesCache.Cache
                .FirstOrDefault(x => x.Name != null && x.Name == partType.Name && x.OrganizationId == userContext.OrganizationId);
            if (existingEntity == null)
            {
                if (!string.IsNullOrEmpty(partType.Icon))
                {
                    partType.Icon = SvgSanitizer.Sanitize(partType.Icon);
                }

                var entity = new DataModel.PartType
                {
                    DateCreatedUtc = DateTime.UtcNow,
                    DateModifiedUtc = DateTime.UtcNow,
                    Name = partType.Name,
                    Icon = partType.Icon,
                    ParentPartTypeId = partType.ParentPartTypeId,
                    PartTypeId = partType.PartTypeId,
                    UserId = userContext.UserId,
                    OrganizationId = userContext.OrganizationId
                };
                context.PartTypes.Add(entity);
                await context.SaveChangesAsync();
                _partTypesCache.InvalidateCache();
                return _mapper.Map<PartType>(entity);
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

        private IQueryable<DataModel.Part> GetPartsQueryable(BinnerContext context, PaginatedRequest? request, IUserContext? userContext, Expression<Func<DataModel.Part, bool>>? additionalPredicate = null)
        {
            var stringCompare = StringComparison.InvariantCultureIgnoreCase;
            DataModel.PartType? partType = null;
            var filterBy = request?.By?.Split(',') ?? Array.Empty<string>();
            var filterByValues = request?.Value?.Split(',') ?? Array.Empty<string>();

            var entitiesQueryable = context.Parts
                .Include(x => x.PartType)
                .Include(x => x.PartSuppliers)
                .Where(x => x.OrganizationId == userContext.OrganizationId);
            var orderingApplied = false;
            
            if (filterBy.Any())
            {
                if (filterBy.Contains("partType", stringCompare) || filterBy.Contains("partTypeId", stringCompare))
                {
                    ICollection<CachedPartTypeResponse> matchingPartTypes = Array.Empty<CachedPartTypeResponse>();
                    // recursively get part types and their children
                    if (filterBy.Contains("partTypeId"))
                    {
                        // ensure part type exists by partTypeId
                        var index = filterBy.Select((value, index) => new { value, index })
                            .Where(x => x.value.Equals("partTypeId", stringCompare))
                            .Select(x => x.index)
                            .First();
                        var partTypeId = int.Parse(filterByValues[index]);
                        matchingPartTypes = GetPartTypesFromId(partTypeId, userContext);
                    }else if (filterBy.Contains("partType"))
                    {
                        // ensure part type exists by partTypeId
                        var index = filterBy.Select((value, index) => new { value, index })
                            .Where(x => x.value.Equals("partType", stringCompare))
                            .Select(x => x.index)
                            .First();
                        var partTypeName = filterByValues[index];
                        matchingPartTypes = GetPartTypesFromName(partTypeName, userContext);
                    }
                    // query all the children using a custom predicate builder
                    var predicate = PredicateBuilder.True<DataModel.Part>();
                    predicate = predicate.AND(x => x.PartTypeId == 0); // force the following predicates to be grouped together as an OR
                    foreach (var childPartType in matchingPartTypes)
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

                // do any non-part type filtering
                foreach (var by in filterBy.Where(x => !x.Equals("partType", stringCompare)))
                {
                    var index = filterBy.Select((value, index) => new { value, index })
                        .Where(x => x.value.Equals(by, stringCompare))
                        .Select(x => x.index)
                        .First();
                    if (index < filterByValues.Length)
                    {
                        var value = filterByValues[index];
                        entitiesQueryable = entitiesQueryable.WhereIf(!string.IsNullOrEmpty(by), x => EF.Property<string>(x, by!.UcFirst()) == value);
                    }
                }
            }

            // finally, do any keyword filtering
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                entitiesQueryable = entitiesQueryable.Where(x => 
                    EF.Functions.Like(x.PartNumber, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.ManufacturerPartNumber, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.Description, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.Manufacturer, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.Keywords, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.DigiKeyPartNumber, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.MouserPartNumber, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.ArrowPartNumber, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.TmePartNumber, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.Location, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.BinNumber, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.BinNumber2, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.PartType.Name, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.SymbolName, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.FootprintName, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.ExtensionValue1, '%' + request.Keyword + '%')
                    || EF.Functions.Like(x.ExtensionValue2, '%' + request.Keyword + '%')
                    || x.PartSuppliers.Any(y => EF.Functions.Like(y.SupplierPartNumber, '%' + request.Keyword + '%'))
                );
            }

            if (additionalPredicate != null)
                entitiesQueryable = entitiesQueryable.Where(additionalPredicate);

            // apply specified sorting
            if (!orderingApplied)
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy ?? "DateCreatedUtc"));
            }

            return entitiesQueryable;
        }

        public async Task<PaginatedResponse<Part>> GetPartsAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                // ensure camel case names, EF properties are case sensitive
                request.OrderBy = request.OrderBy.UcFirst();
            }

            var entitiesQueryable = GetPartsQueryable(context, request, userContext);

            var pageRecords = (request.Page - 1) * request.Results;
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

        private ICollection<CachedPartTypeResponse> GetPartTypesFromId(int partTypeId, IUserContext? userContext)
        {
            var allPartTypes = new List<CachedPartTypeResponse>();
            var matchingPartTypes = _partTypesCache.Cache
                .Where(x => x.PartTypeId == partTypeId && (x.OrganizationId == userContext.OrganizationId || (x.OrganizationId == null && x.UserId == null)))
                .ToList();
            allPartTypes.AddRange(matchingPartTypes);
            foreach (var partType in matchingPartTypes)
            {
                var children = GetPartTypesWithChildren(partType, userContext);
                allPartTypes.AddRange(children);
            }

            return allPartTypes;
        }

        private ICollection<CachedPartTypeResponse> GetPartTypesFromName(string name, IUserContext? userContext)
        {
            var allPartTypes = new List<CachedPartTypeResponse>();
            var matchingPartTypes = _partTypesCache.Cache
                .Where(x => x.Name == name && (x.OrganizationId == userContext.OrganizationId || (x.OrganizationId == null && x.UserId == null)))
                .ToList();
            allPartTypes.AddRange(matchingPartTypes);
            foreach (var partType in matchingPartTypes)
            {
                var children = GetPartTypesWithChildren(partType, userContext);
                allPartTypes.AddRange(children);
            }

            return allPartTypes;
        }

        private ICollection<CachedPartTypeResponse> GetPartTypesWithChildren(CachedPartTypeResponse partType, IUserContext? userContext)
        {
            var allPartTypes = new List<CachedPartTypeResponse>();
            var childPartTypes = _partTypesCache.Cache
                .Where(x => x.ParentPartTypeId == partType.PartTypeId && (x.OrganizationId == userContext.OrganizationId || (x.OrganizationId == null && x.UserId == null)))
                .ToList();
            allPartTypes.AddRange(childPartTypes);
            foreach (var child in childPartTypes)
            {
                var children = GetPartTypesWithChildren(child, userContext);
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
                new (x => x.TmePartNumber, x => x.TmePartNumber),
                new (x => x.PackageType, x => x.PackageType),
                new (x => x.PartId, x => x.PartId),
                new (x => x.PartNumber, x => x.PartNumber),
                new (x => x.PartTypeId, x => x.PartTypeId),
                new (x => x.ProductUrl, x => x.ProductUrl),
                new (x => x.ProjectId, x => x.ProjectId),
                new (x => x.Quantity, x => x.Quantity),
                new (x => x.SymbolName, x => x.SymbolName),
                new (x => x.FootprintName, x => x.FootprintName),
                new (x => x.ExtensionValue1, x => x.ExtensionValue1),
                new (x => x.ExtensionValue2, x => x.ExtensionValue2),
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
            var entity = _partTypesCache.Cache
                .FirstOrDefault(x => x.PartTypeId == partTypeId && (x.OrganizationId == userContext.OrganizationId || x.OrganizationId == null));
            if (entity == null)
                return null;
            return _mapper.Map<PartType?>(entity);
        }

        public Task<PartType?> GetPartTypeAsync(string name, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var entity = _partTypesCache.Cache
                .FirstOrDefault(x => x.Name == name && (x.OrganizationId == userContext.OrganizationId || x.OrganizationId == null));
            if (entity == null)
                return null;
            return Task.FromResult(_mapper.Map<PartType?>(entity));
        }

        public Task<ICollection<PartType>> GetPartTypesAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var entities = _partTypesCache.Cache
                .Where(x => x.OrganizationId == userContext.OrganizationId || x.OrganizationId == null)
                .OrderBy(x => x.OrganizationId == null)
                .ThenBy(x => x.ParentPartType?.Name)
                .ThenBy(x => x.Name)
                .ToList();
            return Task.FromResult(_mapper.Map<ICollection<PartType>>(entities));
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
                .FirstOrDefaultAsync(x => x.Provider == providerName && x.UserId == userContext.UserId);
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
                _partTypesCache.InvalidateCache();
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
                if (!string.IsNullOrEmpty(partType.Icon))
                {
                    partType.Icon = SvgSanitizer.Sanitize(partType.Icon);
                }
                entity = _mapper.Map(partType, entity);
                EnforceIntegrityModify(entity, userContext);
                await context.SaveChangesAsync();
                _partTypesCache.InvalidateCache();
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
}