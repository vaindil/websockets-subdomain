using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using TwitchLib.Api.Helix;
using VainBot.Infrastructure;
using WebSockets.Data;
using WebSockets.Data.Services;
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

            services.AddHttpClient(NamedHttpClients.TwitchOAuth,
                options => options.BaseAddress = new Uri("https://id.twitch.tv/oauth2/"));

            services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);

            services.Configure<FitzyConfig>(Configuration.GetSection("Fitzy"));
            services.Configure<ZubatConfig>(Configuration.GetSection("Zubat"));
            services.Configure<TwitchConfig>(Configuration.GetSection("Twitch"));

            var twitchApi = new Helix();
            twitchApi.Settings.ClientId = Configuration["Zubat:TwitchClientId"];

            services.AddSingleton(twitchApi);

            services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));

            // no, I'm not using interfaces. sue me.
            services.AddSingleton<FitzyWebSocketManager>();
            services.AddSingleton<TwitchWebSocketManager>();
            services.AddSingleton<TwitchActionsWebSocketManager>();
            services.AddScoped<KeyValueService>();
        }

        public void Configure(IApplicationBuilder app, IMemoryCache cache, KeyValueService kvSvc)
        {
            InitializeCache(kvSvc, cache);

            cache.Set(CacheKeys.TwitchNotificationIds, new List<string>(), CacheHelpers.EntryOptions);

            app.UseForwardedHeaders();

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
            app.UseMiddleware<TwitchActionsWebSocketMiddleware>();
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
