using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetEscapades.AspNetCore.SecurityHeaders;
using System;
using System.Collections.Concurrent;
using System.IO;
using WebSockets.Web.Classes;
using WebSockets.Web.Data;
using WebSockets.Web.Utils;

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

            app.UseSecurityHeaders(new HeaderPolicyCollection()
                .AddDefaultSecurityHeaders()
                .RemoveServerHeader());

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(20)
            });

            app.UseMiddleware<FitzyWinLossWebSocket>();
            app.UseMvc();
        }
    }
}
