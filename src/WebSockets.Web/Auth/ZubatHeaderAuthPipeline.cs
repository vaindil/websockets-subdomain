using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using WebSockets.Web.Models.Configs;

namespace WebSockets.Web.Auth
{
    public class ZubatHeaderAuthPipeline
    {
        public void Configure(IApplicationBuilder applicationBuilder, IOptions<ZubatConfig> options)
        {
            applicationBuilder.UseMiddleware<HeaderAuthMiddleware>(options.Value.ApiSecret);
        }
    }
}
