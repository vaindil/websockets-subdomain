using Microsoft.AspNetCore.Mvc;
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

        public TwitchActionsController(TwitchActionsWebSocketManager wsMgr)
        {
            _wsMgr = wsMgr;
        }

        [HttpPut("live")]
        public async Task<IActionResult> StreamLive()
        {
            await _wsMgr.SendAllAsync("LIVE");

            return NoContent();
        }

        [HttpPut("offline")]
        public async Task<IActionResult> StreamOffline()
        {
            await _wsMgr.SendAllAsync("OFFLINE");

            return NoContent();
        }

        [HttpPost]
        [MiddlewareFilter(typeof(FitzyHeaderAuthPipeline))]
        public async Task<IActionResult> AddReport([FromBody]TwitchActionModel model)
        {
            if (model == null)
                return BadRequest("request was empty");

            if (string.IsNullOrEmpty(model.ModUsername))
                return BadRequest("mod username was empty");

            if (string.IsNullOrEmpty(model.UserUsername))
                return BadRequest("user username was empty");

            if (string.IsNullOrEmpty(model.Action))
                return BadRequest("action was empty");

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

            await _wsMgr.SendAllAsync(sb.ToString());

            return NoContent();
        }
    }
}
