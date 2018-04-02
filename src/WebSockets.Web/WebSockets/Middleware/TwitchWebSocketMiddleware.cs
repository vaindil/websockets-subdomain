using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebSockets.Web.WebSockets.Middleware
{
    public class TwitchWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TwitchWebSocketManager _wsMgr;

        public TwitchWebSocketMiddleware(
            RequestDelegate next,
            TwitchWebSocketManager wsMgr)
        {
            _next = next;
            _wsMgr = wsMgr;
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

            var guid = _wsMgr.AddWebSocket(await context.WebSockets.AcceptWebSocketAsync());

            await _wsMgr.ReceiveUntilClosedAsync(guid);
        }
    }
}
