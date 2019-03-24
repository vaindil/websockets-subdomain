using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebSockets.Web.Models.TwitchWebhooks
{
    public class StreamUpDownPayload
    {
        [JsonProperty(PropertyName = "data")]
        public List<StreamUpDownData> Data { get; set; }
    }

    public class StreamUpDownData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "login")]
        public string Login { get; set; }

        [JsonProperty(PropertyName = "display_name")]
        public string DisplayName { get; set; }
    }
}
