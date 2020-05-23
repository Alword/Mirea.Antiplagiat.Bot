using Microsoft.VisualBasic;
using Mirea.Antiplagiat.Bot.Commands;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Mirea.Antiplagiat.Bot.Models.Commands
{
    public class StartCommand : BaseCommand
    {
        public override string Name => "start";

        public override void Execute(Message message, ITelegramBotClient client)
        {
            client.SendTextMessageAsync(message.Chat.Id, AppData.Strings.HelloWorld);
        }
    }
}
