using Mirea.Antiplagiat.Bot.Commands;
using Mirea.Antiplagiat.Bot.Controllers;
using Mirea.Antiplagiat.Bot.Data;
using Mirea.Antiplagiat.Bot.Extentions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.GroupUpdate;
using File = System.IO.File;

namespace Mirea.Antiplagiat.Bot.Models.Commands
{
    class CheckDocument : BaseCommand
    {
        public override string Name => throw new NotImplementedException();

        private readonly IAntiplagiatService antiplagiatService;
        private readonly MireaAntiplagiatDataContext context;
        public CheckDocument(IAntiplagiatService antiplagiatService, MireaAntiplagiatDataContext context)
        {
            this.antiplagiatService = antiplagiatService;
            this.context = context;
        }
        public override bool Execute(GroupUpdate update, IVkApi bot)
        {
            if (update.Type == GroupUpdateType.MessageNew &&
                update.MessageNew != null &&
                update.MessageNew.Message != null
                && update.MessageNew.Message.Attachments.Any())
            {
                var attachment = update.MessageNew.Message.Attachments.First();
                if (attachment.Type == typeof(Document))
                {
                    var document = attachment.Instance as Document;

                    using (var client = new WebClient())
                    {
                        string salt = Guid.NewGuid().ToString().Substring(0, 8);
                        string path = Path.Combine(Folders.Docs(), $"{update.MessageNew.Message.FromId} {salt} Check.{document.Ext}");
                        client.DownloadFile(document.Uri, path);
                        update.ReplyMessageNew(bot, AppData.Strings.DocumentSaved);
                        context.PathUserId.Add(path, (long)update.MessageNew.Message.FromId);
                        antiplagiatService.EnqueueDocument(path);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
