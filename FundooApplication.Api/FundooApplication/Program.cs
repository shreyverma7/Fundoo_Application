using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FundooApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {

            CreateHostBuilder(args).Build().Run();
        }

        //This method is responsible for creating and configuring the host for the ASP.NET Core application. It returns an IHostBuilder instance.
        public static IHostBuilder CreateHostBuilder(string[] args) =>
        //This method creates a default host builder with some predefined configurations and services.It sets up various aspects of the application, including logging and configuration.
            Host.CreateDefaultBuilder(args)
             //This part of the code configures the web host, specifying that it should use a class called Startup to configure the application.The Startup class is responsible for configuring the application's services, middleware, and routing.
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
