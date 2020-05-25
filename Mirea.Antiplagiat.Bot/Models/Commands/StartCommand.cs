using Microsoft.VisualBasic;
using Mirea.Antiplagiat.Bot.Commands;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;

namespace Mirea.Antiplagiat.Bot.Models.Commands
{
    public class StartCommand : BaseCommand
    {
        public override string Name => "start";

        public override bool Execute(GroupUpdate update, IVkApi client)
        {
            if (update.Type == GroupUpdateType.MessageNew)
            {
                Random rnd = new Random();
                client.Messages.Send(new MessagesSendParams
                {
                    RandomId = rnd.Next(),
                    UserId = update.MessageNew.Message.FromId,
                    Message = AppData.Strings.Help
                });
                return true;
            }
            return false;
        }
    }
}
