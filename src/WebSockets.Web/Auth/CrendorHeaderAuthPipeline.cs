using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using WebSockets.Web.Models.Configs;

namespace WebSockets.Web.Auth
{
    public class CrendorHeaderAuthPipeline
    {
        public void Configure(IApplicationBuilder applicationBuilder, IOptions<CrendorConfig> options)
        {
            applicationBuilder.UseMiddleware<HeaderAuthMiddleware>(options.Value.ApiSecret);
        }
    }
}
