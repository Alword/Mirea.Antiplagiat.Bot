using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Mirea.Antiplagiat.Bot.Commands
{
    public abstract class BaseCommand
    {
        public abstract string Name { get;}
        public abstract void Execute(Message message, ITelegramBotClient client);
        public bool Contaions(string command) 
        {
            return command.Contains(this.Name);
        }
    }
}
