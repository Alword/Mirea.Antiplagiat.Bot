using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mirea.Antiplagiat.Bot.Abstractions;
using Mirea.Antiplagiat.Bot.Commands;
using Mirea.Antiplagiat.Bot.Controllers;
using Mirea.Antiplagiat.Bot.Data;
using Mirea.Antiplagiat.Bot.Models;
using Mirea.Antiplagiat.Bot.Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IVkApi bot;
        private readonly List<BaseCommand> commands;
        private readonly ulong groupId;

        public App(ILogger<App> logger,
            IVkApi bot,
            IConfigurationRoot configuration,
            IAntiplagiatService antiplagiatService,
            MireaAntiplagiatDataContext context,
            SendReportCommand sendReportCommand)
        {
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
        internal Task Run()
        {
            var plagiatTask = antiplagiatService.Run();

            while (true)
            {
                var s = bot.Groups.GetLongPollServer(groupId);
                var poll = bot.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams() { Server = s.Server, Ts = s.Ts, Key = s.Key, Wait = 25 });
                if (poll?.Updates == null) continue;

                foreach (var update in poll.Updates)
                {
                    Task.Run(() =>
                    {
                        foreach (var command in commands)
                        {
                            if (command.Execute(update, bot)) break;
                        }
                    });
                }
            }
        }
    }
}