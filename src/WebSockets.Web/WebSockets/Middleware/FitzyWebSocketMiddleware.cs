using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using WebSockets.Web.Utils;

namespace WebSockets.Web.WebSockets.Middleware
{
    public class FitzyWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FitzyWebSocketManager _wsMgr;
        private readonly IMemoryCache _cache;

        public FitzyWebSocketMiddleware(
            RequestDelegate next,
            FitzyWebSocketManager wsMgr,
            IMemoryCache cache)
        {
            _next = next;
            _wsMgr = wsMgr;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path != "/fitzy/ws")
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
            var (wins, losses, draws) = _cache.GetCurrentFitzyRecord();

            await _wsMgr.SendAsync(guid, $"{wins} {losses} {draws}");

            await _wsMgr.ReceiveUntilClosedAsync(guid);
        }
    }
}
