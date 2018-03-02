using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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
            var challenge = Request.Query?.FirstOrDefault(q => q.Key == "challenge");
            if (challenge == null)
                return BadRequest();

            return Ok(challenge);
        }

        [HttpPost("stream/{twitchId}")]
        public async Task<IActionResult> StreamUpDown(string twitchId)
        {
            if (!Request.Headers.Keys.Contains("X-Hub-Signature"))
                return BadRequest();

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

            _cache.Set($"twitch-{twitchId}", status);

            return NoContent();
        }
    }
}
