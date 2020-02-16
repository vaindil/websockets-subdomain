using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebSockets.Web.Models.TwitchWebhooks
{
    public class StreamChangedNotificationMessage : StreamChangedData
    {
        public StreamChangedNotificationMessage(string username, string userId, List<StreamChangedData> dataList)
        {
            if (dataList.Count == 0)
            {
                Status = "offline";
                Username = username;
                UserId = userId;
            }
            else
            {
                var data = dataList[0];

                Status = "live";
                Id = data.Id;
                UserId = data.UserId;
                Username = data.Username;
                GameId = data.GameId;
                Title = data.Title;
                ViewerCount = data.ViewerCount;
                StartedAt = data.StartedAt;
                ThumbnailUrl = data.ThumbnailUrl;
            }
        }

        // either "live" or "offline"
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
