using Binner.Common.Models;
using Binner.Common.Services;
using Binner.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly IProjectService _projectService;

        public ProjectController(ILogger<ProjectController> logger, IMemoryCache cache, WebHostServiceConfiguration config, IProjectService projectService)
        {
            _logger = logger;
            _cache = cache;
            _config = config;
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
            if (request == null)
                return Ok(await _projectService.GetProjectsAsync());
            var project = await _projectService.GetProjectAsync(request.ProjectId);
            if (project == null) return NotFound();

            return Ok(project);
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateProjectAsync(CreateProjectRequest request)
        {
            var project = await _projectService.AddProjectAsync(new Project
            {
                Name = request.Name,
                Description = request.Description,
                Location = request.Location,
                DateCreatedUtc = DateTime.UtcNow
            });
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
            var project = await _projectService.UpdateProjectAsync(new Project
            {
                ProjectId = request.ProjectId,
                Name = request.Name,
                Description = request.Description,
                Location = request.Location
            });
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
