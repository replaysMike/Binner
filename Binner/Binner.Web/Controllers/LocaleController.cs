using Binner.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Binner.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class LocaleController : ControllerBase
    {
        private const int FlushQueueMs = 3000;
        internal static Lazy<ILogger<LocaleController>> Logger = new (() => ApplicationLogging.LoggerFactory.CreateLogger<LocaleController>());
        private static readonly Lazy<Timer> Timer = new (() =>
        {
            var timer = new Timer(FlushQueueMs);
            timer.Elapsed += Timer_Elapsed;
            return timer;
        });

        private static ConcurrentQueue<LogEntry> Queue = new ConcurrentQueue<LogEntry>();

        public LocaleController()
        {
            Timer.Value.Start();
        }

        /// <summary>
        /// Store a missing language entry
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpPost("missing")]
        public async Task<IActionResult> AddMissingLangEntryAsync([FromQuery] string? language)
        {
            // {"page.settings.description": "Value in english"}
            if (string.IsNullOrEmpty(language))
                return BadRequest();
            var referer = Request.Headers.Referer;

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();
            var result = JObject.Parse(json);

            foreach (KeyValuePair<string, JToken> element in result)
            {
                var key = element.Key;
                var value = element.Value.ToString();
                Queue.Enqueue(new LogEntry
                {
                    Page = referer,
                    Language = language,
                    Key = key,
                    Value = value
                });
            }
            return Ok();
        }

        /// <summary>
        /// Flush the log queue and group logs by language and referring page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var entries = new List<LogEntry>();
            if(Queue.Count == 0)
                return;

            while (Queue.TryDequeue(out var entry))
            {
                entries.Add(entry);
            }

            // group by page and language, order by language
            var grouping = entries.GroupBy(x => new { x.Page, x.Language });
            foreach (var group in grouping.OrderBy(x => x.Key.Language))
            {
                var page = !string.IsNullOrEmpty(group.Key.Page) ? new Uri(group.Key.Page).LocalPath : string.Empty;
                Logger.Value.LogWarning($"Missing Translations | {group.Key.Language} | {page}");
                // sort by key
                foreach(var entry in group.OrderBy(x => x.Key))
                    Logger.Value.LogWarning($"[{entry.Language}] {entry.Key,-50} \"{entry.Value}\"");
            }
        }

        public class LogEntry
        {
            public string? Page { get; set; }
            public string? Language { get; set; }
            public string? Key { get; set; }
            public string? Value { get; set; }
        }
    }
}
