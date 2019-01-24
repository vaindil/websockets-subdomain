using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSockets.Web.Models;
using WebSockets.Web.Models.Configs;

namespace WebSockets.Web.WebSockets.Middleware
{
    public class ZubatWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ZubatWebSocketManager _wsMgr;
        private readonly ZubatConfig _config;

        public ZubatWebSocketMiddleware(
            RequestDelegate next,
            ZubatWebSocketManager wsMgr,
            IOptions<ZubatConfig> options)
        {
            _next = next;
            _wsMgr = wsMgr;
            _config = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path != "/zubat/requests/ws")
            {
                await _next(context);
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("This endpoint accepts only websocket connections.");
                return;
            }

            if (!context.Request.Query.TryGetValue("jwt", out var jwt))
            {
                context.Response.StatusCode = 401;
                return;
            }

            try
            {
                var jwtKey = Encoding.UTF8.GetBytes(_config.JwtSigningKey);
                var decoded = Jose.JWT.Decode<ZubatJwt>(jwt, jwtKey, Jose.JwsAlgorithm.HS256);

                if (!_config.RequestAdminTwitchIds.Contains(decoded.Twitch_id))
                    throw new Exception();
            }
            catch
            {
                context.Response.StatusCode = 401;
                return;
            }

            var guid = _wsMgr.AddWebSocket(await context.WebSockets.AcceptWebSocketAsync());

            await _wsMgr.ReceiveUntilClosedAsync(guid);
        }
    }
}
