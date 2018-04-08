namespace WebSockets.Data.Twitch
{
    public class TwitchMarker
    {
        public int Id { get; set; }

        public string StreamId { get; set; }

        public string QueryTime { get; set; }

        public string MarkedBy { get; set; }

        public string Reason { get; set; }

        public TwitchStream Stream { get; set; }
    }
}
