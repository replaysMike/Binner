using AnyMapper;
using Binner.Common.Configuration;
using Binner.Common.Models;
using Binner.Common.Models.Requests;
using Binner.Common.Models.Swarm.Requests;
using Binner.Common.Services;
using Binner.Model.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class ProjectController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partService;
        private readonly IProjectService _projectService;

        public ProjectController(ILogger<ProjectController> logger, WebHostServiceConfiguration config, IPartService partService, IProjectService projectService)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _projectService = projectService;
        }

        /// <summary>
        /// Get an existing project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery]GetProjectRequest request)
        {
            Project? project = null;
            if (!string.IsNullOrEmpty(request.Name))
            {
                project = await _projectService.GetProjectAsync(request.Name);
            }
            else
            {
                project = await _projectService.GetProjectAsync(request.ProjectId);
            }
            
            if (project == null) return NotFound();
            
            var bomResponse = Mapper.Map<Project, BomResponse>(project);
            var partsForProject = await _partService.GetPartsAsync(x => x.ProjectId == project.ProjectId);
            bomResponse.Parts = Mapper.Map<ICollection<Part>, ICollection<PartResponse>>(partsForProject);
            var partTypes = await _partService.GetPartTypesAsync();
            foreach (var part in bomResponse.Parts)
            {
                part.PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name).FirstOrDefault();
                part.MountingType = ((MountingType)part.MountingTypeId).ToString();
                part.Keywords = string.Join(" ", partsForProject.First(x => x.PartId == part.PartId).Keywords ?? new List<string>());
            }

            return Ok(bomResponse);
        }

        /// <summary>
        /// Get an existing project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetProjectsAsync([FromQuery]PaginatedRequest request)
        {
            var projects = await _projectService.GetProjectsAsync(request);
            var projectsResponse = Mapper.Map<ICollection<Project>, ICollection<ProjectResponse>>(projects);
            foreach (var project in projectsResponse)
            {
                var partsForProject = await _partService.GetPartsAsync(x => x.ProjectId == project.ProjectId);
                project.Parts = partsForProject.Count;
            }

            return Ok(projectsResponse);
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateProjectAsync(CreateProjectRequest request)
        {
            var mappedProject = Mapper.Map<CreateProjectRequest, Project>(request);
            mappedProject.DateCreatedUtc = DateTime.UtcNow;
            var project = await _projectService.AddProjectAsync(mappedProject);
            return Ok(project);
        }

        /// <summary>
        /// Update an existing project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateProjectAsync(UpdateProjectRequest request)
        {
            var mappedProject = Mapper.Map<UpdateProjectRequest, Project>(request);
            var project = await _projectService.UpdateProjectAsync(mappedProject);
            return Ok(project);
        }

        /// <summary>
        /// Delete an existing project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeletePartAsync(DeleteProjectRequest request)
        {
            var isDeleted = await _projectService.DeleteProjectAsync(new Project
            {
                ProjectId = request.ProjectId
            });
            return Ok(isDeleted);
        }

        /// <summary>
        /// Add part to project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("part")]
        public async Task<IActionResult> AddPartProjectAsync(AddBomPartRequest request)
        {
            var project = await _projectService.AddPartAsync(request);
            if (project == null)
                return NotFound();
            return Ok(project);
        }

        /// <summary>
        /// Update part details in project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("part")]
        public async Task<IActionResult> UpdatePartProjectAsync(UpdateBomPartRequest request)
        {
            var project = await _projectService.UpdatePartAsync(request);
            if (project == null)
                return NotFound();
            return Ok(project);
        }

        /// <summary>
        /// Remove a part from a project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("part")]
        public async Task<IActionResult> RemovePartProjectAsync(RemoveBomPartRequest request)
        {
            var isDeleted = await _projectService.RemovePartAsync(request);
            return Ok(isDeleted);
        }
    }
}
