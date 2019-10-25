using AnyMapper;
using Binner.Common.Models;
using Binner.Common.Services;
using Binner.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly IMemoryCache _cache;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partsService;
        private readonly IProjectService _projectService;

        public ProjectController(ILogger<ProjectController> logger, IMemoryCache cache, WebHostServiceConfiguration config, IPartService partsService, IProjectService projectService)
        {
            _logger = logger;
            _cache = cache;
            _config = config;
            _partsService = partsService;
            _projectService = projectService;
        }

        /// <summary>
        /// Get an existing project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync(GetProjectRequest request)
        {
            var project = await _projectService.GetProjectAsync(request.ProjectId);
            if (project == null) return NotFound();

            return Ok(project);
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
                var partsForProject = await _partsService.GetPartsAsync(x => x.ProjectId == project.ProjectId);
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
    }
}
