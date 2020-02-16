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
            services.AddMemoryCache();

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<VbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("Postgres")));

            services.AddControllers();

            services.AddHttpClient(NamedHttpClients.TwitchOAuth,
                options => options.BaseAddress = new Uri("https://id.twitch.tv/oauth2/"));

            services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);

            services.Configure<FitzyConfig>(Configuration.GetSection("Fitzy"));
            services.Configure<CrendorConfig>(Configuration.GetSection("Crendor"));
            services.Configure<TwitchConfig>(Configuration.GetSection("Twitch"));

            services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));

            // no, I'm not using interfaces. sue me.
            services.AddSingleton<FitzyWebSocketManager>();
            services.AddSingleton<TwitchWebSocketManager>();
            services.AddSingleton<TwitchActionsWebSocketManager>();
            services.AddSingleton<CrendorWebSocketManager>();
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
                .AllowAnyMethod());

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(20)
            });

            app.UseMiddleware<FitzyWebSocketMiddleware>();
            app.UseMiddleware<TwitchActionsWebSocketMiddleware>();
            app.UseMiddleware<TwitchWebSocketMiddleware>();
            app.UseMiddleware<CrendorWebSocketMiddleware>();

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private void InitializeCache(KeyValueService kvSvc, IMemoryCache cache)
        {
            var winKv = kvSvc.GetByKeyAsync(CacheKeys.FitzyWins).GetAwaiter().GetResult();
            var lossKv = kvSvc.GetByKeyAsync(CacheKeys.FitzyLosses).GetAwaiter().GetResult();
            var drawKv = kvSvc.GetByKeyAsync(CacheKeys.FitzyDraws).GetAwaiter().GetResult();

            int.TryParse(winKv?.Value ?? "0", out var winCount);
            int.TryParse(lossKv?.Value ?? "0", out var lossCount);
            int.TryParse(drawKv?.Value ?? "0", out var drawCount);

            cache.Set(CacheKeys.FitzyWins, winCount, CacheHelpers.EntryOptions);
            cache.Set(CacheKeys.FitzyLosses, lossCount, CacheHelpers.EntryOptions);
            cache.Set(CacheKeys.FitzyDraws, drawCount, CacheHelpers.EntryOptions);

            var crendorPointsKv = kvSvc.GetByKeyAsync(CacheKeys.CrendorSubPoints).GetAwaiter().GetResult();
            int.TryParse(crendorPointsKv?.Value ?? "-1", out var crendorPoints);

            cache.Set(CacheKeys.CrendorSubPoints, crendorPoints, CacheHelpers.EntryOptions);
        }
    }
}
