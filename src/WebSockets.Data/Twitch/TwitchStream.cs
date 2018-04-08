using System.Collections.Generic;

namespace WebSockets.Data.Twitch
{
    public class TwitchStream
    {
        public string Id { get; set; }

        public string ChannelId { get; set; }

        public string ChannelName { get; set; }

        public string VodId { get; set; }

        public ICollection<TwitchMarker> Markers { get; set; }
    }
}
