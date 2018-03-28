using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using WebSockets.Core;
using WebSockets.Web.Configs;
using WebSockets.Web.Utils;

namespace WebSockets.Web.Controllers
{
    [Route("twitch")]
    public class TwitchWebhooksController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly string _hubSecret;

        public TwitchWebhooksController(IMemoryCache cache, IOptions<TwitchConfig> options)
        {
            _cache = cache;
            _hubSecret = options.Value.HubSecret;
        }

        [HttpGet("stream/{twitchId}")]
        public IActionResult Verify(SubscriptionResponse response)
        {
            if (string.IsNullOrEmpty(response?.Challenge))
                return BadRequest();

            Console.WriteLine($"CHALLENGE RECEIVED: {response.Challenge}");
            Console.Beep();

            return Ok(response.Challenge);
        }

        [HttpPost("stream/{twitchId}")]
        public async Task<IActionResult> StreamUpDown(string twitchId)
        {
            if (!Request.Headers.Keys.Contains("X-Hub-Signature"))
                return BadRequest();

            //if (!UpDownHasListeners())
            //    return NoContent();

            var signature = Request.Headers.First(h => h.Key == "X-Hub-Signature").Value.ToString();
            var sigSplit = signature.Split('=');
            if (sigSplit.Length > 1)
                signature = sigSplit[1];

            if (!TwitchSignatureVerifier.Verify(_hubSecret, signature, await Request.GetBodyAsBytesAsync()))
                return BadRequest();

            StreamStatus status;
            var body = await Request.GetBodyAsStringAsync();

            if (body.Contains("thumbnail_url"))
                status = StreamStatus.Up;
            else
                status = StreamStatus.Down;

            Console.WriteLine($"STREAM RECEIVED: {twitchId} : {status}");
            Console.Beep();
            Console.Beep();

            var queue = _cache.Get<ConcurrentQueue<TwitchStreamUpDown>>(CacheKeys.TwitchStreamUpDown);
            queue.Enqueue(new TwitchStreamUpDown(twitchId, status));

            return NoContent();
        }

        private bool UpDownHasListeners()
        {
            return _cache.Get<bool>(CacheKeys.TwitchStreamUpDownHasListeners);
        }

        public class SubscriptionResponse
        {
            [BindProperty(Name = "hub.mode")]
            public string Mode { get; set; }

            [BindProperty(Name = "hub.topic")]
            public string Topic { get; set; }

            [BindProperty(Name = "hub.lease_seconds")]
            public int LeaseSeconds { get; set; }

            [BindProperty(Name = "hub.challenge")]
            public string Challenge { get; set; }

            [BindProperty(Name = "hub.reason")]
            public string Reason { get; set; }
        }
    }
}
