using Microsoft.Extensions.Logging;
using Mirea.Antiplagiat.Bot.Commands;
using Mirea.Antiplagiat.Bot.Models.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Mirea.Antiplagiat.Bot
{
    internal class App
    {

        private readonly ILogger<App> logger;
        private readonly ITelegramBotClient telegramBotClient;

        private readonly CheckDocument checkDocument;
        private readonly List<BaseCommand> commands;
        private int offset = 0;
        public App(ILogger<App> logger, ITelegramBotClient telegramBotClient)
        {
            this.logger = logger;
            this.telegramBotClient = telegramBotClient;
            this.commands = new List<BaseCommand> { new StartCommand() };
            checkDocument = new CheckDocument();
        }
        internal async Task Run()
        {
            while (true)
            {
                var updates = await telegramBotClient.GetUpdatesAsync(offset);
                foreach (var update in updates)
                {
                    offset = update.Id + 1;

                    if (update.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
                    {
                        checkDocument.Execute(update.Message, telegramBotClient);
                        continue;
                    }

                    foreach (var command in commands)
                    {
                        if (command.Contaions(update.Message.Text))
                        {
                            command.Execute(update.Message, telegramBotClient);
                        }
                    }
                }
                await Task.Delay(250);
            }
        }
    }
}