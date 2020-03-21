using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WebSockets.Web.Models.Configs;

namespace WebSockets.Web.WebSockets.Middleware
{
    public class TwitchWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TwitchWebSocketManager _wsMgr;
        private readonly TwitchConfig _config;

        public TwitchWebSocketMiddleware(
            RequestDelegate next,
            TwitchWebSocketManager wsMgr,
            IOptions<TwitchConfig> options)
        {
            _next = next;
            _wsMgr = wsMgr;
            _config = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path != "/twitch/stream/ws")
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

            if (!context.Request.Query.TryGetValue("key", out var key) || key.ToString() != _config.SocketSecret)
            {
                context.Response.StatusCode = 401;
                return;
            }

            var guid = _wsMgr.AddWebSocket(await context.WebSockets.AcceptWebSocketAsync());

            await _wsMgr.ReceiveUntilClosedAsync(guid);
        }
    }
}
