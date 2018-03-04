using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.IO;
using WebSockets.Classes;
using WebSockets.Data;
using WebSockets.Utils;

namespace WebSockets
{
    public class Startup
    {
        private static IConfiguration Configuration { get; set; }

        public Startup()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddMvc();

            services.AddSingleton(Configuration);
            services.AddSingleton<IRepository, Repository>();
            services.Configure<FitzyConfig>(Configuration.GetSection("Fitzy"));
            services.Configure<TwitchConfig>(Configuration.GetSection("Twitch"));
        }

        public void Configure(IApplicationBuilder app, IMemoryCache cache)
        {
            cache.Set(CacheKeys.TwitchStreamUpDown, new ConcurrentQueue<TwitchStreamUpDown>(), CacheHelpers.GetEntryOptions());
            cache.Set(CacheKeys.TwitchStreamUpDownHasListeners, false, CacheHelpers.GetEntryOptions());

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(20)
            });

            app.UseMiddleware<FitzyWinLossWebSocket>();
            app.UseMvc();
        }
    }
}
