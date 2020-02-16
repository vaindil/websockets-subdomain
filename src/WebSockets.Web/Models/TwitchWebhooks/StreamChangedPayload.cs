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

        // this could be DateTimeOffset with no additional logic required,
        // but this payload is just passed on through so it doesn't need
        // to be parsed
        [JsonPropertyName("started_at")]
        public string StartedAt { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }
}
