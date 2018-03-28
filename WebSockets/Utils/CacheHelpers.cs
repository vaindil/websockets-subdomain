using Microsoft.Extensions.Caching.Memory;

namespace WebSockets.Web.Utils
{
    public static class CacheHelpers
    {
        public static MemoryCacheEntryOptions GetEntryOptions()
        {
            return new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            };
        }
    }

    public static class CacheKeys
    {
        public const string FitzyWins = "FitzyWins";
        public const string FitzyLosses = "FitzyLosses";
        public const string FitzyDraws = "FitzyDraws";

        public const string TwitchStreamUpDown = "TwitchStreamUpDown";
        public const string TwitchStreamUpDownHasListeners = "TwitchStreamUpDownHasListeners";
    }
}
