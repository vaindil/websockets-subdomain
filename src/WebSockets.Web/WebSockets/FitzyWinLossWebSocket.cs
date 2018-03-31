using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSockets.Web.Utils;

namespace WebSockets.Web.WebSockets
{
    public class FitzyWinLossWebSocket
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new ConcurrentDictionary<Guid, WebSocket>();

        public FitzyWinLossWebSocket(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path != "/fitzy/ws" || !context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var guid = Guid.NewGuid();
            _sockets.TryAdd(guid, webSocket);

            await SendMessageAsync(webSocket, GetCurrentValues());

            while (_sockets.Count > 0)
            {
                if (webSocket.State != WebSocketState.Open)
                    break;

                if (_cache.TryGetValue(CacheKeys.FitzyRecordUpdateNeeded, out _))
                {
                    await MessageAllAsync(GetCurrentValues());
                    _cache.Remove(CacheKeys.FitzyRecordUpdateNeeded);
                }

                await Task.Delay(2000);
            }

            _sockets.TryRemove(guid, out _);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            webSocket.Dispose();
        }

        private async Task SendMessageAsync(WebSocket socket, string data)
        {
            await socket.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(data)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        private async Task MessageAllAsync(string data)
        {
            foreach (var socket in _sockets)
            {
                if (socket.Value.State == WebSocketState.Open)
                    await SendMessageAsync(socket.Value, data);
            }
        }

        private string GetCurrentValues()
        {
            var (wins, losses, draws) = _cache.GetCurrentFitzyRecord();
            return $"{wins} {losses} {draws}";
        }
    }
}
