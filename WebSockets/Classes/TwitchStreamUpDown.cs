﻿using WebSockets.Utils;

namespace WebSockets.Classes
{
    public class TwitchStreamUpDown
    {
        public TwitchStreamUpDown(string userId, StreamStatus status)
        {
            UserId = userId;
            Status = status;
        }

        public string UserId { get; set; }

        public StreamStatus Status { get; set; }
    }
}
