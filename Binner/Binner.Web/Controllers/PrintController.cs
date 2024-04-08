using Binner.Common;
using Binner.Common.IO.Printing;
using Binner.Common.Services;
using Binner.Model.Configuration;
using Binner.Model.IO.Printing;
using Binner.Model.IO.Printing.PrinterHardware;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Binner.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Binner.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class PrintController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partService;
        private readonly ILabelPrinterHardware _labelPrinter;
        private readonly ILabelGenerator _labelGenerator;
        private readonly FontManager _fontManager;
        private readonly IUserService _userService;
        private readonly IPrintService _printService;

        public PrintController(ILogger<ProjectController> logger, WebHostServiceConfiguration config, IPartService partService, ILabelPrinterHardware labelPrinter, ILabelGenerator labelGenerator, FontManager fontManager, IUserService userService, IPrintService printService)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _labelPrinter = labelPrinter;
            _labelGenerator = labelGenerator;
            _fontManager = fontManager;
            _userService = userService;
            _printService = printService;
        }

        /// <summary>
        /// Print or preview a label
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("beta")]
        [AllowAnonymous]
        public async Task<IActionResult> PrintLabel(CustomLabelRequest request)
        {
            try
            {
                var userContext = await _userService.ValidateUserImageToken(request.Token ?? string.Empty);
                if (userContext == null) return GetInvalidTokenImage();
                System.Threading.Thread.CurrentPrincipal = new TokenPrincipal(userContext, request.Token);

                if (request.Label.LabelTemplateId == null)
                    return BadRequest("Label must have a LabelTemplateId!");

                var stream = new MemoryStream();

                var part = new Part
                {
                    PartId = 1,
                    PartNumber = "NCC-1701-G",
                    Description = "This part is an placeholder for an actual part for print testing purposes.",
                    ManufacturerPartNumber = "NCC-1701-G-TI4473",
                    Manufacturer = "Texas Instruments",
                    Location = "Home",
                    BinNumber = "11",
                    BinNumber2 = "21",
                    Cost = 0.99,
                    MountingTypeId = 1,
                    PartTypeId = 18,
                    PackageType = "DIP8",
                    Quantity = 500,
                    Keywords = new List<string> { "example product", "ic", "sensor" },
                    DigiKeyPartNumber = "701-7011-1-ND",
                    MouserPartNumber = "MS-7011",
                    ArrowPartNumber = "AR-7011",
                    TmePartNumber = "TM-7011",
                    FootprintName = "DIP-20",
                    SymbolName = "IC1",
                    ExtensionValue1 = "Custom Value 1",
                    ExtensionValue2 = "Custom Value 2",
                    ProjectId = 1,
                    Currency = "USD",
                };
                var image = _labelGenerator.CreateLabelImage(request, part);
                await image.SaveAsPngAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                // load the label template
                var template = await _printService.GetLabelTemplateAsync(request.Label.LabelTemplateId.Value);

                if (!request.GenerateImageOnly)
                    _labelPrinter.PrintLabelImage(image, new PrinterOptions((LabelSource)(template?.LabelPaperSource ?? 0), template.Name, false));

                return new FileStreamResult(stream, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Print Error! ", ex));
            }
        }

        /// <summary>
        /// Create a label template
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("template")]
        public async Task<IActionResult> CreateLabelTemplateAsync(LabelTemplate request)
        {
            try
            {
                var model = await _printService.AddLabelTemplateAsync(request);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Print Error! ", ex));
            }
        }

        /// <summary>
        /// Update a label template
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("template")]
        public async Task<IActionResult> UpdateLabelTemplateAsync(LabelTemplate request)
        {
            try
            {
                var model = await _printService.UpdateLabelTemplateAsync(request);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Print Error! ", ex));
            }
        }

        /// <summary>
        /// List all label templates
        /// </summary>
        /// <returns></returns>
        [HttpGet("templates")]
        public async Task<IActionResult> GetLabelTemplatesAsync()
        {
            try
            {
                var models = await _printService.GetLabelTemplatesAsync();

                return Ok(models);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Print Error! ", ex));
            }
        }

        /// <summary>
        /// Create or update a label
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("label")]
        public async Task<IActionResult> CreateLabelAsync(CreateLabelRequest request)
        {
            try
            {
                if (request.Label.LabelTemplateId == null)
                    return BadRequest();
                var jsonOptions = new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var labelDef = new CustomLabelDefinition
                {
                    Boxes = request.Boxes,
                    Label = request.Label
                };
                var label = new Label
                {
                    LabelId = request.LabelId ?? 0,
                    LabelTemplateId = request.Label.LabelTemplateId.Value,
                    Template = JsonConvert.SerializeObject(labelDef, jsonOptions),
                    Name = request.Name,
                    IsPartLabelTemplate = request.IsDefaultPartLabel
                };
                var model = await _printService.AddOrUpdateLabelAsync(label);

                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Print Error! ", ex));
            }
        }

        /// <summary>
        /// List all labels
        /// </summary>
        /// <returns></returns>
        [HttpGet("labels")]
        public async Task<IActionResult> GetLabelsAsync()
        {
            try
            {
                var models = await _printService.GetLabelsAsync();

                return Ok(models);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Print Error! ", ex));
            }
        }

        /// <summary>
        /// Print a custom label
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("custom")]
        [AllowAnonymous]
        public async Task<IActionResult> PrintCustomLabelAsync(PrintLabelRequest request)
        {
            try
            {
                var userContext = await _userService.ValidateUserImageToken(request.Token ?? string.Empty);
                if (userContext == null) return GetInvalidTokenImage();
                System.Threading.Thread.CurrentPrincipal = new TokenPrincipal(userContext, request.Token);

                var stream = new MemoryStream();

                Image image;
                if (!request.Lines.Any())
                    image = new BlankImage(text: "No lines specified!", fontFamily: _fontManager.InstalledFonts.Families.First()).Image;
                else
                    image = _labelPrinter.PrintLabel(request.Lines, new PrinterOptions(request.LabelSource, request.LabelName ?? string.Empty, request.GenerateImageOnly, request.ShowDiagnostic));
                await image.SaveAsPngAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return new FileStreamResult(stream, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Print Error! ", ex));
            }
        }

        /// <summary>
        /// Get a list of available fonts
        /// </summary>
        /// <returns></returns>
        [HttpGet("fonts")]
        public IActionResult GetFonts()
        {
            try
            {
                var fontFiles = _fontManager.InstalledFonts.Families.Select(x => x.Name);
                return Ok(fontFiles);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ExceptionResponse("Fonts Error! ", ex));
            }

        }

        private FileStreamResult GetInvalidTokenImage()
        {
            var image = new BlankImage(300, 100, Color.Red, Color.Red, "Invalid Image Token!\nYou may need to re-login.");
            var stream = new MemoryStream();
            image.Image.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "image/png");
        }
    }
}
