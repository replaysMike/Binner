using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Integrations.DigiKey.V4;
using Binner.Model.Responses;
using Binner.Model.Swarm;
using System.Linq.Expressions;
using static Binner.Common.Integrations.DigikeyApi;

namespace Binner.Testing
{
    public class InMemoryStorageProvider : IStorageProvider
    {
        private readonly Dictionary<long, Part> _parts = new();
        private readonly Dictionary<long, Project> _projects = new();
        private readonly Dictionary<long, ProjectPartAssignment> _projectPartAssignments = new();
        private readonly Dictionary<long, ProjectPcbAssignment> _projectPcbAssignments = new();
        private readonly Dictionary<long, PartType> _partTypes = new();
        private readonly Dictionary<long, Pcb> _pcbs = new();
        private readonly Dictionary<long, StoredFile> _storedFiles = new();
        private readonly Dictionary<long, PcbStoredFileAssignment> _pcbStoredFileAssignments = new();
        private readonly Dictionary<long, PartSupplier> _partSuppliers = new();
        private readonly Dictionary<long, User> _users = new();

        public InMemoryStorageProvider(bool createEmpty = false)
        {
            if (!createEmpty)
            {
                _parts.Add(1, new Part { 
                    PartNumber = "LM358", 
                    PartId = 1,
                    PartTypeId = 1,
                    Quantity = 5,
                    LowStockThreshold = 10,
                    ProductUrl = "https://example.com/LM358",
                    DatasheetUrl = "https://example.com/LM358/datasheet.pdf",
                    ImageUrl = "https://example.com/LM358/image.png",
                    DigiKeyPartNumber = "LM358-ND",
                    ArrowPartNumber = "AR-LM358",
                    MouserPartNumber = "MO-LM358",
                    Location = "Vancouver",
                    BinNumber = "1",
                    BinNumber2 = "1",
                    Cost = 1.39,
                    Currency = "CAD",
                    Description = "OP AMP",
                    Keywords = new List<string> { "ic", "op amp" },
                    Manufacturer = "Texas Instruments",
                    ManufacturerPartNumber = "LM358-TI",
                    MountingTypeId = (int)MountingTypes.SurfaceMount,
                    PackageType = "DIP8",
                    UserId = 1,
                });
                _projects.Add(1, new Project { Name = "Test Project", ProjectId = 1 });
            }
            _partTypes.Add(1, new PartType { Name = "IC", PartTypeId = 1 });
            _partTypes.Add(2, new PartType { Name = "Resistor", PartTypeId = 2 });
            _partTypes.Add(3, new PartType { Name = "Capacitor", PartTypeId = 3 });
            _partTypes.Add(4, new PartType { Name = "Inductor", PartTypeId = 4 });

            if (_parts.Any())
            {
                _partSuppliers.Add(1, new PartSupplier
                {
                    PartSupplierId = 1,
                    Name = "DigiKey",
                    PartId = 1,
                    Part = _parts.First().Value,
                    Cost = 1.39,
                    QuantityAvailable = 1000,
                    ProductUrl = "https://example.com/LM358",
                    ImageUrl = "https://example.com/LM358/image.png",
                    SupplierPartNumber = "LM358-ND",
                    MinimumOrderQuantity = 1,
                    UserId = 1,
                });
            }
        }

        public async Task<Part> AddPartAsync(Part part, IUserContext? userContext)
        {
            part.UserId = userContext?.UserId;
            var id = _parts.OrderByDescending(x => x.Key).Select(x => x.Key).FirstOrDefault() + 1;
            part.PartId = id;
            _parts.Add(id, part);
            return part;
        }

        public async Task<PartSupplier> AddPartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext)
        {
            partSupplier.UserId = userContext?.UserId;
            var id = _partSuppliers.OrderByDescending(x => x.Key).Select(x => x.Key).FirstOrDefault() + 1;
            partSupplier.PartSupplierId = id;
            _partSuppliers.Add(id, partSupplier);
            return partSupplier;
        }

        public async Task<Pcb> AddPcbAsync(Pcb pcb, IUserContext? userContext)
        {
            pcb.UserId = userContext?.UserId;
            var id = _pcbs.OrderByDescending(x => x.Key).Select(x => x.Key).FirstOrDefault() + 1;
            pcb.PcbId = id;
            _pcbs.Add(id, pcb);
            return pcb;
        }

        public async Task<PcbStoredFileAssignment> AddPcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext)
        {
            assignment.UserId = userContext?.UserId ?? 0;
            var id = _pcbStoredFileAssignments.OrderByDescending(x => x.Key).Select(x => x.Key).FirstOrDefault() + 1;
            assignment.PcbStoredFileAssignmentId = id;
            _pcbStoredFileAssignments.Add(id, assignment);
            return assignment;
        }

        public async Task<Project> AddProjectAsync(Project project, IUserContext? userContext)
        {
            project.UserId = userContext?.UserId;
            var id = _projects.OrderByDescending(x => x.Key).Select(x => x.Key).FirstOrDefault() + 1;
            project.ProjectId = id;
            _projects.Add(id, project);
            return project;
        }

        public async Task<ProjectPartAssignment> AddProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext)
        {
            assignment.UserId = userContext?.UserId ?? 0;
            var id = _projectPartAssignments.OrderByDescending(x => x.Key).Select(x => x.Key).FirstOrDefault() + 1;
            assignment.ProjectPartAssignmentId = id;
            _projectPartAssignments.Add(id, assignment);
            return assignment;
        }

        public async Task<ProjectPcbAssignment> AddProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext)
        {
            assignment.UserId = userContext?.UserId ?? 0;
            var id = _projectPcbAssignments.OrderByDescending(x => x.Key).Select(x => x.Key).FirstOrDefault() + 1;
            assignment.ProjectPcbAssignmentId = id;
            _projectPcbAssignments.Add(id, assignment);
            return assignment;
        }

        public async Task<StoredFile> AddStoredFileAsync(StoredFile storedFile, IUserContext? userContext)
        {
            storedFile.UserId = userContext?.UserId ?? 0;
            var id = _storedFiles.OrderByDescending(x => x.Key).Select(x => x.Key).FirstOrDefault() + 1;
            storedFile.StoredFileId = id;
            _storedFiles.Add(id, storedFile);
            return storedFile;
        }

        public Task<OAuthAuthorization> CreateOAuthRequestAsync(OAuthAuthorization authRequest, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePartAsync(Part part, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePartTypeAsync(PartType partType, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePcbAsync(Pcb pcb, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteProjectAsync(Project project, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteStoredFileAsync(StoredFile storedFile, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<SearchResult<Part>>> FindPartsAsync(string keywords, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public async Task<IBinnerDb> GetDatabaseAsync(IUserContext? userContext)
        {
            return new LegacyBinnerDb
            {
                Parts = _parts.Values,
                Pcbs = _pcbs.Values,
                PartTypes = _partTypes.Values,
                ProjectPartAssignments = _projectPartAssignments.Values,
                PartSuppliers = _partSuppliers.Values,
                PcbStoredFileAssignments = _pcbStoredFileAssignments.Values,
                ProjectPcbAssignments = _projectPcbAssignments.Values,
                Projects = _projects.Values,
                StoredFiles = _storedFiles.Values,
                Count = _parts.Count,
            };
        }

        public Task<PaginatedResponse<Part>> GetLowStockAsync(PaginatedRequest request, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<OAuthCredential?> GetOAuthCredentialAsync(string providerName, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<OAuthAuthorization?> GetOAuthRequestAsync(Guid requestId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public async Task<PartType?> GetOrCreatePartTypeAsync(PartType partType, IUserContext? userContext)
        {
            if (_partTypes.Where(x => x.Value.Name == partType.Name).Any())
                return _partTypes.Where(x => x.Value.Name == partType.Name).Select(x => x.Value).First();
            var id = _partTypes.OrderByDescending(x => x.Key).Select(x => x.Key).FirstOrDefault() + 1;
            partType.PartTypeId = id;
            _partTypes.Add(id, partType);
            return partType;
        }

        public Task<ICollection<ProjectPartAssignment>> GetPartAssignmentsAsync(long partId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public async Task<Part?> GetPartAsync(long partId, IUserContext? userContext)
        {
            return _parts.Where(x => x.Key == partId).Select(x => x.Value).FirstOrDefault();
        }

        public async Task<Part?> GetPartAsync(string partNumber, IUserContext? userContext)
        {
            return _parts.Where(x => x.Value.PartNumber == partNumber).Select(x => x.Value).FirstOrDefault();
        }

        public async Task<PaginatedResponse<Part>> GetPartsAsync(PaginatedRequest request, IUserContext? userContext)
        {
            //return _parts.Select(x => x.Value).ToList();
            throw new NotImplementedException();
        }

        public async Task<ICollection<Part>> GetPartsAsync(Expression<Func<Part, bool>> predicate, IUserContext? userContext)
        {
            return _parts.Select(x => x.Value).ToList();
        }

        public async Task<long> GetPartsCountAsync(IUserContext? userContext)
        {
            return _parts.Count();
        }

        public Task<PartSupplier?> GetPartSupplierAsync(long partSupplierId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<PartSupplier>> GetPartSuppliersAsync(long partId, IUserContext? userContext)
        {
            return _partSuppliers.Where(x => x.Key == partId).Select(x => x.Value).ToList();
        }

        public Task<decimal> GetPartsValueAsync(IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public async Task<PartType?> GetPartTypeAsync(long partTypeId, IUserContext? userContext)
        {
            return _partTypes.Where(x => x.Key == partTypeId).Select(x => x.Value).FirstOrDefault();
        }

        public async Task<ICollection<PartType>> GetPartTypesAsync(IUserContext? userContext)
        {
            return _partTypes.Select(x => x.Value).ToList();
        }

        public async Task<Pcb?> GetPcbAsync(long pcbId, IUserContext? userContext)
        {
            return _pcbs.Where(x => x.Key == pcbId).Select(x => x.Value).FirstOrDefault();
        }

        public async Task<ICollection<Pcb>> GetPcbsAsync(long projectId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<PcbStoredFileAssignment?> GetPcbStoredFileAssignmentAsync(long pcbStoredFileAssignmentId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<PcbStoredFileAssignment>> GetPcbStoredFileAssignmentsAsync(long pcbId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public async Task<Project?> GetProjectAsync(long projectId, IUserContext? userContext)
        {
            return _projects.Where(x => x.Key == projectId).Select(x => x.Value).FirstOrDefault();
        }

        public async Task<Project?> GetProjectAsync(string projectName, IUserContext? userContext)
        {
            return _projects.Where(x => x.Value.Name == projectName).Select(x => x.Value).FirstOrDefault();
        }

        public Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectPartAssignmentId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectId, long partId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectPartAssignment?> GetProjectPartAssignmentAsync(long projectId, string partName, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ProjectPartAssignment>> GetProjectPartAssignmentsAsync(long projectId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ProjectPartAssignment>> GetProjectPartAssignmentsAsync(long projectId, PaginatedRequest request, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectPcbAssignment?> GetProjectPcbAssignmentAsync(long projectPcbAssignmentId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ProjectPcbAssignment>> GetProjectPcbAssignmentsAsync(long projectId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Project>> GetProjectsAsync(PaginatedRequest request, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<StoredFile?> GetStoredFileAsync(long storedFileId, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<StoredFile?> GetStoredFileAsync(string filename, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<StoredFile>> GetStoredFilesAsync(long partId, StoredFileType? fileType, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<StoredFile>> GetStoredFilesAsync(PaginatedRequest request, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetUniquePartsCountAsync(IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task RemoveOAuthCredentialAsync(string providerName, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemovePcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<OAuthCredential> SaveOAuthCredentialAsync(OAuthCredential credential, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ConnectionResponse> TestConnectionAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OAuthAuthorization> UpdateOAuthRequestAsync(OAuthAuthorization authRequest, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<Part> UpdatePartAsync(Part part, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<PartSupplier> UpdatePartSupplierAsync(PartSupplier partSupplier, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<PartType> UpdatePartTypeAsync(PartType partType, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<Pcb> UpdatePcbAsync(Pcb pcb, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<PcbStoredFileAssignment> UpdatePcbStoredFileAssignmentAsync(PcbStoredFileAssignment assignment, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<Project> UpdateProjectAsync(Project project, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectPartAssignment> UpdateProjectPartAssignmentAsync(ProjectPartAssignment assignment, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectPcbAssignment> UpdateProjectPcbAssignmentAsync(ProjectPcbAssignment assignment, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public Task<StoredFile> UpdateStoredFileAsync(StoredFile storedFile, IUserContext? userContext)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _parts.Clear();
            _projects.Clear();
            _projectPartAssignments.Clear();
            _projectPcbAssignments.Clear();
            _partTypes.Clear();
            _pcbs.Clear();
            _storedFiles.Clear();
            _pcbStoredFileAssignments.Clear();
            _partSuppliers.Clear();
            _users.Clear();
        }
    }
}
