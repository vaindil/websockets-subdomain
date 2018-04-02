using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using WebSockets.Web.Models.Configs;

namespace WebSockets.Web.Auth
{
    public class FitzyHeaderAuthPipeline
    {
        public void Configure(IApplicationBuilder applicationBuilder, IOptions<FitzyConfig> options)
        {
            applicationBuilder.UseMiddleware<HeaderAuthMiddleware>(options.Value.ApiSecret);
        }
    }
}
