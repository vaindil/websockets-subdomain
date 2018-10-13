using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WebSockets.Web.Models.Configs;
using WebSockets.Web.Utils;

namespace WebSockets.Web.WebSockets.Middleware
{
    public class ZubatWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ZubatWebSocketManager _wsMgr;
        private readonly IMemoryCache _cache;

        public ZubatWebSocketMiddleware(
            RequestDelegate next,
            ZubatWebSocketManager wsMgr,
            IMemoryCache cache)
        {
            _next = next;
            _wsMgr = wsMgr;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path != "/zubat/ws")
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

            _cache.TryGetValue(CacheKeys.ZubatSecondsRemaining, out var curSeconds);
            await _wsMgr.SendAsync(guid, curSeconds.ToString());

            await _wsMgr.ReceiveUntilClosedAsync(guid);
        }
    }
}
