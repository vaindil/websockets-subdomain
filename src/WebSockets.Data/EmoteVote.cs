using System;
using System.Net;

namespace WebSockets.Data
{
    public class EmoteVote
    {
        public int Id { get; set; }

        public IPAddress IpAddress { get; set; }

        public DateTime LoggedAt { get; set; }

        public string EmoteName { get; set; }

        public string VoteFor { get; set; }
    }
}
