using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetEscapades.AspNetCore.SecurityHeaders;
using System;
using System.Collections.Concurrent;
using System.IO;
using WebSockets.Data;
using WebSockets.Data.Services;
using WebSockets.Web.Models;
using WebSockets.Web.Models.Configs;
using WebSockets.Web.Utils;
using WebSockets.Web.WebSockets;
using WebSockets.Web.WebSockets.Middleware;

namespace WebSockets.Web
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
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<VbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("Postgres")));

            services.AddMvc();
            services.AddMemoryCache();

            services.Configure<FitzyConfig>(Configuration.GetSection("Fitzy"));
            services.Configure<TwitchConfig>(Configuration.GetSection("Twitch"));

            // no, I'm not using interfaces. sue me.
            services.AddSingleton<FitzyWebSocketManager>();
            services.AddSingleton<TwitchWebSocketManager>();
            services.AddScoped<KeyValueService>();
        }

        public void Configure(IApplicationBuilder app, IMemoryCache cache, KeyValueService kvSvc)
        {
            InitializeCache(kvSvc, cache);

            cache.Set(CacheKeys.TwitchStreamUpDown, new ConcurrentQueue<TwitchStreamUpDown>(), CacheHelpers.EntryOptions);
            cache.Set(CacheKeys.TwitchStreamUpDownHasListeners, false, CacheHelpers.EntryOptions);

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

            app.UseMiddleware<FitzyWebSocketMiddleware>();
            app.UseMiddleware<TwitchWebSocketMiddleware>();
            app.UseMvc();
        }

        private void InitializeCache(KeyValueService kvSvc, IMemoryCache cache)
        {
            var winKv = kvSvc.GetByKeyAsync(CacheKeys.FitzyWins).GetAwaiter().GetResult();
            var lossKv = kvSvc.GetByKeyAsync(CacheKeys.FitzyLosses).GetAwaiter().GetResult();
            var drawKv = kvSvc.GetByKeyAsync(CacheKeys.FitzyDraws).GetAwaiter().GetResult();

            int.TryParse(winKv?.Value, out var winCount);
            int.TryParse(lossKv?.Value, out var lossCount);
            int.TryParse(drawKv?.Value, out var drawCount);

            cache.Set(CacheKeys.FitzyWins, winCount, CacheHelpers.EntryOptions);
            cache.Set(CacheKeys.FitzyLosses, lossCount, CacheHelpers.EntryOptions);
            cache.Set(CacheKeys.FitzyDraws, drawCount, CacheHelpers.EntryOptions);
        }
    }
}
