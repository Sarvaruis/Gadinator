using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ProjektTestowySerwer.Models;
using System;
using System.IO;

namespace ProjektTestowySerwer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!Directory.Exists("Backgrounds"))
            {
                Directory.CreateDirectory("Backgrounds");
            }
            if (!Directory.Exists("ObjectImages"))
            {
                Directory.CreateDirectory("ObjectImages");
            }

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://localhost:4990")
                .UseStartup<Startup>()
                .Build();
    }
}
