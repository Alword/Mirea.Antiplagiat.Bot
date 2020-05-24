using Microsoft.VisualBasic;
using Mirea.Antiplagiat.Bot.Commands;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using VkNet.Abstractions;
using VkNet.Model;

namespace Mirea.Antiplagiat.Bot.Models.Commands
{
    public class StartCommand : BaseCommand
    {
        public override string Name => "start";

        public override void Execute(Message message, IVkApi client)
        {
            // client.SendTextMessageAsync(message.Chat.Id, AppData.Strings.HelloWorld);
        }
    }
}
