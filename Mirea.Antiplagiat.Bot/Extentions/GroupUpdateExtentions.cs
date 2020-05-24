using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Abstractions;
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;

namespace Mirea.Antiplagiat.Bot.Extentions
{
    public static class GroupUpdateExtentions
    {
        private static readonly Random random = new Random();

        public static void ReplyMessageNew(this GroupUpdate update, IVkApi vkApi, string message)
        {
            vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = random.Next(),
                UserId = update.MessageNew.Message.FromId,
                Message = message
            });
        }

        public static void Reply(this IVkApi vkApi, long id, string message)
        {
            vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = random.Next(),
                UserId = id,
                Message = message
            });
        }
    }
}
