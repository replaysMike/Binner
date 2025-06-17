using AutoMapper;
using Binner.Services;
using Binner.Global.Common;
using Binner.Model.KiCad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    /// <summary>
    /// Download file controller
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class DownloadController : ControllerBase
    {
        private const string KiCadConfigFilename = "Binner.kicad_httplib";
        private readonly ILogger<DownloadController> _logger;
        private readonly IRequestContextAccessor _requestContextAccessor;
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;

        public DownloadController(ILogger<DownloadController> logger, IMapper mapper, IAccountService accountService, IRequestContextAccessor requestContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _accountService = accountService;
            _requestContextAccessor = requestContextAccessor;
        }

        /// <summary>
        /// Download a KiCad configuration file
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("kicad")]
        public async Task<IActionResult> DownloadKiCadConfigAsync([FromQuery]string token)
        {
            var entity = await _accountService.GetTokenAsync(token, Model.Authentication.TokenTypes.KiCadApiToken);
            if (entity == null) return NotFound();

            var kiCadTimeouts = new KiCadTimeouts();
            if (!string.IsNullOrEmpty(entity.TokenConfig))
                kiCadTimeouts = JsonConvert.DeserializeObject<KiCadTimeouts>(entity.TokenConfig);
            var config = new KiCadHttpLibraryConfig(token, kiCadTimeouts);
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            var data = JsonConvert.SerializeObject(config, settings);
            writer.Write(data);
            await writer.FlushAsync();
            return ExportToFile(stream);
        }

        private IActionResult ExportToFile(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/octet-stream", KiCadConfigFilename);
        }
    }
}
