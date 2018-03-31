using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using WebSockets.Data.Services;
using WebSockets.Web.Models.Configs;
using WebSockets.Web.Utils;

namespace WebSockets.Web.Controllers
{
    [Route("fitzy")]
    public class FitzyWinLossController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly KeyValueService _kvSvc;
        private readonly string _apiSecret;

        public FitzyWinLossController(IMemoryCache cache, KeyValueService kvSvc, IOptions<FitzyConfig> options)
        {
            _cache = cache;
            _kvSvc = kvSvc;
            _apiSecret = options.Value.ApiSecret;
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
        public async Task<IActionResult> Wins(int num)
        {
            if (!CheckHeader())
                return Unauthorized();

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

            SetUpdateNeeded();
            await _kvSvc.CreateOrUpdateAsync(CacheKeys.FitzyWins, num.ToString());

            return NoContent();
        }

        [HttpPut("losses/{num}")]
        public async Task<IActionResult> Losses(int num)
        {
            if (!CheckHeader())
                return Unauthorized();

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

            SetUpdateNeeded();
            await _kvSvc.CreateOrUpdateAsync(CacheKeys.FitzyLosses, num.ToString());

            return NoContent();
        }

        [HttpPut("draws/{num}")]
        public async Task<IActionResult> Draws(int num)
        {
            if (!CheckHeader())
                return Unauthorized();

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

            SetUpdateNeeded();
            await _kvSvc.CreateOrUpdateAsync(CacheKeys.FitzyDraws, num.ToString());

            return NoContent();
        }

        private void SetUpdateNeeded()
        {
            _cache.Set(CacheKeys.FitzyRecordUpdateNeeded, true, CacheHelpers.GetEntryOptions());
        }

        private bool CheckHeader()
        {
            return Request.Headers.TryGetValue("Authorization", out var header) && header.ToString() == _apiSecret;
        }
    }
}
