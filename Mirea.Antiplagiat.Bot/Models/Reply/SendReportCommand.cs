using Mirea.Antiplagiat.Bot.Abstractions;
using Mirea.Antiplagiat.Bot.Commands;
using Mirea.Antiplagiat.Bot.Controllers;
using Mirea.Antiplagiat.Bot.Data;
using Mirea.Antiplagiat.Bot.Extentions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using VkNet.Abstractions;
using VkNet.Enums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;

namespace Mirea.Antiplagiat.Bot.Models.Commands
{
    public class SendReportCommand : IBackgroundWorker
    {
        private readonly IAntiplagiatService antiplagiatService;
        private readonly MireaAntiplagiatDataContext context;
        private readonly IVkApi vkApi;
        public SendReportCommand(IAntiplagiatService antiplagiatService, IVkApi vkApi, MireaAntiplagiatDataContext context)
        {
            this.vkApi = vkApi;
            this.context = context;
            this.antiplagiatService = antiplagiatService;
            antiplagiatService.OnDocumentChecked += AntiplagiatService_OnDocumentChecked;
        }
        private void AntiplagiatService_OnDocumentChecked(object sender, string documentPath, string resultPath)
        {
            if (context.PathUserId.ContainsKey(documentPath))
            {
                long userId = context.PathUserId[documentPath];

                var uploadServer = vkApi.Docs.GetMessagesUploadServer(userId);


                int repeatTime = 10;
                bool ok = false;
                while (!ok && repeatTime > 0)
                {
                    repeatTime--;
                    using var wc = new WebClient();
                    try
                    {
                        var result = Encoding.ASCII.GetString(wc.UploadFile(uploadServer.UploadUrl, resultPath));
                        var docs = vkApi.Docs.Save(result, title: Path.GetFileName(resultPath), tags: null);
                        vkApi.Messages.Send(new MessagesSendParams()
                        {
                            RandomId = new Random().Next(),
                            UserId = userId,
                            Message = AppData.Strings.CheckComplete,
                            Attachments = new List<MediaAttachment> { docs.First().Instance },
                        });
                    }
                    catch (Exception e)
                    {
                        ok = false;
                        System.Threading.Thread.Sleep(10000);
                    }
                    ok = true;
                }
            }
        }
        ~SendReportCommand()
        {
            antiplagiatService.OnDocumentChecked -= AntiplagiatService_OnDocumentChecked;
        }
    }
}
