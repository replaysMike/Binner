using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Security.Claims;
using Binner.Data;
using System.Linq;
using Binner.Model.Authentication;
using Newtonsoft.Json;
using Binner.Model.BinnerBin;
using System.Threading;

namespace Binner.Web.Controllers
{
    public class RefCleanupService : Microsoft.Extensions.Hosting.BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                HighlightController.CleanupRefs();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    public class highlight {
        public readonly int Color;
        public Dictionary<int, DateTime> UserIds;

        public highlight(int color, int userId, DateTime time) 
        {
            Color = color;
            UserIds = new Dictionary<int, DateTime>
            {
                { userId, time }
            };
        }

        public void update(int userId, DateTime time)
        {
            if (UserIds.ContainsKey(userId))
            {
                UserIds[userId] = time;
            }
            else
            {
                UserIds.Add(userId, time);
            }
        }

        public bool expired(DateTime time, int timeout)
        {
            foreach (var UserId in UserIds) {
                if (UserId.Value.AddSeconds(timeout) > time) {
                    return false;
                }
            }

            return true;
        }
    };

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class HighlightController : ControllerBase
    {
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private static readonly Random rnd = new Random();

        private static Dictionary<BinnerBinConfig, highlight> highlights = new Dictionary<BinnerBinConfig, highlight>();
        private readonly static Mutex m = new Mutex();

        // all the available colors in css with names
        private readonly string[] availableColors = { 
            "red", "orange", "turquoise", "green", "yellow", "darkgoldenrod",
            "darkslateblue", "blue", "skyblue", "pink", "magenta", "purple"
        };

        public HighlightController(IDbContextFactory<BinnerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Update a part to highlight
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("update")]
        public async Task<IActionResult> PartHighlight([FromQuery] string partNumber)
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId;

            if (user == null || !Int32.TryParse(user, out userId))
            {
                return Unauthorized();
            }

            await using var dbContext = await _contextFactory.CreateDbContextAsync();

            // check if any of the parts have the provided location 
            var part = await dbContext.Parts.Where(x =>
                x.PartNumber == partNumber
            ).FirstOrDefaultAsync();

            // check if we have found a part
            if (part == null || part.Location == string.Empty || part.BinNumber == string.Empty) 
            {
                return BadRequest();
            }

            // get the bin we need to show for this part
            BinnerBinConfig config = new BinnerBinConfig(part.Location, part.BinNumber, part.BinNumber2);

            m.WaitOne();
            try
            {
                if (highlights.ContainsKey(config)) {
                    highlights[config].update(userId, DateTime.UtcNow);
                }
                else {
                    highlights.Add(config, new highlight(rnd.Next(0, availableColors.Length - 1), userId, DateTime.UtcNow));
                }
                
                // return the color for the request
                return Ok(new {color = availableColors[highlights[config].Color]});
            }
            finally
            {
                m.ReleaseMutex();
            }
        }

        /// <summary>
        /// Get an the status for one of the bins. Note we use POST here to prevent caching
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetAsync([FromQuery] string apiKey)
        {
            await using var dbContext = await _contextFactory.CreateDbContextAsync();
            var tokenEntity = await dbContext.UserTokens.Where(x => 
                x.TokenTypeId == TokenTypes.BinnerBinApiToken && 
                x.Token == apiKey && 
                x.DateRevokedUtc == null && 
                (x.DateExpiredUtc > DateTime.UtcNow || x.DateExpiredUtc == null)
            ).FirstOrDefaultAsync();

            // check if authorized
            if (tokenEntity == null)
            {
                return Unauthorized();
            }

            var binConfig = new BinnerBinConfig();
            if (!string.IsNullOrEmpty(tokenEntity.TokenConfig))
            {
                binConfig = JsonConvert.DeserializeObject<BinnerBinConfig>(tokenEntity.TokenConfig);
            }

            if (binConfig == null || binConfig.Location == string.Empty || binConfig.BinNumber == string.Empty)
            {
                return Unauthorized();
            }

            // check if we have anything to highligh
            if (highlights.Keys.Count == 0)
            {
                List<BinnerBinResponse> items = new();
                return Ok(new { highlight = items });
            }

            // wait until we have the mutex
            m.WaitOne();
            try {
                // filter out the parts this bin can highligh
                List<BinnerBinResponse> values = new List<BinnerBinResponse>();

                foreach (var location in highlights)
                {
                    if (location.Key.Location != binConfig.Location || location.Key.BinNumber != binConfig.BinNumber) {
                        continue;
                    }

                    var b = new BinnerBinResponse(
                        location.Key.BinNumber2 ?? string.Empty,
                        availableColors[location.Value.Color]
                    );
                    values.Add(b);
                }

                var result = new { highlight = values };
                return Ok(result);
            }
            finally {
                m.ReleaseMutex();
            }
        }

        /// <summary>
        /// Clears the expired highlights
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static void CleanupRefs() 
        {
            // wait until we have the mutex
            m.WaitOne();
            try
            {
                // get the current time
                DateTime time = DateTime.UtcNow;

                // search for expired references
                foreach (var r in highlights)
                {
                    if (r.Value.expired(time, 15))
                    {
                        highlights.Remove(r.Key);
                    }
                }
            }
            finally
            {
                m.ReleaseMutex();
            }
        }
    }
}
