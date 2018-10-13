using Microsoft.Extensions.Caching.Memory;

namespace WebSockets.Web.Utils
{
    public static class CacheHelpers
    {
        public static MemoryCacheEntryOptions EntryOptions
        {
            get
            {
                return new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                };
            }
        }

        public static (int Wins, int Losses, int Draws) GetCurrentFitzyRecord(this IMemoryCache cache)
        {
            cache.TryGetValue(CacheKeys.FitzyWins, out int wins);
            cache.TryGetValue(CacheKeys.FitzyLosses, out int losses);
            cache.TryGetValue(CacheKeys.FitzyDraws, out int draws);

            return (wins, losses, draws);
        }
    }

    public static class CacheKeys
    {
        public const string FitzyWins = "FitzyWins";
        public const string FitzyLosses = "FitzyLosses";
        public const string FitzyDraws = "FitzyDraws";

        public const string TwitchStreamUpDown = "TwitchStreamUpDown";
        public const string TwitchStreamUpDownHasListeners = "TwitchStreamUpDownHasListeners";

        public const string ZubatSecondsRemaining = "ZubatSecondsRemaining";
    }
}
