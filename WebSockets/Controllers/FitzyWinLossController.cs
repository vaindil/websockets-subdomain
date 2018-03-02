using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using WebSockets.Classes;

namespace WebSockets.Controllers
{
    [Route("fitzy")]
    public class FitzyWinLossController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly string _apiSecret;

        public FitzyWinLossController(IMemoryCache cache, IOptions<FitzyConfig> options)
        {
            _cache = cache;
            _apiSecret = options.Value.ApiSecret;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var record = GetCurrentRecord();
            var result = $"Wins: {record.Wins} | Losses: {record.Losses}";

            if (record.Draws > 0)
                result += $" | Draws: {record.Draws}";

            return Ok(result);
        }

        [HttpPut("wins/{num}")]
        public IActionResult Wins(int num)
        {
            if (!CheckHeader())
                return Unauthorized();

            if (num > 99)
                num = 99;

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
            if (!CheckHeader())
                return Unauthorized();

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
            if (!CheckHeader())
                return Unauthorized();

            if (num > 99)
                num = 99;

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

        private (int Wins, int Losses, int Draws) GetCurrentRecord()
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

            return (wins, losses, draws);
        }

        private bool CheckHeader()
        {
            if (Request.Headers.TryGetValue("Authorization", out var header) && header.ToString() == _apiSecret)
                return true;

            return false;
        }
    }
}
