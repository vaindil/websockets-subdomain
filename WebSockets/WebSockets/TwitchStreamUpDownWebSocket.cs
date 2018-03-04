using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSockets.Classes;
using WebSockets.Utils;

namespace WebSockets.WebSockets
{
    public class TwitchStreamUpDownWebSocket
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new ConcurrentDictionary<Guid, WebSocket>();

        public TwitchStreamUpDownWebSocket(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path != "/twitch/stream/ws" || !context.WebSockets.IsWebSocketRequest) {
                await _next(context);
                return;
            }

            var queue = _cache.Get<ConcurrentQueue<TwitchStreamUpDown>>(CacheKeys.TwitchStreamUpDown);

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var guid = Guid.NewGuid();
            _sockets.TryAdd(guid, webSocket);

            _cache.Set(CacheKeys.TwitchStreamUpDownHasListeners, true, CacheHelpers.GetEntryOptions());

            while (_sockets.Count > 0)
            {
                if (webSocket.State != WebSocketState.Open)
                    break;

                while (!queue.IsEmpty)
                {
                    if (queue.TryDequeue(out var stream))
                        await MessageAllAsync($"{stream.UserId} {stream.Status.ToString()}");
                }

                await Task.Delay(2000);
            }

            _cache.Set(CacheKeys.TwitchStreamUpDownHasListeners, false);

            _sockets.TryRemove(guid, out _);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal", CancellationToken.None);
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
    }
}
