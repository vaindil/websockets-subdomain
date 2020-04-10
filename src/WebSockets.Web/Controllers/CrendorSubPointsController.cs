using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using WebSockets.Data.Services;
using WebSockets.Web.Auth;
using WebSockets.Web.Utils;
using WebSockets.Web.WebSockets;

namespace WebSockets.Web.Controllers
{
    [Route("crendor")]
    public class CrendorSubPointsController : ControllerBase
    {
        private readonly CrendorWebSocketManager _wsMgr;
        private readonly IMemoryCache _cache;
        private readonly KeyValueService _kvSvc;

        public CrendorSubPointsController(CrendorWebSocketManager wsMgr, IMemoryCache cache, KeyValueService kvSvc)
        {
            _wsMgr = wsMgr;
            _cache = cache;
            _kvSvc = kvSvc;
        }

        [HttpGet("points")]
        public IActionResult Get()
        {
            _cache.TryGetValue(CacheKeys.CrendorSubPoints, out int points);
            return Ok(points);
        }

        /// <summary>
        /// Adds a given number of points to the current total.
        /// </summary>
        /// <param name="num">Number of points to add to the total</param>
        [HttpPost("points/{num}")]
        [MiddlewareFilter(typeof(CrendorHeaderAuthPipeline))]
        public async Task<IActionResult> AddPoints(int num)
        {
            if (!_cache.TryGetValue(CacheKeys.CrendorSubPoints, out int curPoints))
            {
                curPoints = 0;
            }

            num += curPoints;
            var numString = num.ToString();

            _cache.Set(CacheKeys.CrendorSubPoints, num, CacheHelpers.EntryOptions);
            await _wsMgr.SendAllAsync(numString);

            await _kvSvc.CreateOrUpdateAsync(CacheKeys.CrendorSubPoints, numString);

            return NoContent();
        }

        /// <summary>
        /// Sets the current total.
        /// </summary>
        /// <param name="num">Total to set</param>
        [HttpPut("points/{num}")]
        [MiddlewareFilter(typeof(CrendorHeaderAuthPipeline))]
        public async Task<IActionResult> SetPoints(int num)
        {
            var numString = num.ToString();

            _cache.Set(CacheKeys.CrendorSubPoints, num, CacheHelpers.EntryOptions);
            await _wsMgr.SendAllAsync(numString);

            await _kvSvc.CreateOrUpdateAsync(CacheKeys.CrendorSubPoints, numString);

            return NoContent();
        }
    }
}
