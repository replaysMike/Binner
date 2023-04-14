using AnyMapper;
using Binner.Common.Configuration;
using Binner.Common.IO;
using Binner.Common.Models;
using Binner.Common.Models.Requests;
using Binner.Common.Models.Responses;
using Binner.Common.Services;
using Binner.Model.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    /// <summary>
    /// Bill of Materials (BOM) part management controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class BomController : ControllerBase
    {
        private readonly ILogger<BomController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partService;
        private readonly IProjectService _projectService;
        private readonly IPcbService _pcbService;

        public BomController(ILogger<BomController> logger, WebHostServiceConfiguration config, IPartService partService, IProjectService projectService, IPcbService pcbService)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _projectService = projectService;
            _pcbService = pcbService;
        }

        /// <summary>
        /// Get an existing BOM project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetProjectRequest request)
        {
            var bomResponse = await GetBomResponseAsync(request);
            if (bomResponse == null)
                return NotFound();

            return Ok(bomResponse);
        }

        /// <summary>
        /// Get a list of BOM projects
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetProjectsAsync([FromQuery] PaginatedRequest request)
        {
            var projects = await _projectService.GetProjectsAsync(request);
            var projectsResponse = new List<BomBasicResponse>();
            foreach (var project in projects)
            {
                var bomResponse = Mapper.Map<Project, BomBasicResponse>(project);
                bomResponse.PartCount = (await _projectService.GetPartsAsync(project.ProjectId)).Count;
                bomResponse.PcbCount = (await _projectService.GetPcbsAsync(project.ProjectId)).Count;
                projectsResponse.Add(bomResponse);
            }

            return Ok(projectsResponse);
        }

        public async Task<BomResponse?> GetBomResponseAsync(GetProjectRequest request)
        {
            Project? project = null;
            if (!string.IsNullOrEmpty(request.Name))
            {
                project = await _projectService.GetProjectAsync(request.Name);
            }
            else if (request.ProjectId > 0)
            {
                project = await _projectService.GetProjectAsync(request.ProjectId);
            }
            else
            {
                return null;
            }

            if (project == null) return null;

            var bomResponse = Mapper.Map<Project, BomResponse>(project);
            bomResponse.Parts = await _projectService.GetPartsAsync(project.ProjectId);
            bomResponse.Pcbs = await _projectService.GetPcbsAsync(project.ProjectId);
            var partTypes = await _partService.GetPartTypesAsync();
            foreach (var projectPart in bomResponse.Parts)
            {
                var part = projectPart.Part;
                if (part != null)
                {
                    part.PartType = partTypes.Where(x => x.PartTypeId == part.PartTypeId).Select(x => x.Name)
                        .FirstOrDefault();
                    part.MountingType = ((MountingType)part.MountingTypeId).ToString();
                    // part.Keywords = string.Join(" ", bomResponse.Parts.First(x => x.PartId == part.PartId).Keywords ?? new List<string>());
                }
            }
            return bomResponse;
        }

        /// <summary>
        /// Delete a project (BOM)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteProjectAsync(DeleteProjectRequest request)
        {
            var isDeleted = await _projectService.DeleteProjectAsync(new Project
            {
                ProjectId = request.ProjectId
            });
            return Ok(isDeleted);
        }

        /// <summary>
        /// Add part to BOM
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("part")]
        public async Task<IActionResult> AddPartProjectAsync(AddBomPartRequest request)
        {
            var projectPart = await _projectService.AddPartAsync(request);
            if (projectPart == null)
                return NotFound();
            return Ok(projectPart);
        }

        /// <summary>
        /// Update part details in BOM
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("part")]
        public async Task<IActionResult> UpdatePartProjectAsync(UpdateBomPartRequest request)
        {
            var projectPart = await _projectService.UpdatePartAsync(request);
            if (projectPart == null)
                return NotFound();
            return Ok(projectPart);
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

        /// <summary>
        /// Get pcb
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("pcb")]
        public async Task<IActionResult> GetPcbAsync(GetBomPcbRequest request)
        {
            var pcb = await _pcbService.GetPcbAsync(request.PcbId);
            if (pcb == null)
                return NotFound();
            return Ok(pcb);
        }

        /// <summary>
        /// Create a new pcb
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("pcb")]
        public async Task<IActionResult> CreatePcb(CreateBomPcbRequest request)
        {
            var pcb = await _pcbService.AddPcbAsync(new Pcb
            {
                Name = request.Name,
                Description = request.Description,
                SerialNumberFormat = request.SerialNumberFormat,
                LastSerialNumber = request.SerialNumber,
                Quantity = request.Quantity,
                Cost = request.Cost
            }, request.ProjectId);
            if (pcb == null)
                return NotFound();
            return Ok(pcb);
        }

        /// <summary>
        /// Update pcb
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("pcb")]
        public async Task<IActionResult> UpdatePcbAsync(Pcb request)
        {
            var pcb = await _pcbService.UpdatePcbAsync(request);
            if (pcb == null)
                return NotFound();
            return Ok(pcb);
        }

        /// <summary>
        /// Delete a pcb
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("pcb")]
        public async Task<IActionResult> RemovePcbAsync(RemoveBomPcbRequest request)
        {
            var pcb = await _pcbService.GetPcbAsync(request.PcbId);
            if (pcb == null)
                return NotFound();
            var isDeleted = await _pcbService.DeletePcbAsync(pcb, request.ProjectId);
            return Ok(isDeleted);
        }

        /// <summary>
        /// Produce a pcb which will adjust inventory quantities accordingly
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("produce")]
        public async Task<IActionResult> ProducePcbs(ProduceBomPcbRequest request)
        {
            try
            {
                var success = await _projectService.ProducePcbAsync(request);
                if (success)
                {
                    return await GetAsync(new GetProjectRequest
                    {
                        ProjectId = request.ProjectId
                    });
                }

                return BadRequest();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Failed to produce pcb", ex));
            }
        }

        /// <summary>
        /// Download a BOM list
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("download")]
        public async Task<IActionResult> DownloadBomAsync(DownloadBomRequest request)
        {
            var bomResponse = await GetBomResponseAsync(new GetProjectRequest { Name = request.Name, ProjectId = request.ProjectId });
            if (bomResponse == null)
                return NotFound();
            switch (request.Format)
            {
                default:
                case DownloadFormats.Csv:
                {
                    var exporter = new BomCsvExporter();
                    var streams = exporter.Export(bomResponse);
                    return ExportToFile(streams, $"{bomResponse.Name}-Csv-BOM");
                }
                case DownloadFormats.Excel:
                {
                    var exporter = new BomExcelExporter();
                    var file = exporter.Export(bomResponse);
                    return ExportToFile(file, $"{bomResponse.Name}-Excel-BOM");
                }
            }
        }

        private IActionResult ExportToFile(IDictionary<StreamName, Stream> streams, string filename)
        {
            var zipStream = new MemoryStream();
            using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var stream in streams)
                {
                    // write stream to a buffer
                    stream.Value.Seek(0, SeekOrigin.Begin);
                    var buffer = new byte[stream.Value.Length];
                    stream.Value.Read(buffer, 0, (int)stream.Value.Length);

                    // add the stream to the zipfile
                    var file = zipFile.CreateEntry($"{stream.Key.Name}.{stream.Key.FileExtension}");
                    using var fileStream = file.Open();
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
            zipStream.Seek(0, SeekOrigin.Begin);
            return File(zipStream, "application/octet-stream", $"{filename}.zip");
        }

        private IActionResult ExportToFile(Stream stream, string filename)
        {
            var zipStream = new MemoryStream();
            using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // write stream to a buffer
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);

                // add the stream to the zipfile
                var file = zipFile.CreateEntry($"{filename}.xlsx");
                using var fileStream = file.Open();
                fileStream.Write(buffer, 0, buffer.Length);
            }
            zipStream.Seek(0, SeekOrigin.Begin);
            return File(zipStream, "application/octet-stream", $"{filename}.zip");
        }
    }
}
