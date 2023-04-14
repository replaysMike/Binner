using Binner.Common.Configuration;
using Binner.Common.IO.Printing;
using Binner.Common.Models;
using Binner.Common.Models.Responses;
using Binner.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;

namespace Binner.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class PrintController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partService;
        private readonly ILabelPrinterHardware _labelPrinter;
        private readonly FontManager _fontManager;

        public PrintController(ILogger<ProjectController> logger, WebHostServiceConfiguration config, IPartService partService, ILabelPrinterHardware labelPrinter, FontManager fontManager)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _labelPrinter = labelPrinter;
            _fontManager = fontManager;
        }

        /// <summary>
        /// Print a custom label
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("custom")]
        public IActionResult PrintCustomLabel(PrintLabelRequest request)
        {
            try
            {
                var stream = new MemoryStream();

                Image image;
                if (!request.Lines.Any())
                    image = new BlankImage(text: "No lines specified!", fontFamily: _fontManager.InstalledFonts.Families.First()).Image;
                else
                    image = _labelPrinter.PrintLabel(request.Lines, new PrinterOptions(request.LabelSource, request.LabelName ?? string.Empty, request.GenerateImageOnly, request.ShowDiagnostic));
                image.SaveAsPng(stream);
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
    }
}
