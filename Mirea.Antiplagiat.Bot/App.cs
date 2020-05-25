using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mirea.Antiplagiat.Bot.Abstractions;
using Mirea.Antiplagiat.Bot.Commands;
using Mirea.Antiplagiat.Bot.Controllers;
using Mirea.Antiplagiat.Bot.Data;
using Mirea.Antiplagiat.Bot.Models;
using Mirea.Antiplagiat.Bot.Models.Commands;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace Mirea.Antiplagiat.Bot
{
    internal class App
    {

        private readonly List<IBackgroundWorker> workers;
        private readonly IAntiplagiatService antiplagiatService;
        private readonly ILogger<App> logger;
        private readonly IVkApi bot;
        private readonly List<BaseCommand> commands;
        private readonly ulong groupId;

        public App(
            ILogger<App> logger,
            IVkApi bot,
            IConfigurationRoot configuration,
            IAntiplagiatService antiplagiatService,
            MireaAntiplagiatDataContext context,
            SendReportCommand sendReportCommand)
        {
            logger.LogInformation(nameof(App));
            this.logger = logger;
            this.bot = bot;
            this.groupId = configuration.GetSection("ConnectionStrings").GetValue<ulong>("GroupId");
            this.antiplagiatService = antiplagiatService;
            workers = new List<IBackgroundWorker>
            {
                sendReportCommand
            };
            this.commands = new List<BaseCommand>
            {
                new CheckDocument(antiplagiatService,context),
                new StartCommand()
            };

        }
        internal async Task Run()
        {
            while (true)
            {
                CancellationTokenSource plagiatTask = new CancellationTokenSource();
                CancellationTokenSource botTask = new CancellationTokenSource();
                try
                {
                    Task t1 = Task.Run(() => BotCicle(botTask.Token)).ContinueWith((t) =>
                    {
                        LogTaskException(t, plagiatTask, botTask);
                    });
                    Task t2 = Task.Run(() => antiplagiatService.Run(plagiatTask.Token)).ContinueWith((t) =>
                    {
                        LogTaskException(t, plagiatTask, botTask);
                    });
                    await Task.WhenAll(t1, t2);
                }
                finally
                {
                    Log.Information("Восстановление процессов");
                }
            }
        }

        private static void LogTaskException(Task t, CancellationTokenSource plagiatTask, CancellationTokenSource botTask)
        {
            if (t.IsFaulted)
            {
                plagiatTask.Cancel();
                botTask.Cancel();
                Log.Error(t.Exception.Message);
                Log.Error(t.Exception.InnerException.Message);
            }
        }

        private void BotCicle(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var s = bot.Groups.GetLongPollServer(groupId);
                var poll = bot.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams() { Server = s.Server, Ts = s.Ts, Key = s.Key, Wait = 25 });
                if (poll?.Updates == null) continue;

                foreach (var update in poll.Updates)
                {
                    foreach (var command in commands)
                    {
                        if (command.Execute(update, bot)) break;
                    }
                }
            }
        }
    }
}