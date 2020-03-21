using System;

namespace WebSockets.Data
{
    public class TwitchWebhookNotification
    {
        // ID of the notification as provided in the header, not the ID of the stream
        // that's provided when a stream goes live
        public string Id { get; set; }

        public DateTimeOffset ReceivedAt { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }

        public string GameId { get; set; }

        public string Title { get; set; }

        public DateTimeOffset? StartedAt { get; set; }
    }
}
