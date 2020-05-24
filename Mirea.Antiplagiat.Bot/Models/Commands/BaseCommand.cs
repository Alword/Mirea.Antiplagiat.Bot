using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.GroupUpdate;

namespace Mirea.Antiplagiat.Bot.Commands
{
    public abstract class BaseCommand
    {
        public abstract string Name { get;}
        public abstract bool Execute(GroupUpdate update, IVkApi bot);
        public bool Contaions(string command) 
        {
            return command.Contains(this.Name);
        }
    }
}
