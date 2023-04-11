﻿using AutoMapper;
using Binner.Data;
using Binner.Model.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TypeSupport.Extensions;
using static Azure.Core.HttpHeader;
using DataModel = Binner.Data.Model;

namespace Binner.StorageProvider.EntityFrameworkCore
{
    public class EntityFrameworkStorageProvider : IStorageProvider
    {
        public const string ProviderName = "EntityFramework";
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IDictionary<string, string> _config;
        private readonly IMapper _mapper;

        public EntityFrameworkStorageProvider(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, IDictionary<string, string> config)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _config = config;
        }

        public async Task<Part> AddPartAsync(Part part, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = Map(part, userContext);
            context.Parts.Add(entity);
            await context.SaveChangesAsync();
            part.PartId = entity.PartId;
            return part;
        }

        public async Task<Project?> AddProjectAsync(Project project, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = Map(project, userContext);
            context.Projects.Add(entity);
            await context.SaveChangesAsync();
            project.ProjectId = entity.ProjectId;
            return project;
        }

        public async Task<bool> DeletePartAsync(Part part, IUserContext userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = context.Parts.FirstOrDefault(x => x.PartId == part.PartId && x.UserId == userContext.UserId);
            if (entity == null)
                return false;
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
            // todo: this search is inefficient in EF but don't know how to convert it yet
            var query =
$@"WITH PartsExactMatch (PartId, Rank) AS
(
SELECT PartId, 10 as Rank FROM Parts WHERE UserId = @UserId AND (
PartNumber = @Keywords 
OR DigiKeyPartNumber = @Keywords 
OR MouserPartNumber = @Keywords
OR ManufacturerPartNumber = @Keywords
OR Description = @Keywords 
OR Keywords = @Keywords 
OR Location = @Keywords 
OR BinNumber = @Keywords 
OR BinNumber2 = @Keywords)
),
PartsBeginsWith (PartId, Rank) AS
(
SELECT PartId, 100 as Rank FROM Parts WHERE UserId = @UserId AND (
PartNumber LIKE @Keywords + '%'
OR DigiKeyPartNumber LIKE @Keywords + '%'
OR MouserPartNumber LIKE @Keywords + '%'
OR ManufacturerPartNumber LIKE @Keywords + '%'
OR Description LIKE @Keywords + '%'
OR Keywords LIKE @Keywords + '%'
OR Location LIKE @Keywords + '%'
OR BinNumber LIKE @Keywords + '%'
OR BinNumber2 LIKE @Keywords+ '%')
),
PartsAny (PartId, Rank) AS
(
SELECT PartId, 200 as Rank FROM Parts WHERE UserId = @UserId AND (
PartNumber LIKE '%' + @Keywords + '%'
OR DigiKeyPartNumber LIKE '%' + @Keywords + '%'
OR MouserPartNumber LIKE '%' + @Keywords + '%'
OR ManufacturerPartNumber LIKE '%' + @Keywords + '%'
OR Description LIKE '%' + @Keywords + '%'
OR Keywords LIKE '%' + @Keywords + '%'
OR Location LIKE '%' + @Keywords + '%'
OR BinNumber LIKE '%' + @Keywords + '%'
OR BinNumber2 LIKE '%' + @Keywords + '%')
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
;";
            using var context = await _contextFactory.CreateDbContextAsync();
            var userId = userContext.UserId;
            var result = await context.Parts.FromSqlRaw(query, new SqlParameter("Keywords", keywords), new SqlParameter("UserId", userId))
                .ToListAsync();
            return result.Select(x => new SearchResult<Part>(Map(x, userContext), 0)).ToList();
        }

        // todo: migrate
        public async Task<IBinnerDb> GetDatabaseAsync(IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var parts = await GetPartsAsync(userContext);
            return new Binner.Model.Common.BinnerDbV7
            {
                OAuthCredentials = await GetOAuthCredentialAsync(userContext),
                Parts = parts,
                PartTypes = await GetPartTypesAsync(userContext),
                Projects = await GetProjectsAsync(userContext),
                Count = parts.Count,
                FirstPartId = parts.OrderBy(x => x.PartId).First().PartId,
                LastPartId = parts.OrderBy(x => x.PartId).Last().PartId,
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

            return new ConnectionResponse { IsSuccess = false, Errors = new List<string>() { "No data returned! "} };
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
                ReturnToUrl = authRequest.ReturnToUrl,
                UserId = userContext.UserId,
                DateCreatedUtc = DateTime.UtcNow,
                DateModifiedUtc = DateTime.UtcNow
            };
            using var context = await _contextFactory.CreateDbContextAsync();
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
                entity.DateModifiedUtc = DateTime.UtcNow;
                entity.UserId = userContext.UserId;
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
            var result = await context.Parts
                .Where(x => x.UserId == userContext.UserId)
                .Select(x => Map(x, userContext))
                .ToListAsync();
            return result;
        }

        private async Task<ICollection<OAuthCredential>> GetOAuthCredentialAsync(IUserContext userContext)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var result = await context.OAuthCredentials
                .Where(x => x.UserId == userContext.UserId)
                .Select(x => Map(x, userContext))
                .ToListAsync();
            return result;
        }

        private async Task<ICollection<Project>> GetProjectsAsync(IUserContext userContext)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var result = await context.Projects
                .Where(x => x.UserId == userContext.UserId)
                .Select(x => Map(x, userContext))
                .ToListAsync();
            return result;
        }

        public async Task<PaginatedResponse<Part>> GetLowStockAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var pageRecords = (request.Page - 1) * request.Results;
            using var context = await _contextFactory.CreateDbContextAsync();
            var totalItems = await context.Parts.CountAsync(x => x.Quantity <= x.LowStockThreshold && x.UserId == userContext.UserId);
            var parts = await context.Parts
                .Where(x => x.Quantity <= x.LowStockThreshold && x.UserId == userContext.UserId)
                //.OrderBy(request.OrderBy, request.Direction) // todo: ordering
                .Skip(pageRecords)
                .Take(request.Results)
                .Select(x => Map(x, userContext))
                .ToListAsync();
            // map entities to parts
            return new PaginatedResponse<Part>(totalItems, request.Results, request.Page, parts);
        }

        public async Task<StoredFile> AddStoredFileAsync(StoredFile storedFile, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var entity = _mapper.Map<DataModel.StoredFile>(storedFile);
            entity.UserId = userContext.UserId;
            using var context = await _contextFactory.CreateDbContextAsync();
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
            var entities = await context.StoredFiles
                //.OrderBy(string.IsNullOrEmpty(request.OrderBy) ? "ProjectPartAssignmentId" : request.OrderBy, request.Direction) // todo: ordering
                .Skip(pageRecords)
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
                entity.DateModifiedUtc = DateTime.UtcNow;
                entity.UserId = userContext.UserId;
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
            var entity = _mapper.Map<DataModel.Pcb>(pcb);
            entity.UserId = userContext.UserId;
            using var context = await _contextFactory.CreateDbContextAsync();
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
                entity.DateModifiedUtc = DateTime.UtcNow;
                entity.UserId = userContext.UserId;
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
            var entity = _mapper.Map<DataModel.PcbStoredFileAssignment>(assignment);
            entity.UserId = userContext.UserId;
            using var context = await _contextFactory.CreateDbContextAsync();
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
                entity.DateModifiedUtc = DateTime.UtcNow;
                entity.UserId = userContext.UserId;
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
            var entities = await context.ProjectPartAssignments
                .Where(x => x.ProjectId == projectId && x.UserId == userContext.UserId)
                //.OrderBy(string.IsNullOrEmpty(request.OrderBy) ? "ProjectPartAssignmentId" : request.OrderBy, request.Direction) // todo: ordering
                .Skip(pageRecords)
                .Take(request.Results)
                .ToListAsync();
            return _mapper.Map<ICollection<ProjectPartAssignment>>(entities);
        }

        public async Task<ProjectPartAssignment> AddProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var entity = _mapper.Map<DataModel.ProjectPartAssignment>(assignment);
            entity.UserId = userContext.UserId;
            using var context = await _contextFactory.CreateDbContextAsync();
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
                entity.DateModifiedUtc = DateTime.UtcNow;
                entity.UserId = userContext.UserId;
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
            var entity = _mapper.Map<DataModel.ProjectPcbAssignment>(assignment);
            entity.UserId = userContext.UserId;
            using var context = await _contextFactory.CreateDbContextAsync();
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
                entity.DateModifiedUtc = DateTime.UtcNow;
                entity.UserId = userContext.UserId;
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
            return Map(entity, userContext);
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
                    DateModifiedUtc = DateTime.UtcNow,
                    Name = partType.Name,
                    ParentPartTypeId = partType.ParentPartTypeId,
                    PartTypeId = partType.PartTypeId,
                    UserId = userContext.UserId
                };
                context.PartTypes.Add(existingEntity);
                await context.SaveChangesAsync();
            }
            return Map(existingEntity, userContext);
        }

        public async Task<Part?> GetPartAsync(long partId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts.FirstOrDefaultAsync(x => x.PartId == partId && x.UserId == userContext.UserId);
            if (entity == null)
                return null;
            return Map(entity, userContext);
        }

        public async Task<Part?> GetPartAsync(string partNumber, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts.FirstOrDefaultAsync(x => x.PartNumber == partNumber && x.UserId == userContext.UserId);
            if (entity == null)
                return null;
            return Map(entity, userContext);
        }

        public async Task<PaginatedResponse<Part>> GetPartsAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var pageRecords = (request.Page - 1) * request.Results;
            using var context = await _contextFactory.CreateDbContextAsync();
            var parts = await context.Parts
                .Where(x => x.UserId == userContext.UserId)
                .WhereIf(!string.IsNullOrEmpty(request.By), x => x.GetPropertyValue(request.By.UcFirst()).ToString() == request.Value)
                // .OrderBy(request.OrderBy, request.Direction) // todo: ordering
                .Skip(pageRecords)
                .Take(request.Results)
                .Select(entity => Map(entity, userContext))
                .ToListAsync();
            return new PaginatedResponse<Part>(parts.Count, pageRecords, request.Page, parts);
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

            return entities
                .Select(x => Map(x, userContext))
                .ToList();
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
            return Map(entity, userContext);

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
            return entities.Select(x => Map(x, userContext)).ToList();
        }

        public async Task<Project?> GetProjectAsync(long projectId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Projects
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userContext.UserId);
            if (entity == null)
                return null;
            return Map(entity, userContext);
        }

        public async Task<Project?> GetProjectAsync(string projectName, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Projects
                .FirstOrDefaultAsync(x => x.Name == projectName && x.UserId == userContext.UserId);
            if (entity == null)
                return null;
            return Map(entity, userContext);
        }

        public async Task<ICollection<Project>> GetProjectsAsync(PaginatedRequest request, IUserContext? userContext)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            var pageRecords = (request.Page - 1) * request.Results;
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Projects
                .Where(x => x.UserId == userContext.UserId)
                .WhereIf(!string.IsNullOrEmpty(request.By), x => x.GetPropertyValue(request.By.UcFirst()).ToString() == request.Value)
                // .OrderBy(request.OrderBy, request.Direction) // todo: ordering
                .Skip(pageRecords)
                .Take(request.Results)
                .Select(entity => Map(entity, userContext))
                .ToListAsync();
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
                entity.DateModifiedUtc = DateTime.UtcNow;
            }
            else
            {
                // insert
                entity = Map(credential, userContext);
                context.OAuthCredentials.Add(entity);
            }
            await context.SaveChangesAsync();
            return Map(entity, userContext);
        }

        public async Task<Part> UpdatePartAsync(Part part, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Parts
                .FirstOrDefaultAsync(x => x.PartId == part.PartId && x.UserId == userContext.UserId);
            if (entity != null)
            {
                entity = Map(entity, part, userContext);
                entity.DateModifiedUtc = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return Map(entity, userContext);
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
                entity = Map(entity, partType, userContext);
                entity.DateModifiedUtc = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return Map(entity, userContext);
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
                entity = Map(entity, project, userContext);
                entity.DateModifiedUtc = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return Map(entity, userContext);
            }
            return null;
        }

        public async Task<PartSupplier> AddPartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = Map(partSupplier, userContext);
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
                .Select(x => Map(x, userContext))
                .FirstOrDefaultAsync();
            return entity;
        }

        public async Task<ICollection<PartSupplier>> GetPartSuppliersAsync(long partId, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entities = await context.PartSuppliers
                .Include(x => x.Part)
                .Where(x => x.PartId == partId && x.UserId == userContext.UserId)
                .Select(x => Map(x, userContext))
                .ToListAsync();
            return entities;
        }

        public async Task<PartSupplier> UpdatePartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext)
        {
            if (userContext == null) throw new ArgumentNullException(nameof(userContext));
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PartSuppliers
                .FirstOrDefaultAsync(x => x.PartSupplierId == partSupplier.PartSupplierId && x.UserId == userContext.UserId);
            if (entity != null)
            {
                entity = Map(entity, partSupplier, userContext);
                entity.DateModifiedUtc = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return Map(entity, userContext);
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

        private static PartSupplier Map(DataModel.PartSupplier entity, IUserContext userContext)
        {
            return new PartSupplier
            {
                Cost = entity.Cost,
                DateCreatedUtc = entity.DateCreatedUtc,
                DateModifiedUtc = entity.DateModifiedUtc,
                ImageUrl = entity.ImageUrl,
                MinimumOrderQuantity = entity.MinimumOrderQuantity,
                Name = entity.Name,
                // todo: migrate
                // Part = Map(entity.Part, userContext),
                PartId = entity.PartId,
                PartSupplierId = entity.PartSupplierId,
                ProductUrl = entity.ProductUrl,
                QuantityAvailable = entity.QuantityAvailable,
                SupplierPartNumber = entity.SupplierPartNumber
            };
        }

        private static DataModel.PartSupplier Map(PartSupplier model, IUserContext userContext)
        {
            return new DataModel.PartSupplier
            {
                Cost = model.Cost,
                DateCreatedUtc = model.DateCreatedUtc,
                DateModifiedUtc = DateTime.UtcNow,
                ImageUrl = model.ImageUrl,
                MinimumOrderQuantity = model.MinimumOrderQuantity,
                Name = model.Name,
                PartId = model.PartId,
                PartSupplierId = model.PartSupplierId,
                ProductUrl = model.ProductUrl,
                QuantityAvailable = model.QuantityAvailable,
                SupplierPartNumber = model.SupplierPartNumber,
                UserId = userContext.UserId
            };
        }

        private static DataModel.PartSupplier Map(DataModel.PartSupplier entity, PartSupplier model, IUserContext userContext)
        {
            entity.Cost = model.Cost;
            entity.DateCreatedUtc = model.DateCreatedUtc;
            entity.DateModifiedUtc = DateTime.UtcNow;
            entity.ImageUrl = model.ImageUrl;
            entity.MinimumOrderQuantity = model.MinimumOrderQuantity;
            entity.Name = model.Name;
            entity.PartId = model.PartId;
            entity.PartSupplierId = model.PartSupplierId;
            entity.ProductUrl = model.ProductUrl;
            entity.QuantityAvailable = model.QuantityAvailable;
            entity.SupplierPartNumber = model.SupplierPartNumber;
            entity.UserId = userContext.UserId;
            return entity;
        }

        private static Part Map(DataModel.Part? entity, IUserContext userContext)
            => entity != null ? new Part
            {
                BinNumber = entity.BinNumber,
                BinNumber2 = entity.BinNumber2,
                Cost = (decimal)entity.Cost,
                Currency = entity.Currency,
                DatasheetUrl = entity.DatasheetUrl,
                DateCreatedUtc = entity.DateCreatedUtc,
                Description = entity.Description,
                DigiKeyPartNumber = entity.DigiKeyPartNumber,
                ImageUrl = entity.ImageUrl,
                Keywords = entity?.Keywords.Split(",").ToList() ?? new List<string>(),
                Location = entity.Location,
                LowestCostSupplier = entity.LowestCostSupplier,
                LowestCostSupplierUrl = entity.LowestCostSupplierUrl,
                LowStockThreshold = entity.LowStockThreshold,
                Manufacturer = entity.Manufacturer,
                ManufacturerPartNumber = entity.ManufacturerPartNumber,
                MountingTypeId = entity.MountingTypeId,
                MouserPartNumber = entity.MouserPartNumber,
                ArrowPartNumber = entity.ArrowPartNumber,
                PackageType = entity.PackageType,
                PartId = entity.PartId,
                PartNumber = entity.PartNumber,
                PartTypeId = entity.PartTypeId,
                ProductUrl = entity.ProductUrl,
                ProjectId = entity.ProjectId,
                Quantity = entity.Quantity,
                // todo: migrate
                //SwarmPartNumberManufacturerId = entity.SwarmPartNumberManufacturerId,
                UserId = entity.UserId,
            } : new Part();

        private static DataModel.Part Map(Part model, IUserContext userContext)
            => new DataModel.Part
            {
                BinNumber = model.BinNumber,
                BinNumber2 = model.BinNumber2,
                Cost = (double)model.Cost,
                Currency = model.Currency,
                DatasheetUrl = model.DatasheetUrl,
                DateCreatedUtc = model.DateCreatedUtc,
                Description = model.Description,
                DigiKeyPartNumber = model.DigiKeyPartNumber,
                ImageUrl = model.ImageUrl,
                Keywords = model.Keywords != null ? string.Join(",", model.Keywords) : string.Empty,
                Location = model.Location,
                LowestCostSupplier = model.LowestCostSupplier,
                LowestCostSupplierUrl = model.LowestCostSupplierUrl,
                LowStockThreshold = model.LowStockThreshold,
                Manufacturer = model.Manufacturer,
                ManufacturerPartNumber = model.ManufacturerPartNumber,
                MountingTypeId = model.MountingTypeId,
                MouserPartNumber = model.MouserPartNumber,
                ArrowPartNumber = model.ArrowPartNumber,
                PackageType = model.PackageType,
                PartId = model.PartId,
                PartNumber = model.PartNumber,
                PartTypeId = model.PartTypeId,
                ProductUrl = model.ProductUrl,
                ProjectId = model.ProjectId,
                Quantity = model.Quantity,
                // todo: migrate
                //SwarmPartNumberManufacturerId = model.SwarmPartNumberManufacturerId,
                UserId = userContext.UserId,
            };

        private static DataModel.Part Map(DataModel.Part entity, Part model, IUserContext userContext)
        {
            entity.BinNumber = model.BinNumber;
            entity.BinNumber2 = model.BinNumber2;
            entity.Cost = (double)model.Cost;
            entity.Currency = model.Currency;
            entity.DatasheetUrl = model.DatasheetUrl;
            entity.DateCreatedUtc = model.DateCreatedUtc;
            entity.Description = model.Description;
            entity.DigiKeyPartNumber = model.DigiKeyPartNumber;
            entity.ImageUrl = model.ImageUrl;
            entity.Keywords = model.Keywords != null ? string.Join(",", model.Keywords) : string.Empty;
            entity.Location = model.Location;
            entity.LowestCostSupplier = model.LowestCostSupplier;
            entity.LowestCostSupplierUrl = model.LowestCostSupplierUrl;
            entity.LowStockThreshold = model.LowStockThreshold;
            entity.Manufacturer = model.Manufacturer;
            entity.ManufacturerPartNumber = model.ManufacturerPartNumber;
            entity.MountingTypeId = model.MountingTypeId;
            entity.MouserPartNumber = model.MouserPartNumber;
            entity.ArrowPartNumber = model.ArrowPartNumber;
            entity.PackageType = model.PackageType;
            entity.PartId = model.PartId;
            entity.PartNumber = model.PartNumber;
            entity.PartTypeId = model.PartTypeId;
            entity.ProductUrl = model.ProductUrl;
            entity.ProjectId = model.ProjectId;
            entity.Quantity = model.Quantity;
            // todo: migrate
            //entity.SwarmPartNumberManufacturerId = model.SwarmPartNumberManufacturerId;
            entity.UserId = userContext.UserId;
            return entity;
        }


        private static DataModel.Project Map(Project model, IUserContext userContext)
            => new DataModel.Project
            {
                Color = model.Color,
                DateCreatedUtc = model.DateCreatedUtc,
                DateModifiedUtc = DateTime.UtcNow,
                Description = model.Description,
                Location = model.Location,
                Notes = model.Notes,
                Name = model.Name,
                UserId = userContext.UserId,
            };

        private static Project Map(DataModel.Project entity, IUserContext userContext)
            => new Project
            {
                ProjectId = entity.ProjectId,
                Color = entity.Color,
                DateCreatedUtc = entity.DateCreatedUtc,
                Description = entity.Description,
                Location = entity.Location,
                Notes = entity.Notes,
                Name = entity.Name,
                UserId = entity.UserId
            };

        private static DataModel.Project Map(DataModel.Project entity, Project model, IUserContext userContext)
        {
            entity.DateCreatedUtc = model.DateCreatedUtc;
            entity.Name = model.Name;
            entity.UserId = userContext.UserId;
            entity.DateModifiedUtc = DateTime.UtcNow;
            entity.Color = model.Color;
            entity.Description = model.Description;
            entity.Location = model.Location;
            entity.Notes = model.Notes;
            return entity;
        }

        private static OAuthCredential Map(DataModel.OAuthCredential entity, IUserContext userContext)
            => new OAuthCredential
            {
                AccessToken = entity.AccessToken,
                DateCreatedUtc = entity.DateCreatedUtc,
                DateExpiresUtc = entity.DateExpiresUtc,
                Provider = entity.Provider,
                RefreshToken = entity.RefreshToken,
                UserId = entity.UserId,
            };

        private static DataModel.OAuthCredential Map(OAuthCredential model, IUserContext userContext)
            => new DataModel.OAuthCredential
            {
                AccessToken = model.AccessToken,
                DateCreatedUtc = model.DateCreatedUtc,
                DateExpiresUtc = model.DateExpiresUtc,
                DateModifiedUtc = DateTime.UtcNow,
                Provider = model.Provider,
                RefreshToken = model.RefreshToken,
                UserId = userContext.UserId,
            };

        private static DataModel.PartType Map(DataModel.PartType entity, PartType model, IUserContext userContext)
        {
            entity.DateCreatedUtc = model.DateCreatedUtc;
            entity.Name = model.Name;
            entity.ParentPartTypeId = model.ParentPartTypeId;
            entity.PartTypeId = model.PartTypeId;
            entity.UserId = userContext.UserId;
            entity.DateModifiedUtc = DateTime.UtcNow;
            return entity;
        }

        private static PartType Map(DataModel.PartType entity, IUserContext userContext) => new PartType
        {
            DateCreatedUtc = entity.DateCreatedUtc,
            Name = entity.Name,
            ParentPartTypeId = entity.ParentPartTypeId,
            PartTypeId = entity.PartTypeId,
            UserId = entity.UserId
        };

        private static DataModel.PartType Map(PartType model, IUserContext userContext)
            => new DataModel.PartType
            {
                DateCreatedUtc = model.DateCreatedUtc,
                Name = model.Name,
                ParentPartTypeId = model.ParentPartTypeId,
                PartTypeId = model.PartTypeId,
                DateModifiedUtc = DateTime.UtcNow,
                UserId = userContext.UserId
            };

        public void Dispose()
        {
            _config.Clear();
        }
    }
}