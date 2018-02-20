using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;

namespace WebSockets
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddMvc(o =>
            {
                o.RequireHttpsPermanent = true;
                o.Filters.Add(new RequireHttpsAttribute());
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            // this should already be done in nginx but meh
            var options = new RewriteOptions().AddRedirectToHttps();
            app.UseRewriter(options);

            app.UseWebSockets();
            app.UseMiddleware<FitzyWinLossMiddleware>();
            app.UseMvc();
        }
    }
}
