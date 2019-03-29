using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSockets.Web.Models.Configs;
using WebSockets.Web.Models.TwitchWebhooks;
using WebSockets.Web.Utils;
using WebSockets.Web.WebSockets;

namespace WebSockets.Web.Controllers
{
    [Route("twitch_webhooks")]
    public class TwitchWebhooksController : ControllerBase
    {
        private readonly List<string> _notificationIds;
        private readonly TwitchActionsWebSocketManager _fitzyWsMgr;
        private readonly TwitchConfig _twitchConfig;

        private readonly ILogger _logger;

        public TwitchWebhooksController(
            IMemoryCache cache,
            TwitchActionsWebSocketManager fitzyWsMgr,
            IOptions<TwitchConfig> twitchOptions,
            ILogger<TwitchWebhooksController> logger)
        {
            _notificationIds = cache.Get<List<string>>(CacheKeys.TwitchNotificationIds);
            _fitzyWsMgr = fitzyWsMgr;
            _twitchConfig = twitchOptions.Value;
            _logger = logger;
        }

        [HttpGet("{channel}")]
        public IActionResult VerifyWebhook(string channel)
        {
            _logger.LogInformation($"Twitch webhook verification received for {channel}");

            Request.Query.TryGetValue("hub.challenge", out var challenge);

            return Ok(challenge.ToString());
        }

        [HttpPost("{channel}")]
        public async Task<IActionResult> HandleWebhook(string channel)
        {
            _logger.LogInformation($"Received Twitch webhook for {channel}");

            var bytes = await Request.GetBodyAsBytesAsync();
            if (!Request.Headers.TryGetValue("X-Hub-Signature", out var signature))
            {
                _logger.LogWarning("No X-Hub-Signature header found");
                return BadRequest();
            }

            if (!TwitchSignatureVerifier.Verify(_twitchConfig.WebhookSecret, signature.ToString(), bytes))
            {
                _logger.LogWarning("X-Hub-Signature is invalid");
                return BadRequest();
            }

            Request.Headers.TryGetValue("Twitch-Notification-Id", out var notificationId);
            _logger.LogInformation($"The notification's ID is: {notificationId}");
            if (_notificationIds.Contains(notificationId))
            {
                _logger.LogInformation("This notification ID is already being tracked, webhook ignored.");
                return NoContent();
            }

            _notificationIds.Add(notificationId);

            var bodyString = Encoding.UTF8.GetString(bytes);
            var payload = JsonConvert.DeserializeObject<StreamUpDownPayload>(bodyString);

            var message = "LIVE";

            if (payload.Data.Count == 0)
                message = "OFFLINE";

            _logger.LogInformation($"Stream status changed: {channel} is now {message}.");

            if (string.Equals(channel, "fitzyhere", StringComparison.OrdinalIgnoreCase))
            {
                await _fitzyWsMgr.SendAllAsync(message);
            }

            return NoContent();
        }
    }
}
