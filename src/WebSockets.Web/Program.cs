﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WebSockets
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(o => o.AddServerHeader = false)
                .UseStartup<Startup>()
                .Build();
    }
}
