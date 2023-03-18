using Binner.Model.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class PcbService : IPcbService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly RequestContextAccessor _requestContext;

        public PcbService(IStorageProvider storageProvider, RequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
        }

        public async Task<Pcb> AddPcbAsync(Pcb pcb, long projectId)
        {
            var user = _requestContext.GetUserContext();
            var addedPcb = await _storageProvider.AddPcbAsync(pcb, user);
            await _storageProvider.AddProjectPcbAssignmentAsync(new ProjectPcbAssignment
            {
                PcbId = addedPcb.PcbId,
                ProjectId = projectId,
            }, user);
            return addedPcb;
        }

        public async Task<bool> DeletePcbAsync(Pcb pcb, long projectId)
        {
            var user = _requestContext.GetUserContext();
            // delete project pcb assignment
            await _storageProvider.RemoveProjectPcbAssignmentAsync(new ProjectPcbAssignment{ PcbId = pcb.PcbId, ProjectId = projectId}, user);

            // delete any stored file assignments associated with pcb
            var storedFileAssignments = await _storageProvider.GetPcbStoredFileAssignmentsAsync(pcb.PcbId, user);
            foreach (var storedFileAssignment in storedFileAssignments)
            {
                await _storageProvider.RemovePcbStoredFileAssignmentAsync(storedFileAssignment, user);
            }
            // remove any part assignments associated with this pcb
            var projectPartAssignments = await _storageProvider.GetProjectPartAssignmentsAsync(projectId, user);
            foreach (var partAssignment in projectPartAssignments.Where(x => x.PcbId == pcb.PcbId))
            {
                await _storageProvider.RemoveProjectPartAssignmentAsync(partAssignment, user);
            }
            var success = await _storageProvider.DeletePcbAsync(pcb, user);
            return success;
        }

        public async Task<Pcb?> GetPcbAsync(long pcbId)
        {
            return await _storageProvider.GetPcbAsync(pcbId, _requestContext.GetUserContext());
        }

        public async Task<ICollection<Pcb>> GetPcbsAsync(long projectId)
        {
            return await _storageProvider.GetPcbsAsync(projectId, _requestContext.GetUserContext());
        }

        public async Task<Pcb?> UpdatePcbAsync(Pcb pcb)
        {
            return await _storageProvider.UpdatePcbAsync(pcb, _requestContext.GetUserContext());
        }
    }
}
