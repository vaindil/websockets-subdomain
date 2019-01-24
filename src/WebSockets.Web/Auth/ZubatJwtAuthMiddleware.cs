using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text;
using System.Threading.Tasks;
using WebSockets.Web.Models;
using WebSockets.Web.Models.Configs;

namespace WebSockets.Web.Auth
{
    public class ZubatJwtAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ZubatConfig _config;

        public ZubatJwtAuthMiddleware(RequestDelegate next, IOptions<ZubatConfig> zubatOptions)
        {
            _next = next;
            _config = zubatOptions.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var headerSv))
            {
                var header = headerSv.ToString();
                if (header.StartsWith("Bearer "))
                {
                    header = header.Remove(0, 7);

                    var key = Encoding.UTF8.GetBytes(_config.JwtSigningKey);
                    try
                    {
                        var jwt = Jose.JWT.Decode<ZubatJwt>(header, key, Jose.JwsAlgorithm.HS256);
                        context.Items["TwitchId"] = jwt.Twitch_id;
                    }
                    catch
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Missing or invalid authorization header");
                        return;
                    }

                    // I have no idea if it's okay to put this in the try/catch, so I put it here
                    await _next(context);
                    return;
                }
            }

            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Missing or invalid authorization header");
        }
    }
}
