using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using WebSockets.Data.Services;
using WebSockets.Web.Auth;
using WebSockets.Web.Utils;
using WebSockets.Web.WebSockets;

namespace WebSockets.Web.Controllers
{
    [Route("fitzy")]
    public class FitzyWinLossController : ControllerBase
    {
        private readonly FitzyWebSocketManager _wsMgr;
        private readonly IMemoryCache _cache;
        private readonly KeyValueService _kvSvc;

        public FitzyWinLossController(
            FitzyWebSocketManager wsMgr,
            IMemoryCache cache,
            KeyValueService kvSvc)
        {
            _wsMgr = wsMgr;
            _cache = cache;
            _kvSvc = kvSvc;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var record = _cache.GetCurrentFitzyRecord();
            var result = $"Wins: {record.Wins} | Losses: {record.Losses}";

            if (record.Draws > 0)
                result += $" | Draws: {record.Draws}";

            return Ok(result);
        }

        [HttpPut("wins/{num}")]
        [MiddlewareFilter(typeof(FitzyHeaderAuthPipeline))]
        public async Task<IActionResult> Wins(int num)
        {
            if (num > 99)
                num = 99;

            if (_cache.TryGetValue(CacheKeys.FitzyWins, out int wins))
            {
                num = num > -1 ? num : Math.Min(wins + 1, 99);
                _cache.Set(CacheKeys.FitzyWins, num);
            }
            else
            {
                num = num > -1 ? num : 1;
                _cache.Set(CacheKeys.FitzyWins, num);
            }

            await _wsMgr.SendAllCurrentRecordAsync();
            await _kvSvc.CreateOrUpdateAsync(CacheKeys.FitzyWins, num.ToString());

            return NoContent();
        }

        [HttpPut("losses/{num}")]
        [MiddlewareFilter(typeof(FitzyHeaderAuthPipeline))]
        public async Task<IActionResult> Losses(int num)
        {
            if (num > 99)
                num = 99;

            if (_cache.TryGetValue(CacheKeys.FitzyLosses, out int losses))
            {
                num = num > -1 ? num : Math.Min(losses + 1, 99);
                _cache.Set(CacheKeys.FitzyLosses, num);
            }
            else
            {
                num = num > -1 ? num : 1;
                _cache.Set(CacheKeys.FitzyLosses, num);
            }

            await _wsMgr.SendAllCurrentRecordAsync();
            await _kvSvc.CreateOrUpdateAsync(CacheKeys.FitzyLosses, num.ToString());

            return NoContent();
        }

        [HttpPut("draws/{num}")]
        [MiddlewareFilter(typeof(FitzyHeaderAuthPipeline))]
        public async Task<IActionResult> Draws(int num)
        {
            if (num > 99)
                num = 99;

            if (_cache.TryGetValue(CacheKeys.FitzyDraws, out int draws))
            {
                num = num > -1 ? num : Math.Min(draws + 1, 99);
                _cache.Set(CacheKeys.FitzyDraws, num);
            }
            else
            {
                num = num > -1 ? num : 1;
                _cache.Set(CacheKeys.FitzyDraws, num);
            }

            await _wsMgr.SendAllCurrentRecordAsync();
            await _kvSvc.CreateOrUpdateAsync(CacheKeys.FitzyDraws, num.ToString());

            return NoContent();
        }

        [HttpPut("clear")]
        [MiddlewareFilter(typeof(FitzyHeaderAuthPipeline))]
        public async Task<IActionResult> Clear()
        {
            _cache.Set(CacheKeys.FitzyWins, 0);
            _cache.Set(CacheKeys.FitzyLosses, 0);
            _cache.Set(CacheKeys.FitzyDraws, 0);

            await _wsMgr.SendAllCurrentRecordAsync();

            await _kvSvc.CreateOrUpdateAsync(CacheKeys.FitzyWins, "0");
            await _kvSvc.CreateOrUpdateAsync(CacheKeys.FitzyLosses, "0");
            await _kvSvc.CreateOrUpdateAsync(CacheKeys.FitzyDraws, "0");

            return NoContent();
        }

        [HttpPost("refresh")]
        [MiddlewareFilter(typeof(FitzyHeaderAuthPipeline))]
        public async Task<IActionResult> Refresh()
        {
            await _wsMgr.SendAllAsync("REFRESH");

            return NoContent();
        }
    }
}
