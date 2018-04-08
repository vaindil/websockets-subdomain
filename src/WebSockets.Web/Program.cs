using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSockets.Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(l => l.SetMinimumLevel(LogLevel.Warning))
                .UseKestrel(o => o.AddServerHeader = false)
                .UseStartup<Startup>()
                .Build();
    }
}
