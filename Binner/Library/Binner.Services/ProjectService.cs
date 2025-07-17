using AutoMapper;
using Binner.Common.IO;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mapper = AnyMapper.Mapper;

namespace Binner.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ILogger<ProjectService> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IRequestContextAccessor _requestContext;

        public ProjectService(ILogger<ProjectService> logger, IMapper mapper, IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor, IDbContextFactory<BinnerContext> contextFactory)
        {
            _logger = logger;
            _mapper = mapper;
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
            _contextFactory = contextFactory;
        }

        public async Task<Project?> AddProjectAsync(Project project)
        {
            return await _storageProvider.AddProjectAsync(project, _requestContext.GetUserContext());
        }

        public async Task<Project?> ImportProjectAsync(ImportProjectRequest<IFormFile> request)
        {
            var stream = new MemoryStream();
            await request.File.CopyToAsync(stream);
            stream.Position = 0;

            var userContext = _requestContext.GetUserContext();
            var projectName = request.Name ?? "Empty";
            var project = await _storageProvider.GetProjectAsync(projectName, userContext);
            if (project == null)
            {
                project = new Project
                {
                    Name = projectName,
                    Description = request.Description
                };
                try
                {
                    project = await _storageProvider.AddProjectAsync(project, userContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to create project '{project?.Name}'");
                }

            }

            ImportResult? result = null;
            if (project != null)
            {
                try
                {
                    var extension = Path.GetExtension(request.File.FileName);
                    switch (extension.ToLower())
                    {
                        case ".csv":
                            var csvImporter = new CsvBOMImporter(_storageProvider);
                            result = await csvImporter.ImportAsync(project, stream, userContext);
                            break;
                        case ".xls":
                        case ".xlsx":
                        case ".xlsm":
                        case ".xlsb":
                            var excelImporter = new ExcelBOMImporter(_storageProvider);
                            result = await excelImporter.ImportAsync(project, stream, userContext);
                            break;
                    }
                    if (result?.Errors.Any() == true)
                    {
                        _logger.LogError($"Failed to import project '{project.Name}': {string.Join(", ", result.Errors)}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to import project '{project?.Name}'");
                }

            }

            return project;
        }

        public async Task<bool> DeleteProjectAsync(Project project)
        {
            var user = _requestContext.GetUserContext();
            // delete any pcb assignments
            var pcbAssignments = await _storageProvider.GetProjectPcbAssignmentsAsync(project.ProjectId, user);
            foreach (var pcbAssignment in pcbAssignments)
            {
                // delete any stored file assignments associated with pcb
                var storedFileAssignments = await _storageProvider.GetPcbStoredFileAssignmentsAsync(pcbAssignment.PcbId, user);
                foreach (var storedFileAssignment in storedFileAssignments)
                {
                    await _storageProvider.RemovePcbStoredFileAssignmentAsync(storedFileAssignment, user);
                }

                await _storageProvider.RemoveProjectPcbAssignmentAsync(pcbAssignment, user);
            }
            // remove any part assignments associated with this project
            var projectPartAssignments = await _storageProvider.GetProjectPartAssignmentsAsync(project.ProjectId, user);
            foreach (var partAssignment in projectPartAssignments)
            {
                await _storageProvider.RemoveProjectPartAssignmentAsync(partAssignment, user);
            }

            var success = await _storageProvider.DeleteProjectAsync(project, user);
            return success;
        }

        public async Task<Project?> GetProjectAsync(long projectId)
        {
            return await _storageProvider.GetProjectAsync(projectId, _requestContext.GetUserContext());
        }

        public async Task<Project?> GetProjectAsync(string name)
        {
            return await _storageProvider.GetProjectAsync(name, _requestContext.GetUserContext());
        }

        public async Task<ICollection<Project>> GetProjectsAsync(PaginatedRequest request)
        {
            return await _storageProvider.GetProjectsAsync(request, _requestContext.GetUserContext());
        }

        public async Task<Project?> UpdateProjectAsync(Project project)
        {
            project.DateModifiedUtc = DateTime.UtcNow;
            return await _storageProvider.UpdateProjectAsync(project, _requestContext.GetUserContext());
        }

        public async Task<ICollection<ProjectPart>> GetPartsAsync(long projectId)
        {
            var user = _requestContext.GetUserContext();
            var parts = new List<ProjectPart>();
            var assignments = await _storageProvider.GetProjectPartAssignmentsAsync(projectId, user);
            foreach (var assignment in assignments)
            {
                var projectPart = Mapper.Map<ProjectPart>(assignment);
                if (assignment.PartId != null)
                {
                    var part = await _storageProvider.GetPartAsync(assignment.PartId.Value, user);
                    if (part != null)
                    {
                        projectPart.Part = Mapper.Map<PartResponse>(part);
                        projectPart.ShortId = part.ShortId;
                        parts.Add(projectPart);
                    }
                }
                else if (!string.IsNullOrEmpty(assignment.PartName))
                {
                    parts.Add(projectPart);
                }
            }

            return parts;
        }

        public async Task<ICollection<ProjectPcb>> GetPcbsAsync(long projectId)
        {
            var user = _requestContext.GetUserContext();
            var projectPcbs = new List<ProjectPcb>();
            var pcbs = await _storageProvider.GetPcbsAsync(projectId, user);
            var projectParts = await _storageProvider.GetProjectPartAssignmentsAsync(projectId, user);
            foreach (var pcb in pcbs.OrderBy(x => x.DateCreatedUtc))
            {
                var storedFile = await _storageProvider.GetStoredFilesAsync(pcb.PcbId, StoredFileType.Pcb, user);
                var projectPcb = Mapper.Map<ProjectPcb>(pcb);
                if (storedFile.Any())
                    projectPcb.StoredFile = Mapper.Map<StoredFile>(storedFile.First());
                // get parts count for pcb
                projectPcb.PartsCount = projectParts.Count(x => x.PcbId == pcb.PcbId);
                projectPcbs.Add(projectPcb);
            }
            return projectPcbs;
        }

        /// <summary>
        /// Add a part to the project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ProjectPart?> AddPartAsync(AddBomPartRequest request)
        {
            Project? project = null;
            var user = _requestContext.GetUserContext();

            if (request.ProjectId != null)
                project = await _storageProvider.GetProjectAsync(request.ProjectId.Value, user);
            else if (!string.IsNullOrEmpty(request.Project))
                project = await _storageProvider.GetProjectAsync(request.Project, user);

            if (project == null)
                return null;

            var part = await _storageProvider.GetPartAsync(request.PartNumber, user);
            var assignment = new ProjectPartAssignment
            {
                PartId = part?.PartId,
                ProjectId = project.ProjectId,
                Notes = request.Notes,
                PartName = request.PartNumber,
                PcbId = request.PcbId,
                Cost = request.Cost,
                Currency = request.Currency,
                Quantity = request.Quantity,
                QuantityAvailable = part == null ? request.QuantityAvailable : 0,
                ReferenceId = request.ReferenceId,
                CustomDescription = request.CustomDescription,
                SchematicReferenceId = request.SchematicReferenceId,
            };
            await _storageProvider.AddProjectPartAssignmentAsync(assignment, user);
            // update project (DateModified)
            project.DateModifiedUtc = DateTime.UtcNow;
            await _storageProvider.UpdateProjectAsync(project, user);

            var projectPart = Mapper.Map<ProjectPart>(assignment);
            projectPart.Part = Mapper.Map<PartResponse>(part);
            return projectPart;
        }

        /// <summary>
        /// Update part details in project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ProjectPart?> UpdatePartAsync(UpdateBomPartRequest request)
        {
            var user = _requestContext.GetUserContext();
            var project = await _storageProvider.GetProjectAsync(request.ProjectId, user);
            if (project == null)
                return null;

            Part? part = null;
            if (request.PartId != null)
                part = await _storageProvider.GetPartAsync(request.PartId.Value, user);
            else if (!string.IsNullOrEmpty(request.PartName))
                part = await _storageProvider.GetPartAsync(request.PartName, user);

            var assignment = await _storageProvider.GetProjectPartAssignmentAsync(request.ProjectPartAssignmentId, user);
            if (assignment != null)
            {
                assignment.PartName = request.PartName;
                assignment.PartId = part?.PartId;
                assignment.Notes = request.Notes;
                assignment.ReferenceId = request.ReferenceId;
                assignment.Cost = request.Cost;
                assignment.Currency = request.Currency;
                assignment.Quantity = request.Quantity;
                assignment.CustomDescription = request.CustomDescription;
                assignment.SchematicReferenceId = request.SchematicReferenceId;

                if (part == null)
                {
                    assignment.QuantityAvailable = request.QuantityAvailable;
                    assignment.Cost = request.Cost;
                    assignment.Currency = request.Currency;
                }
                else
                {
                    assignment.QuantityAvailable = 0;
                    assignment.Cost = 0;
                    assignment.Currency = null;
                }

                await _storageProvider.UpdateProjectPartAssignmentAsync(assignment, user);

                // also update the part quantity and cost if it has changed
                if (request.Part != null && part != null)
                {
                    if (request.Part.Cost != part.Cost)
                        part.Cost = request.Part.Cost;
                    if (request.Part.Quantity >= 0 && request.Part.Quantity != part.Quantity)
                        part.Quantity = request.Part.Quantity;
                    await _storageProvider.UpdatePartAsync(part, user);
                }

                // update project (DateModified)
                project.DateModifiedUtc = DateTime.UtcNow;
                await _storageProvider.UpdateProjectAsync(project, user);

                var projectPart = Mapper.Map<ProjectPart>(assignment);
                projectPart.Part = Mapper.Map<PartResponse>(part);
                return projectPart;
            }

            return null;
        }

        /// <summary>
        /// Remove part from project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> RemovePartAsync(RemoveBomPartRequest request)
        {
            Project? project = null;
            var user = _requestContext.GetUserContext();

            if (!request.Ids.Any())
                return false;
            if (request.ProjectId != null)
                project = await _storageProvider.GetProjectAsync(request.ProjectId.Value, user);
            else if (!string.IsNullOrEmpty(request.Project))
                project = await _storageProvider.GetProjectAsync(request.Project, user);
            if (project == null) return false;

            var success = false;
            foreach (var projectPartAssignmentId in request.Ids)
            {
                var assignment = await _storageProvider.GetProjectPartAssignmentAsync(projectPartAssignmentId, user);
                if (assignment == null) continue;
                if (await _storageProvider.RemoveProjectPartAssignmentAsync(assignment, user))
                    success = true;
            }

            // update project (DateModified)
            project.DateModifiedUtc = DateTime.UtcNow;
            await _storageProvider.UpdateProjectAsync(project, user);

            return success;
        }

        /// <summary>
        /// Move parts to a different PCB (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> MovePartAsync(MoveBomPartRequest request)
        {
            Project? project = null;
            var user = _requestContext.GetUserContext();

            if (!request.Ids.Any())
                return false;
            if (request.ProjectId != null)
                project = await _storageProvider.GetProjectAsync(request.ProjectId.Value, user);
            else if (!string.IsNullOrEmpty(request.Project))
                project = await _storageProvider.GetProjectAsync(request.Project, user);
            if (project == null) return false;

            var success = false;
            foreach (var projectPartAssignmentId in request.Ids)
            {
                var assignment = await _storageProvider.GetProjectPartAssignmentAsync(projectPartAssignmentId, user);
                if (assignment == null) continue;

                if (request.PcbId > 0)
                    assignment.PcbId = request.PcbId;   // move to Pcb
                else
                    assignment.PcbId = null; // move to Unassociated

                assignment = await _storageProvider.UpdateProjectPartAssignmentAsync(assignment, user);
                success = true;
            }

            // update project (DateModified)
            project.DateModifiedUtc = DateTime.UtcNow;
            await _storageProvider.UpdateProjectAsync(project, user);

            return success;
        }

        public async Task<ICollection<ProjectProduceHistory>> GetProduceHistoryAsync(GetProduceHistoryRequest request)
        {
            var user = _requestContext.GetUserContext();
            if (user == null) throw new ArgumentNullException(nameof(user));

            await using var context = await _contextFactory.CreateDbContextAsync();
            var pageRecords = (request.Page - 1) * request.Results;
            var entities = await context.ProjectProduceHistory
                .Include(x => x.ProjectPcbProduceHistory)
                .ThenInclude(x => x.Pcb)
                .Where(x => x.ProjectId == request.ProjectId && x.OrganizationId == user.OrganizationId)
                .OrderByDescending(x => x.DateCreatedUtc)
                .Skip(pageRecords)
                .Take(request.Results)
                .ToListAsync();

            var models = _mapper.Map<ICollection<ProjectProduceHistory>>(entities);
            foreach (var history in models)
            {
                foreach (var pcb in history.Pcbs)
                {
                    var storedFiles = await _storageProvider.GetStoredFilesAsync(pcb.Pcb.PcbId, StoredFileType.Pcb, user);
                    if (storedFiles.Any())
                        pcb.Pcb.StoredFile = Mapper.Map<StoredFile>(storedFiles.First());
                }
            }

            return models;
        }

        public async Task<ProjectProduceHistory> UpdateProduceHistoryAsync(ProjectProduceHistory request)
        {
            var user = _requestContext.GetUserContext();
            if (user == null) throw new ArgumentNullException(nameof(user));

            await using var context = await _contextFactory.CreateDbContextAsync();

            var entity = await context.ProjectProduceHistory
                .Include(x => x.ProjectPcbProduceHistory)
                .ThenInclude(x => x.Pcb)
                .Where(x => x.ProjectProduceHistoryId == request.ProjectProduceHistoryId && x.OrganizationId == user.OrganizationId)
                .FirstOrDefaultAsync();

            entity = _mapper.Map(request, entity);
            foreach (var pcbEntity in entity.ProjectPcbProduceHistory)
            {
                var pcb = request.Pcbs.FirstOrDefault(x => x.ProjectPcbProduceHistoryId == pcbEntity.ProjectPcbProduceHistoryId);
                if (pcb != null)
                {
                    var updatedPcbEntity = _mapper.Map(pcb, pcbEntity);
                    pcbEntity.PartsConsumed = updatedPcbEntity.PartsConsumed;
                    pcbEntity.PcbQuantity = updatedPcbEntity.PcbQuantity;
                    pcbEntity.PcbCost = updatedPcbEntity.PcbCost;
                    pcbEntity.SerialNumber = updatedPcbEntity.SerialNumber;
                    pcbEntity.DateModifiedUtc = DateTime.UtcNow;
                }
            }
            await context.SaveChangesAsync();

            return _mapper.Map<ProjectProduceHistory>(entity);
        }

        public async Task<bool> DeleteProduceHistoryAsync(ProjectProduceHistory request)
        {
            var user = _requestContext.GetUserContext();
            if (user == null) throw new ArgumentNullException(nameof(user));

            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectProduceHistory
                .Include(x => x.Project)
                    .ThenInclude(x => x.ProjectPartAssignments)
                    .ThenInclude(x => x.Part)
                .Include(x => x.ProjectPcbProduceHistory)
                    .ThenInclude(x => x.Pcb)
                .Where(x => x.ProjectProduceHistoryId == request.ProjectProduceHistoryId && x.OrganizationId == user.OrganizationId)
                .FirstOrDefaultAsync();
            if (entity == null) return false;

            // place the parts back in stock
            await ReturnInventoryStockAsync(context, entity);

            // delete all child records
            context.ProjectPcbProduceHistory.RemoveRange(entity.ProjectPcbProduceHistory);

            context.ProjectProduceHistory.Remove(entity);

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePcbProduceHistoryAsync(ProjectPcbProduceHistory request)
        {
            var user = _requestContext.GetUserContext();
            if (user == null) throw new ArgumentNullException(nameof(user));

            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.ProjectPcbProduceHistory
                .Include(x => x.Pcb)
                .Include(x => x.ProjectProduceHistory)
                    .ThenInclude(x => x.Project)
                    .ThenInclude(x => x.ProjectPartAssignments)
                    .ThenInclude(x => x.Part)
                .Where(x => x.ProjectPcbProduceHistoryId == request.ProjectPcbProduceHistoryId && x.OrganizationId == user.OrganizationId)
                .FirstOrDefaultAsync();
            if (entity == null) return false;

            // place the parts back in stock
            await ReturnPcbInventoryStockAsync(context, entity);

            context.ProjectPcbProduceHistory.Remove(entity);

            return await context.SaveChangesAsync() > 0;
        }

        private async Task ReturnInventoryStockAsync(BinnerContext context, Data.Model.ProjectProduceHistory history)
        {
            if (history.ProduceUnassociated)
            {
                var produceQuantity = history.Quantity;
                // place the unassociated parts back in stock
                foreach (var part in history.Project!.ProjectPartAssignments!.Where(x => x.PcbId == null))
                {
                    var quantityRequired = part.Quantity * produceQuantity;
                    if (part.Part != null)
                    {
                        part.Part.Quantity += quantityRequired;
                    }
                    else
                    {
                        part.QuantityAvailable += quantityRequired;
                    }
                }
            }

            // place the pcb parts back in stock
            foreach (var pcbHistory in history.ProjectPcbProduceHistory)
            {
                await ReturnPcbInventoryStockAsync(context, pcbHistory);
            }
        }

        private async Task ReturnPcbInventoryStockAsync(BinnerContext context, Data.Model.ProjectPcbProduceHistory pcbHistory)
        {
            // the pcb quantity multiplier
            var produceQuantity = pcbHistory.ProjectProduceHistory!.Quantity;
            var pcbQuantity = pcbHistory.PcbQuantity;
            var project = pcbHistory.ProjectProduceHistory!.Project;
            var parts = project!.ProjectPartAssignments;
            var partsAssignedToPcb = parts.Where(x => x.PcbId == pcbHistory.PcbId).ToList();
            foreach (var part in partsAssignedToPcb)
            {
                var quantityRequired = part.Quantity * pcbQuantity * produceQuantity;

                // also update the parent history record parts consumed count
                pcbHistory.ProjectProduceHistory.PartsConsumed -= quantityRequired;

                if (part.Part != null)
                    part.Part.Quantity += quantityRequired;
                else
                    part.QuantityAvailable += quantityRequired;
            }
        }

        public async Task<bool> ProducePcbAsync(ProduceBomPcbRequest request)
        {
            // get all the parts in the project
            var user = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var numberOfPcbsProduced = request.Quantity;

            var project = await GetProjectAsync(request.ProjectId);
            if (project == null)
                return false;

            var parts = await context.ProjectPartAssignments
                .Include(x => x.Part)
                .Where(x => x.ProjectId == project.ProjectId && x.OrganizationId == user.OrganizationId)
                .ToListAsync();

            var produceHistory = new Data.Model.ProjectProduceHistory
            {
                ProjectId = project.ProjectId,
                ProduceUnassociated = request.Unassociated,
                Quantity = request.Quantity,
                PartsConsumed = 0,
                DateCreatedUtc = DateTime.UtcNow,
                DateModifiedUtc = DateTime.UtcNow,
                UserId = user.UserId,
                OrganizationId = user.OrganizationId,
            };

            // because some storage providers don't have transaction support, first validate we have the parts/quantities before making any changes
            var pcbPartsConsumed = await ProcessPcbParts(false);
            var partsConsumed = 0;
            if (request.Unassociated)
                partsConsumed = await ProcessNonPcbParts(false);

            // no exceptions thrown, write the changes
            produceHistory.PartsConsumed = pcbPartsConsumed + partsConsumed;
            context.ProjectProduceHistory.Add(produceHistory);
            await ProcessPcbParts(true);
            if (request.Unassociated)
                await ProcessNonPcbParts(true);

            await context.SaveChangesAsync();

            // update project (DateModified)
            project.DateModifiedUtc = DateTime.UtcNow;
            await _storageProvider.UpdateProjectAsync(project, user);

            return true;

            async Task<int> ProcessPcbParts(bool performUpdates)
            {
                var totalConsumed = 0;
                foreach (var pcb in request.Pcbs)
                {
                    var pcbConsumed = 0;
                    var pcbEntity = await context.Pcbs
                        .FirstOrDefaultAsync(x => x.PcbId == pcb.PcbId && x.OrganizationId == user.OrganizationId);
                    if (pcbEntity == null)
                        throw new InvalidOperationException($"The pcb with Id '{pcb.PcbId}' could not be found!");

                    // get the parts for this pcb in the BOM
                    var pcbParts = parts.Where(x => x.PcbId != null && x.PcbId == pcb.PcbId).ToList();
                    foreach (var pcbPart in pcbParts)
                    {
                        var quantityAvailable = pcbPart.Part?.Quantity ?? pcbPart.QuantityAvailable;
                        // get the quantity to remove, which is the number of parts used on this pcb X the number of pcb boards produced
                        // if the pcb has a quantity > 1, it acts as a multiplier for BOMs that produce multiples of a single PCB
                        // a value of 0 is invalid
                        var quantityToRemove = pcbPart.Quantity * numberOfPcbsProduced * (pcbEntity.Quantity > 0 ? pcbEntity.Quantity : 1);
                        if (quantityToRemove > quantityAvailable)
                            throw new InvalidOperationException($"There are not enough parts in inventory for part: {pcbPart.PartName}. In Stock: {pcbPart.Part?.Quantity ?? pcbPart.QuantityAvailable}, Quantity needed: {quantityToRemove}");

                        totalConsumed += quantityToRemove;
                        pcbConsumed += quantityToRemove;

                        if (performUpdates)
                        {
                            if (pcbPart.Part != null)
                                pcbPart.Part.Quantity -= quantityToRemove;
                            else
                                pcbPart.QuantityAvailable -= quantityToRemove;
                        }
                    }

                    if (performUpdates)
                    {
                        pcbEntity.LastSerialNumber = IncrementSerialNumber(pcb.SerialNumber ?? string.Empty, numberOfPcbsProduced, (serial, i) =>
                        {
                            // create a history record
                            var pcbProduceHistory = new Data.Model.ProjectPcbProduceHistory
                            {
                                ProjectProduceHistory = produceHistory,
                                DateCreatedUtc = DateTime.UtcNow,
                                DateModifiedUtc = DateTime.UtcNow,
                                PcbId = pcbEntity.PcbId,
                                PcbQuantity = pcbEntity.Quantity,
                                PcbCost = pcbEntity.Cost,
                                SerialNumber = serial,
                                PartsConsumed = pcbConsumed,
                                UserId = user.UserId,
                                OrganizationId = user.OrganizationId,
                            };
                            context.ProjectPcbProduceHistory.Add(pcbProduceHistory);
                        });
                    }
                }

                return totalConsumed;
            }

            async Task<int> ProcessNonPcbParts(bool performUpdates)
            {
                var totalConsumed = 0;
                foreach (var part in parts.Where(x => x.PcbId == null))
                {
                    var quantityAvailable = part.Part?.Quantity ?? part.QuantityAvailable;
                    // get the quantity to remove, which is the number of parts used on this pcb X the number of pcb boards produced
                    var quantityToRemove = part.Quantity * numberOfPcbsProduced;
                    if (quantityToRemove > quantityAvailable)
                        throw new InvalidOperationException(
                            $"There are not enough parts in inventory for part: {part.PartName}. In Stock: {part.Part?.Quantity ?? part.QuantityAvailable}, Quantity needed: {quantityToRemove}");
                    totalConsumed += quantityToRemove;
                    if (performUpdates)
                    {
                        if (part.Part != null)
                            part.Part.Quantity -= quantityToRemove;
                        else
                            part.QuantityAvailable -= quantityToRemove;
                    }
                }
                return totalConsumed;
            }
        }

        public string IncrementSerialNumber(string nextSerialNumber, int numberOfPcbsProduced, Action<string, int>? onIncrement = null)
        {
            if (string.IsNullOrEmpty(nextSerialNumber)) nextSerialNumber = "00000";

            var serialNumber = nextSerialNumber;
            for (var s = 1; s <= numberOfPcbsProduced; s++)
            {
                // add 1 to the serial number

                // find the index of the last non-numeric character
                var lastNonNumericIndex = 0;
                for (var i = 0; i < nextSerialNumber.Length; i++)
                {

                    var charCode = (int)nextSerialNumber[i];
                    if (charCode < 48 || charCode > 57)
                        lastNonNumericIndex = i;
                }
                // parse the remainder as an integer

                var numericLabel = nextSerialNumber.Substring(lastNonNumericIndex + 1, nextSerialNumber.Length - (lastNonNumericIndex + 1));
                if (int.TryParse(numericLabel, out var parsedNumber))
                {
                    // increment it
                    var nextSerialNumberInt = parsedNumber + (s - 1);
                    var labelPortion = nextSerialNumber.Substring(0, lastNonNumericIndex + 1);
                    serialNumber = labelPortion.PadRight(labelPortion.Length + numericLabel.Length - nextSerialNumberInt.ToString().Length, '0') + nextSerialNumberInt;
                    if (onIncrement != null) onIncrement(serialNumber, s);
                }
            }

            return serialNumber;
        }
    }
}
