using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WebSockets.Web.Models.TwitchWebhooks
{
    public class StreamChangedPayload
    {
        [JsonProperty(PropertyName = "data")]
        public List<StreamChangedData> Data { get; set; }
    }

    public class StreamChangedData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "user_name")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "game_id")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "viewer_count")]
        public int? ViewerCount { get; set; }

        // this could be DateTimeOffset with no additional logic required,
        // but this payload is just passed on through so it doesn't need
        // to be parsed
        [JsonProperty(PropertyName = "started_at")]
        public string StartedAt { get; set; }

        [JsonProperty(PropertyName = "thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }
}
