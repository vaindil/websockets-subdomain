﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebSockets.Web.Models.TwitchWebhooks
{
    public class StreamChangedPayload
    {
        [JsonPropertyName("data")]
        public List<StreamChangedData> Data { get; set; }
    }

    public class StreamChangedData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_name")]
        public string Username { get; set; }

        [JsonPropertyName("game_id")]
        public string GameId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("viewer_count")]
        public int? ViewerCount { get; set; }

        [JsonPropertyName("started_at")]
        public DateTimeOffset StartedAt { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }
}
