using System;

namespace WebSockets.Data.ZubatRequests
{
    public class KillerRequest
    {
        public int Id { get; set; }

        public string KillerName { get; set; }

        public string RequestText { get; set; }

        public string RequestedByTwitchId { get; set; }

        public DateTime RequestedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
