using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSockets
{
    public class FitzyWinLossMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new ConcurrentDictionary<Guid, WebSocket>();

        private int _winCount;
        private int _lossCount;
        private int _drawCount;

        public FitzyWinLossMiddleware(RequestDelegate next, IMemoryCache cache)
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

            if (_sockets.Count == 0)
            {
                _winCount = _cache.GetOrCreate(CacheKeys.FitzyWins, entry =>
                {
                    entry.Priority = CacheItemPriority.NeverRemove;
                    return 0;
                });

                _lossCount = _cache.GetOrCreate(CacheKeys.FitzyLosses, entry =>
                {
                    entry.Priority = CacheItemPriority.NeverRemove;
                    return 0;
                });

                _drawCount = _cache.GetOrCreate(CacheKeys.FitzyDraws, entry =>
                {
                    entry.Priority = CacheItemPriority.NeverRemove;
                    return 0;
                });
            }

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var guid = Guid.NewGuid();
            _sockets.TryAdd(guid, webSocket);

            await SendMessageAsync(webSocket, GetCurrentValues());

            while (_sockets.Count > 0)
            {
                if (webSocket.State != WebSocketState.Open)
                    break;

                var wins = _cache.Get<int>(CacheKeys.FitzyWins);
                if (_winCount != wins)
                {
                    _winCount = wins;
                    await MessageAllAsync(GetCurrentValues());
                }

                var losses = _cache.Get<int>(CacheKeys.FitzyLosses);
                if (_lossCount != losses)
                {
                    _lossCount = losses;
                    await MessageAllAsync(GetCurrentValues());
                }

                var draws = _cache.Get<int>(CacheKeys.FitzyDraws);
                if (_drawCount != draws)
                {
                    _drawCount = draws;
                    await MessageAllAsync(GetCurrentValues());
                }

                await Task.Delay(2000);
            }

            _sockets.TryRemove(guid, out var dummy);
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
            var wins = _cache.GetOrCreate(CacheKeys.FitzyWins, entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                return 0;
            });

            var losses = _cache.GetOrCreate(CacheKeys.FitzyLosses, entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                return 0;
            });

            var draws = _cache.GetOrCreate(CacheKeys.FitzyDraws, entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                return 0;
            });

            return $"{wins} {losses} {draws}";
        }
    }
}
