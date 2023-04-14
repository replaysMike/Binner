﻿using AutoMapper;
using Binner.Data;
using Binner.Model.Common;
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

        public EntityFrameworkStorageProvider(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, string providerName, IDictionary<string, string> config)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _providerName = providerName;
            _config = config;
        }

        public async Task<Part> AddPartAsync(Part part, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
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
            using var context = await _contextFactory.CreateDbContextAsync();
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts.FirstOrDefaultAsync(x => x.PartId == part.PartId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.PartTypes.FirstOrDefault(x => x.PartTypeId == partType.PartTypeId && x.UserId == userContext.UserId);
            if (entity == null)
                return false;
            context.PartTypes.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProjectAsync(Project project, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Projects.FirstOrDefaultAsync(x => x.ProjectId == project.ProjectId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
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
            return new Binner.Model.Common.BinnerDbV7
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
                using var context = await _contextFactory.CreateDbContextAsync();
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
            using var context = await _contextFactory.CreateDbContextAsync();
            EnforceIntegrityCreate(entity, userContext);
            context.OAuthRequests.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<OAuthAuthorization>(entity);
        }

        public async Task<OAuthAuthorization> UpdateOAuthRequestAsync(OAuthAuthorization authRequest, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OAuthRequests
                .FirstOrDefaultAsync(x => x.Provider == authRequest.Provider && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OAuthRequests
                .Where(x => x.RequestId == requestId && x.UserId == userContext.UserId)
                .FirstOrDefaultAsync();
            return _mapper.Map<OAuthAuthorization>(entity);
        }

        private async Task<ICollection<Part>> GetPartsAsync(IUserContext userContext)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.Parts
                .Where(x => x.UserId == userContext.UserId)
                .ToListAsync();
            return _mapper.Map<ICollection<Part>>(entities);
        }

        private async Task<ICollection<OAuthCredential>> GetOAuthCredentialAsync(IUserContext userContext)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.OAuthCredentials
                .Where(x => x.UserId == userContext.UserId)
                .ToListAsync();
            return _mapper.Map<ICollection<OAuthCredential>>(entities); ;
        }

        private async Task<ICollection<Project>> GetProjectsAsync(IUserContext userContext)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.Projects
                .Where(x => x.UserId == userContext.UserId)
                .ToListAsync();
            return _mapper.Map<ICollection<Project>>(entities);
        }

        public async Task<PaginatedResponse<Part>> GetLowStockAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var pageRecords = (request.Page - 1) * request.Results;
            using var context = await _contextFactory.CreateDbContextAsync();
            var totalItems = await context.Parts.CountAsync(x => x.Quantity <= x.LowStockThreshold && x.UserId == userContext.UserId);
            var entitiesQueryable = context.Parts
                .Where(x => x.Quantity <= x.LowStockThreshold && x.UserId == userContext.UserId);
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy));
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
            using var context = await _contextFactory.CreateDbContextAsync();
            EnforceIntegrityCreate(entity, userContext);
            context.StoredFiles.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<StoredFile>(entity);
        }

        public async Task<StoredFile?> GetStoredFileAsync(long storedFileId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.StoredFiles
                .FirstOrDefaultAsync(x => x.StoredFileId == storedFileId && x.UserId == userContext.UserId);
            return _mapper.Map<StoredFile?>(entity);
        }

        public async Task<StoredFile?> GetStoredFileAsync(string filename, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.StoredFiles
                .FirstOrDefaultAsync(x => x.FileName == filename && x.UserId == userContext.UserId);
            return _mapper.Map<StoredFile?>(entity);
        }

        public async Task<ICollection<StoredFile>> GetStoredFilesAsync(long partId, StoredFileType? fileType, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.StoredFiles
                .Where(x => x.PartId == partId && x.StoredFileType == fileType && x.UserId == userContext.UserId)
                .ToListAsync();
            return _mapper.Map<ICollection<StoredFile>>(entities);
        }

        public async Task<ICollection<StoredFile>> GetStoredFilesAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var pageRecords = (request.Page - 1) * request.Results;
            var entitiesQueryable = context.StoredFiles.Where(x => x.UserId == userContext.UserId);
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy));
            }

            var entities = await entitiesQueryable.Skip(pageRecords)
                .Take(request.Results)
                .ToListAsync();
            return _mapper.Map<ICollection<StoredFile>>(entities);
        }

        public async Task<StoredFile> UpdateStoredFileAsync(StoredFile storedFile, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.StoredFiles
                .FirstOrDefaultAsync(x => x.StoredFileId == storedFile.StoredFileId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.StoredFiles.FirstOrDefault(x => x.StoredFileId == storedFile.StoredFileId && x.UserId == userContext.UserId);
            if (entity == null)
                return false;
            context.StoredFiles.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Pcb?> GetPcbAsync(long pcbId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Pcbs
                .FirstOrDefaultAsync(x => x.PcbId == pcbId && x.UserId == userContext.UserId);
            return _mapper.Map<Pcb?>(entity);
        }

        public async Task<ICollection<Pcb>> GetPcbsAsync(long projectId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.ProjectPcbAssignments
                .Include(x => x.Pcb)
                .Where(x => x.ProjectId == projectId && x.UserId == userContext.UserId)
                .Select(x => x.Pcb)
                .ToListAsync();
            return _mapper.Map<ICollection<Pcb>>(entities);
        }

        public async Task<Pcb> AddPcbAsync(Pcb pcb, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.Pcb>(pcb);
            EnforceIntegrityCreate(entity, userContext);
            context.Pcbs.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<Pcb>(entity);
        }

        public async Task<Pcb> UpdatePcbAsync(Pcb pcb, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Pcbs
                .FirstOrDefaultAsync(x => x.PcbId == pcb.PcbId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.Pcbs.FirstOrDefault(x => x.PcbId == pcb.PcbId && x.UserId == userContext.UserId);
            if (entity == null)
                return false;
            context.Pcbs.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<PcbStoredFileAssignment?> GetPcbStoredFileAssignmentAsync(long pcbStoredFileAssignmentId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PcbStoredFileAssignments
                .FirstOrDefaultAsync(x => x.PcbStoredFileAssignmentId == pcbStoredFileAssignmentId && x.UserId == userContext.UserId);
            return _mapper.Map<PcbStoredFileAssignment?>(entity);
        }

        public async Task<ICollection<PcbStoredFileAssignment>> GetPcbStoredFileAssignmentsAsync(long pcbId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.PcbStoredFileAssignments
                .Where(x => x.PcbId == pcbId && x.UserId == userContext.UserId)
                .ToListAsync();
            return _mapper.Map<ICollection<PcbStoredFileAssignment>>(entities);
        }

        public async Task<PcbStoredFileAssignment> AddPcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.PcbStoredFileAssignment>(assignment);
            EnforceIntegrityCreate(entity, userContext);
            context.PcbStoredFileAssignments.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<PcbStoredFileAssignment>(entity);
        }

        public async Task<PcbStoredFileAssignment> UpdatePcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PcbStoredFileAssignments
                .FirstOrDefaultAsync(x => x.PcbStoredFileAssignmentId == assignment.PcbStoredFileAssignmentId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.PcbStoredFileAssignments.FirstOrDefault(x => x.PcbStoredFileAssignmentId == assignment.PcbStoredFileAssignmentId && x.UserId == userContext.UserId);
            if (entity == null)
                return false;
            context.PcbStoredFileAssignments.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<ICollection<ProjectPartAssignment>> GetPartAssignmentsAsync(long partId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.ProjectPartAssignments
                .Where(x => x.PartId == partId && x.UserId == userContext.UserId)
                .ToListAsync();
            return _mapper.Map<ICollection<ProjectPartAssignment>>(entities);
        }

        public async Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectPartAssignmentId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPartAssignments
                .FirstOrDefaultAsync(x => x.ProjectPartAssignmentId == projectPartAssignmentId && x.UserId == userContext.UserId);
            return _mapper.Map<ProjectPartAssignment?>(entity);
        }

        public async Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectId, long partId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPartAssignments
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.PartId == partId && x.UserId == userContext.UserId);
            return _mapper.Map<ProjectPartAssignment?>(entity);
        }

        public async Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectId, string partName, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPartAssignments
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.PartName == partName && x.UserId == userContext.UserId);
            return _mapper.Map<ProjectPartAssignment?>(entity);
        }

        public async Task<ICollection<ProjectPartAssignment>> GetProjectPartAssignmentsAsync(long projectId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.ProjectPartAssignments
                .Where(x => x.ProjectId == projectId && x.UserId == userContext.UserId)
                .ToListAsync();
            return _mapper.Map<ICollection<ProjectPartAssignment>>(entities);
        }

        public async Task<ICollection<ProjectPartAssignment>> GetProjectPartAssignmentsAsync(long projectId, PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var pageRecords = (request.Page - 1) * request.Results;
            var entitiesQueryable = context.ProjectPartAssignments
                .Where(x => x.ProjectId == projectId && x.UserId == userContext.UserId);
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy));
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.ProjectPartAssignment>(assignment);
            EnforceIntegrityCreate(entity, userContext);
            context.ProjectPartAssignments.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<ProjectPartAssignment>(entity);
        }

        public async Task<ProjectPartAssignment> UpdateProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPartAssignments
                .FirstOrDefaultAsync(x => x.ProjectPartAssignmentId == assignment.ProjectPartAssignmentId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.ProjectPartAssignments.FirstOrDefault(x => x.ProjectPartAssignmentId == assignment.ProjectPartAssignmentId && x.UserId == userContext.UserId);
            if (entity == null)
                return false;
            context.ProjectPartAssignments.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectPcbAssignment?> GetProjectPcbAssignmentAsync(long projectPcbAssignmentId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPcbAssignments
                .FirstOrDefaultAsync(x => x.ProjectPcbAssignmentId == projectPcbAssignmentId && x.UserId == userContext.UserId);
            return _mapper.Map<ProjectPcbAssignment?>(entity);
        }

        public async Task<ICollection<ProjectPcbAssignment>> GetProjectPcbAssignmentsAsync(long projectId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.ProjectPcbAssignments
                .Where(x => x.ProjectId == projectId && x.UserId == userContext.UserId)
                .ToListAsync();
            return _mapper.Map<ICollection<ProjectPcbAssignment>>(entities);
        }

        public async Task<ProjectPcbAssignment> AddProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = _mapper.Map<DataModel.ProjectPcbAssignment>(assignment);
            EnforceIntegrityCreate(entity, userContext);
            context.ProjectPcbAssignments.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<ProjectPcbAssignment>(entity);
        }

        public async Task<ProjectPcbAssignment> UpdateProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPcbAssignments
                .FirstOrDefaultAsync(x => x.ProjectPcbAssignmentId == assignment.ProjectPcbAssignmentId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.ProjectPcbAssignments.FirstOrDefault(x => x.ProjectPcbAssignmentId == assignment.ProjectPcbAssignmentId && x.UserId == userContext.UserId);
            if (entity == null)
                return false;
            context.ProjectPcbAssignments.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<OAuthCredential?> GetOAuthCredentialAsync(string providerName, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OAuthCredentials
                .Where(x => x.Provider == providerName && x.UserId == userContext.UserId)
                .FirstOrDefaultAsync();
            if (entity == null)
                return null;
            return _mapper.Map<OAuthCredential?>(entity);
        }

        public async Task<PartType> GetOrCreatePartTypeAsync(PartType partType, IUserContext? userContext)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var existingEntity = await context.PartTypes
                .FirstOrDefaultAsync(x => x.Name != null && x.Name == partType.Name && x.UserId == userContext.UserId);
            if (existingEntity == null)
            {
                existingEntity = new DataModel.PartType
                {
                    DateCreatedUtc = DateTime.UtcNow,
                    //DateModifiedUtc = DateTime.UtcNow,
                    Name = partType.Name,
                    ParentPartTypeId = partType.ParentPartTypeId,
                    PartTypeId = partType.PartTypeId,
                    UserId = userContext.UserId
                };
                context.PartTypes.Add(existingEntity);
                await context.SaveChangesAsync();
            }
            return _mapper.Map<PartType>(existingEntity);
        }

        public async Task<Part?> GetPartAsync(long partId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts.FirstOrDefaultAsync(x => x.PartId == partId && x.UserId == userContext.UserId);
            if (entity == null)
                return null;
            return _mapper.Map<Part?>(entity);
        }

        public async Task<Part?> GetPartAsync(string partNumber, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts.FirstOrDefaultAsync(x => x.PartNumber == partNumber && x.UserId == userContext.UserId);
            if (entity == null)
                return null;
            return _mapper.Map<Part?>(entity);
        }

        public async Task<PaginatedResponse<Part>> GetPartsAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var pageRecords = (request.Page - 1) * request.Results;
            using var context = await _contextFactory.CreateDbContextAsync();
            var totalParts = await context.Parts.CountAsync(x => x.UserId == userContext.UserId);
            var entitiesQueryable = context.Parts
                .Where(x => x.UserId == userContext.UserId)
                .WhereIf(!string.IsNullOrEmpty(request.By), x => x.GetPropertyValue(request.By.UcFirst()).ToString() == request.Value);
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy));
            }

            var entities = await entitiesQueryable
                .Skip(pageRecords)
                .Take(request.Results)
                .ToListAsync();
            return new PaginatedResponse<Part>(totalParts, pageRecords, request.Page, _mapper.Map<ICollection<Part>>(entities));
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.Parts
                .Where(x => x.UserId == userContext.UserId)
                .Where(newPredicate as Expression<Func<DataModel.Part, bool>>)
                .ToListAsync();

            return _mapper.Map<ICollection<Part>>(entities);
        }

        public async Task<long> GetPartsCountAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Parts
                .Where(x => x.UserId == userContext.UserId)
                .SumAsync(x => x.Quantity);
        }

        public async Task<long> GetUniquePartsCountAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Parts
                .CountAsync(x => x.UserId == userContext.UserId);
        }

        public async Task<decimal> GetPartsValueAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            return (decimal)await context.Parts
                .Where(x => x.UserId == userContext.UserId)
                .SumAsync(x => x.Cost * x.Quantity);
        }

        public async Task<PartType?> GetPartTypeAsync(long partTypeId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PartTypes
                .FirstOrDefaultAsync(x => x.PartTypeId == partTypeId && (x.UserId == userContext.UserId || x.UserId == null));
            if (entity == null)
                return null;
            return _mapper.Map<PartType?>(entity);

        }

        public async Task<ICollection<PartType>> GetPartTypesAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.PartTypes
                .Where(x => x.UserId == userContext.UserId || x.UserId == null)
                .OrderBy(x => x.UserId == null)
                .ThenBy(x => x.ParentPartType.Name)
                .ThenBy(x => x.Name)
                .ToListAsync();
            return _mapper.Map<ICollection<PartType>>(entities);
        }

        public async Task<Project?> GetProjectAsync(long projectId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Projects
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userContext.UserId);
            if (entity == null)
                return null;
            return _mapper.Map<Project?>(entity);
        }

        public async Task<Project?> GetProjectAsync(string projectName, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Projects
                .FirstOrDefaultAsync(x => x.Name == projectName && x.UserId == userContext.UserId);
            if (entity == null)
                return null;
            return _mapper.Map<Project?>(entity);
        }

        public async Task<ICollection<Project>> GetProjectsAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var pageRecords = (request.Page - 1) * request.Results;
            using var context = await _contextFactory.CreateDbContextAsync();
            var entitiesQueryable = context.Projects
                .Where(x => x.UserId == userContext.UserId)
                .WhereIf(!string.IsNullOrEmpty(request.By), x => x.GetPropertyValue(request.By.UcFirst()).ToString() == request.Value);
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                if (request.Direction == SortDirection.Descending)
                    entitiesQueryable = entitiesQueryable.OrderByDescending(p => EF.Property<object>(p, request.OrderBy));
                else
                    entitiesQueryable = entitiesQueryable.OrderBy(p => EF.Property<object>(p, request.OrderBy));
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OAuthCredentials
                .FirstOrDefaultAsync(x => x.Provider == providerName && x.UserId == userContext.UserId);
            if (entity != null)
                context.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task<OAuthCredential?> SaveOAuthCredentialAsync(OAuthCredential credential, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            // insert or update
            var entity = await context.OAuthCredentials
                .FirstOrDefaultAsync(x => x.Provider == credential.Provider && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts
                .FirstOrDefaultAsync(x => x.PartId == part.PartId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PartTypes
                .FirstOrDefaultAsync(x => x.PartTypeId == partType.PartTypeId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Projects
                .Include(x => x.ProjectPartAssignments)
                .Include(x => x.ProjectPcbAssignments)
                .Include(x => x.Parts)
                .FirstOrDefaultAsync(x => x.ProjectId == project.ProjectId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PartSuppliers
                .Where(x => x.PartSupplierId == partSupplierId && x.UserId == userContext.UserId)
                .FirstOrDefaultAsync();
            return _mapper.Map<PartSupplier?>(entity);
        }

        public async Task<ICollection<PartSupplier>> GetPartSuppliersAsync(long partId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.PartSuppliers
                .Include(x => x.Part)
                .Where(x => x.PartId == partId && x.UserId == userContext.UserId)
                .ToListAsync();
            return _mapper.Map<ICollection<PartSupplier>>(entities);
        }

        public async Task<PartSupplier> UpdatePartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PartSuppliers
                .FirstOrDefaultAsync(x => x.PartSupplierId == partSupplier.PartSupplierId && x.UserId == userContext.UserId);
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
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.PartSuppliers.FirstOrDefault(x => x.PartSupplierId == partSupplier.PartSupplierId && x.UserId == userContext.UserId);
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
            entity.DateCreatedUtc = DateTime.UtcNow;
            entity.DateModifiedUtc = DateTime.UtcNow;
        }

        public void EnforceIntegrityModify<T>(T entity, IUserContext userContext)
            where T : DataModel.IEntity, DataModel.IUserData
        {
            entity.UserId = userContext.UserId;
            entity.DateModifiedUtc = DateTime.UtcNow;
        }

        public void Dispose()
        {
            _config.Clear();
        }
    }
}