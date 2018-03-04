using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebSockets.Classes;
using WebSockets.Utils;

namespace WebSockets.Controllers
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
        public IActionResult Verify()
        {
            var challenge = Request.Query?.FirstOrDefault(q => q.Key == "hub.challenge");
            if (!challenge.HasValue || challenge.Value.Key == null)
                return BadRequest();

            return Ok(challenge.Value.Value);
        }

        [HttpPost("stream/{twitchId}")]
        public async Task<IActionResult> StreamUpDown(string twitchId)
        {
            if (!Request.Headers.Keys.Contains("X-Hub-Signature"))
                return BadRequest();

            if (!UpDownHasListeners())
                return NoContent();

            var signature = Request.Headers.First(h => h.Key == "X-Hub-Signature").Value.ToString();

            using (var alg = new HMACSHA256(Encoding.UTF8.GetBytes(_hubSecret)))
            {
                var hashBytes = alg.ComputeHash(await Request.GetBodyAsBytesAsync());
                if (Encoding.UTF8.GetString(hashBytes) != signature)
                    return BadRequest();
            }

            StreamStatus status;
            var body = await Request.GetBodyAsStringAsync();

            if (body.Contains("thumbnail_url"))
                status = StreamStatus.Up;
            else
                status = StreamStatus.Down;

            var queue = _cache.Get<ConcurrentQueue<TwitchStreamUpDown>>(CacheKeys.TwitchStreamUpDown);
            queue.Enqueue(new TwitchStreamUpDown(twitchId, status));

            Console.WriteLine($"QUEUE: {twitchId} {status}");

            return NoContent();
        }

        private bool UpDownHasListeners()
        {
            return _cache.Get<bool>(CacheKeys.TwitchStreamUpDownHasListeners);
        }
    }
}
