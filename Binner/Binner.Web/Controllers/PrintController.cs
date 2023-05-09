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

        public PrintController(ILogger<ProjectController> logger, WebHostServiceConfiguration config, IPartService partService, ILabelPrinterHardware labelPrinter, ILabelGenerator labelGenerator, FontManager fontManager, IUserService userService)
        {
            _logger = logger;
            _config = config;
            _partService = partService;
            _labelPrinter = labelPrinter;
            _labelGenerator = labelGenerator;
            _fontManager = fontManager;
            _userService = userService;
        }

        [HttpPost("beta")]
        [AllowAnonymous]
        public async Task<IActionResult> PreviewLabel(CustomLabelRequest request)
        {
            try
            {
                var userContext = await _userService.ValidateUserImageToken(request.Token ?? string.Empty);
                if (userContext == null) return GetInvalidTokenImage();
                System.Threading.Thread.CurrentPrincipal = new TokenPrincipal(userContext, request.Token);

                var stream = new MemoryStream();

                var part = new Part
                {
                    PartNumber = "SC4096", Description = "Test simulation of a printed part", ManufacturerPartNumber = "SC4096STG-11", Manufacturer = "Texas Instruments",
                    Location = "Vancouver", BinNumber = "Bin 11", BinNumber2 = "21"
                };
                var image = _labelGenerator.CreateLabelImage(request, part);
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
