using Binner.Common.IO.Printing;
using Binner.Common.Models;
using Binner.Common.Services;
using Binner.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mime;

namespace Binner.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class PrintController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly WebHostServiceConfiguration _config;
        private readonly IPartService _partService;
        private readonly ILabelPrinter _labelPrinter;

        public PrintController(ILogger<ProjectController> logger, WebHostServiceConfiguration config, IPartService partService, ILabelPrinter labelPrinter)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _labelPrinter = labelPrinter;
        }

        /// <summary>
        /// Print a custom label
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("custom")]
        public IActionResult PrintCustomLabel(PrintLabelRequest request)
        {
            var stream = new MemoryStream();
            var image = _labelPrinter.PrintLabel(request.Lines, new PrinterOptions(request.LabelSource, request.LabelName, request.GenerateImageOnly, request.ShowDiagnostic));
            image.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, "image/png");
        }
    }
}
