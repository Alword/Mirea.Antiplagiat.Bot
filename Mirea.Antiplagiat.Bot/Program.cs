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

            MainAsync(args).GetAwaiter().GetResult();
            return 0;
        }

        static async Task MainAsync(string[] args)
        {
            // Create service collection
            Log.Information(AppData.Strings.CreatingServiceCollection);
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ConfigureFolders();
            // Create service provider
            Log.Information("Building service provider");
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Print connection string to demonstrate configuration object is populated
            try
            {
                Log.Information("Starting service");
                await serviceProvider.GetService<App>().Run();
                Log.Information("Ending service");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error running service");
                throw ex;
            }
            finally
            {
                Console.ReadLine();
                Log.CloseAndFlush();
            }
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

        private static void ConfigureFolders()
        {
            var root = Environment.CurrentDirectory;

            string[] directorys = new string[] { "docs", "reports" };

            foreach (var dir in directorys)
            {
                var path = Path.Combine(root, dir);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                Log.Information($"{AppData.Strings.DicrectoryCreated} {path}");
            };
        }
    }
}
