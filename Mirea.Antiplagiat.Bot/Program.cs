using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mirea.Antiplagiat.Bot.Controllers;
using Mirea.Antiplagiat.Bot.Data;
using Mirea.Antiplagiat.Bot.Models;
using Mirea.Antiplagiat.Bot.Models.Commands;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using VkNet;
using VkNet.Abstractions;
using VkNet.Model;

namespace Mirea.Antiplagiat.Bot
{
    class Program
    {
        public static IConfigurationRoot configuration;

        static int Main(string[] args)
        {
            // Initialize serilog logger
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                 .MinimumLevel.Debug()
                 .Enrich.FromLogContext()
                 .CreateLogger();

            Log.Information(AppData.Strings.CreatingServiceCollection);
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            // Create service provider
            Log.Information("Building service provider");
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<App>().Run().GetAwaiter().GetResult();
            //MainAsync(args).GetAwaiter().GetResult();
            return 0;
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Add logging
            serviceCollection.AddSingleton(LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(dispose: true);
            }));

            serviceCollection.AddLogging();

            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.secret.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

            // Add bot
            string token = configuration.GetConnectionString("DataConnection");
            var api = new VkApi();
            api.Authorize(new ApiAuthParams() { AccessToken = token });
            serviceCollection.AddSingleton<IVkApi>(api);

            // Add antiplagiat
            Credentials credentials = configuration.GetSection(nameof(credentials)).Get<Credentials>();
            serviceCollection.AddSingleton(credentials);
            serviceCollection.AddSingleton<IAntiplagiatService, AntiplagiatService>();

            // Add app context
            serviceCollection.AddSingleton<MireaAntiplagiatDataContext>();
            serviceCollection.AddTransient<SendReportCommand>();

            // Add app
            serviceCollection.AddTransient<App>();
        }
    }
}
