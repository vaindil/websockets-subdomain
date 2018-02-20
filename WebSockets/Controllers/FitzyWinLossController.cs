using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace WebSockets.Controllers
{
    [Route("fitzy")]
    public class FitzyWinLossController : Controller
    {
        private readonly IMemoryCache _cache;

        public FitzyWinLossController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(GetCurrentRatio());
        }

        [HttpPut("wins/{num}")]
        public IActionResult Wins(int num)
        {
            if (_cache.TryGetValue(CacheKeys.FitzyWins, out int wins))
            {
                _cache.Set(CacheKeys.FitzyWins, num > -1 ? num : Math.Min(wins + 1, 99));
            }
            else
            {
                _cache.Set(CacheKeys.FitzyWins, num > -1 ? num : 1);
            }

            return NoContent();
        }

        [HttpPut("losses/{num}")]
        public IActionResult Losses(int num)
        {
            if (num > 99)
                num = 99;

            if (_cache.TryGetValue(CacheKeys.FitzyLosses, out int losses))
            {
                _cache.Set(CacheKeys.FitzyLosses, num > -1 ? num : Math.Min(losses + 1, 99));
            }
            else
            {
                _cache.Set(CacheKeys.FitzyLosses, num > -1 ? num : 1);
            }

            return NoContent();
        }

        [HttpPut("draws/{num}")]
        public IActionResult Draws(int num)
        {
            if (_cache.TryGetValue(CacheKeys.FitzyDraws, out int draws))
            {
                _cache.Set(CacheKeys.FitzyDraws, num > -1 ? num : Math.Min(draws + 1, 99));
            }
            else
            {
                _cache.Set(CacheKeys.FitzyDraws, num > -1 ? num : 1);
            }

            return NoContent();
        }

        private string GetCurrentRatio()
        {
            var wins = _cache.GetOrCreate(CacheKeys.FitzyWins, entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                return 0;
            });

            var losses = _cache.GetOrCreate(CacheKeys.FitzyLosses, entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                return 0;
            });

            var draws = _cache.GetOrCreate(CacheKeys.FitzyDraws, entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                return 0;
            });

            return $"WINS: {wins} | LOSSES: {losses} | DRAWS: {draws}";
        }
    }
}
