using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mirea.Antiplagiat.Bot.Commands;
using Mirea.Antiplagiat.Bot.Controllers;
using Mirea.Antiplagiat.Bot.Models;
using Mirea.Antiplagiat.Bot.Models.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkNet;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace Mirea.Antiplagiat.Bot
{
    internal class App
    {

        private readonly ILogger<App> logger;
        private readonly IVkApi bot;
        private readonly IConfiguration configuration;

        private readonly CheckDocument checkDocument;
        private readonly List<BaseCommand> commands;
        private readonly ulong groupId;

        public App(ILogger<App> logger,
            IVkApi bot,
            IConfigurationRoot configuration)
        {
            this.logger = logger;
            this.bot = bot;
            this.configuration = configuration;

            groupId = configuration.GetSection("ConnectionStrings").GetValue<ulong>("GroupId");


            this.commands = new List<BaseCommand> { new StartCommand() };
            checkDocument = new CheckDocument();
        }
        internal Task Run()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    var s = bot.Groups.GetLongPollServer(groupId);
                    var poll = bot.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams() { Server = s.Server, Ts = s.Ts, Key = s.Key, Wait = 25 });
                    if (poll?.Updates == null) continue;
                    foreach (var a in poll.Updates)
                    {
                        if (a.Type == GroupUpdateType.MessageNew)
                        {
                            SendMessage(a.MessageNew.Message.Text.ToLower(), a.MessageNew.Message.FromId);
                        }
                    }
                }
            });
        }

        public void SendMessage(string message, long? userID)
        {
            Random rnd = new Random();
            bot.Messages.Send(new MessagesSendParams
            {
                RandomId = rnd.Next(),
                UserId = userID,
                Message = message
            });
        }
    }
}