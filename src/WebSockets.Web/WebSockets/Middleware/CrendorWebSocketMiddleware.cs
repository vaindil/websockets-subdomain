using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using WebSockets.Web.Utils;

namespace WebSockets.Web.WebSockets.Middleware
{
    public class CrendorWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CrendorWebSocketManager _wsMgr;
        private readonly IMemoryCache _cache;

        public CrendorWebSocketMiddleware(
            RequestDelegate next,
            CrendorWebSocketManager wsMgr,
            IMemoryCache cache)
        {
            _next = next;
            _wsMgr = wsMgr;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path != "/crendor/ws")
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

            _cache.TryGetValue(CacheKeys.CrendorSubPoints, out int points);
            await _wsMgr.SendAsync(guid, points.ToString());

            await _wsMgr.ReceiveUntilClosedAsync(guid);
        }
    }
}
