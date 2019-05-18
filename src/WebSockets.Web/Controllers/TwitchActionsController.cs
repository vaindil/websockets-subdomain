using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;
using WebSockets.Web.Auth;
using WebSockets.Web.Models;
using WebSockets.Web.WebSockets;

namespace WebSockets.Web.Controllers
{
    [Route("twitch_actions")]
    public class TwitchActionsController : ControllerBase
    {
        private readonly TwitchActionsWebSocketManager _wsMgr;
        private readonly ILogger _logger;

        public TwitchActionsController(TwitchActionsWebSocketManager wsMgr, ILogger<TwitchActionsController> logger)
        {
            _wsMgr = wsMgr;
            _logger = logger;
        }

        [HttpPut("live")]
        [MiddlewareFilter(typeof(FitzyHeaderAuthPipeline))]
        public async Task<IActionResult> StreamLive()
        {
            _logger.LogInformation("Fitzy stream live received");
            await _wsMgr.SendAllAsync("LIVE");

            return NoContent();
        }

        [HttpPut("offline")]
        [MiddlewareFilter(typeof(FitzyHeaderAuthPipeline))]
        public async Task<IActionResult> StreamOffline()
        {
            _logger.LogInformation("Fitzy stream offline received");
            await _wsMgr.SendAllAsync("OFFLINE");

            return NoContent();
        }

        [HttpPost]
        [MiddlewareFilter(typeof(FitzyHeaderAuthPipeline))]
        public async Task<IActionResult> AddReport([FromBody]TwitchActionModel model)
        {
            if (model == null)
            {
                _logger.LogInformation("Twitch action request received: no body");
                return BadRequest("request was empty");
            }

            if (string.IsNullOrEmpty(model.ModUsername))
            {
                _logger.LogInformation("Twitch action request received: no mod username");
                return BadRequest("mod username was empty");
            }

            if (string.IsNullOrEmpty(model.UserUsername))
            {
                _logger.LogInformation("Twitch action request received: no user username");
                return BadRequest("user username was empty");
            }

            if (string.IsNullOrEmpty(model.Action))
            {
                _logger.LogInformation("Twitch action request received: no action");
                return BadRequest("action was empty");
            }

            // FORMAT:
            // ACTION vaindil KeithyDee timeout 600 no reason
            var sb = new StringBuilder("ACTION ");

            sb.Append(model.ModUsername);
            sb.Append(' ');
            sb.Append(model.UserUsername);
            sb.Append(' ');
            sb.Append(model.Action);
            sb.Append(' ');
            sb.Append(model.Duration);
            sb.Append(' ');
            sb.Append(model.Reason);

            var final = sb.ToString();

            await _wsMgr.SendAllAsync(final);

            _logger.LogInformation($"Twitch action request received: {final}");

            return NoContent();
        }
    }
}
