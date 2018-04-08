using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSockets.Data.Services;

namespace WebSockets.Web.Controllers
{
    [Route("twitch")]
    public class TwitchMarkerController : Controller
    {
        private readonly TwitchService _twitchSvc;

        public TwitchMarkerController(TwitchService twitchSvc)
        {
            _twitchSvc = twitchSvc;
        }

        [HttpGet]
        public async Task<IActionResult> GetStream(string streamId = null, string vodId = null)
        {
            return Ok();
        }
    }
}
