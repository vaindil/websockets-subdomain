using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSockets.Web.WebSockets
{
    public abstract class WebSocketManager
    {
        private readonly ConcurrentDictionary<Guid, WebSocket> _sockets;

        protected WebSocketManager()
        {
            _sockets = new ConcurrentDictionary<Guid, WebSocket>();
        }

        public Guid AddWebSocket(WebSocket socket)
        {
            // must already be accepted in the controller, that's how the WebSocket object is created

            var guid = Guid.NewGuid();
            _sockets.TryAdd(guid, socket);

            return guid;
        }

        public async Task ReceiveUntilClosedAsync(Guid guid)
        {
            if (!_sockets.TryGetValue(guid, out var socket))
            {
                await RemoveWebSocketAsync(guid);
                return;
            }

            try
            {
                var buffer = new byte[128];
                var receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (receiveResult.MessageType != WebSocketMessageType.Close)
                {
                    receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await RemoveWebSocketAsync(guid);
            }
            catch (WebSocketException ex)
            {
                await Console.Error.WriteAsync($"{DateTime.UtcNow.ToString("yy-MM-dd hh:mm:ss")}: Websocket exception: {ex.Message}");
            }
        }

        public async Task SendAsync(Guid guid, string message)
        {
            if (!_sockets.TryGetValue(guid, out var socket))
                return;

            if (socket.State != WebSocketState.Open)
            {
                await RemoveWebSocketAsync(guid);
                return;
            }

            await SendMessageAsync(socket, message);
        }

        public async Task SendAllAsync(string message)
        {
            foreach (var socket in _sockets)
            {
                if (socket.Value.State == WebSocketState.Open)
                    await SendMessageAsync(socket.Value, message);
                else
                    await RemoveWebSocketAsync(socket.Key);
            }
        }

        private async Task SendMessageAsync(WebSocket socket, string message)
        {
            try
            {
                await socket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                if (!(ex.InnerException is ObjectDisposedException))
                    throw;
            }
        }

        private async Task RemoveWebSocketAsync(Guid guid)
        {
            if (!_sockets.TryRemove(guid, out var socket))
                return;

            if (socket?.State == WebSocketState.Open)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);

            socket.Dispose();
        }
    }
}
