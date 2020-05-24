using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Abstractions;
using VkNet.Model;

namespace Mirea.Antiplagiat.Bot.Commands
{
    public abstract class BaseCommand
    {
        public abstract string Name { get;}
        public abstract void Execute(Message message, IVkApi client);
        public bool Contaions(string command) 
        {
            return command.Contains(this.Name);
        }
    }
}
