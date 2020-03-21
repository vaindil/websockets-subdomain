using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebSockets.Data;
using WebSockets.Web.Models.Configs;
using WebSockets.Web.Models.TwitchWebhooks;
using WebSockets.Web.Utils;
using WebSockets.Web.WebSockets;

namespace WebSockets.Web.Controllers
{
    [Route("twitch_webhooks")]
    public class TwitchWebhooksController : ControllerBase
    {
        private readonly TwitchWebSocketManager _wsMgr;
        private readonly TwitchActionsWebSocketManager _fitzyWsMgr;
        private readonly TwitchConfig _twitchConfig;
        private readonly VbContext _context;

        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public TwitchWebhooksController(
            TwitchWebSocketManager wsMgr,
            TwitchActionsWebSocketManager fitzyWsMgr,
            IOptions<TwitchConfig> twitchOptions,
            VbContext context,
            IMemoryCache cache,
            ILogger<TwitchWebhooksController> logger)
        {
            _wsMgr = wsMgr;
            _fitzyWsMgr = fitzyWsMgr;
            _twitchConfig = twitchOptions.Value;
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult VerifyWebhook([FromQuery]string username, [FromQuery]string userId)
        {
            _logger.LogInformation($"Twitch webhook verification received for {username} ({userId})");

            Request.Query.TryGetValue("hub.challenge", out var challenge);

            return Ok(challenge.ToString());
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromQuery]string username, [FromQuery]string userId)
        {
            _logger.LogInformation($"Received Twitch webhook for {username}");

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
            if (_cache.TryGetValue(notificationId, out _))
            {
                _logger.LogInformation("Notification ID already being tracked, webhook ignored.");
                return NoContent();
            }

            _cache.Set(notificationId, "", TimeSpan.FromDays(3));

            var bodyString = Encoding.UTF8.GetString(bytes);
            var payload = JsonSerializer.Deserialize<StreamChangedPayload>(bodyString);

            var notificationMsg = new StreamChangedNotificationMessage(username, userId, payload.Data);
            var msgBody = JsonSerializer.Serialize(notificationMsg);

            _logger.LogInformation($"Stream status changed for channel {username}");

            await _wsMgr.SendAllAsync(msgBody);

            if (string.Equals(username, "fitzyhere", StringComparison.OrdinalIgnoreCase))
            {
                await _fitzyWsMgr.SendAllAsync(msgBody);
            }

            _context.TwitchWebhookNotifications.Add(new TwitchWebhookNotification
            {
                Id = notificationId,
                ReceivedAt = DateTimeOffset.UtcNow,
                UserId = userId,
                Username = username,
                GameId = payload.Data?[0]?.GameId,
                Title = payload.Data?[0]?.Title,
                StartedAt = payload.Data?[0]?.StartedAt
            });

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
