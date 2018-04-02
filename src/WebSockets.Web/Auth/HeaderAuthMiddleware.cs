using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebSockets.Web.Auth
{
    public class HeaderAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiSecret;

        public HeaderAuthMiddleware(RequestDelegate next, string apiSecret)
        {
            _next = next;
            _apiSecret = apiSecret;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var header)
                && header.ToString() == _apiSecret)
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Missing or invalid authorization header");
        }
    }
}
