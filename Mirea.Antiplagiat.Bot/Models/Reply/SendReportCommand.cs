using Mirea.Antiplagiat.Bot.Abstractions;
using Mirea.Antiplagiat.Bot.Commands;
using Mirea.Antiplagiat.Bot.Controllers;
using Mirea.Antiplagiat.Bot.Data;
using Mirea.Antiplagiat.Bot.Extentions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using VkNet.Abstractions;
using VkNet.Model.GroupUpdate;

namespace Mirea.Antiplagiat.Bot.Models.Commands
{
    public class SendReportCommand : IBackgroundWorker
    {
        private readonly IAntiplagiatService antiplagiatService;
        private readonly MireaAntiplagiatDataContext context;
        private readonly IVkApi vkApi;
        public SendReportCommand(IAntiplagiatService antiplagiatService, IVkApi vkApi, MireaAntiplagiatDataContext context)
        {
            this.context = context;
            this.antiplagiatService = antiplagiatService;
            antiplagiatService.OnDocumentChecked += AntiplagiatService_OnDocumentChecked;
        }
        private void AntiplagiatService_OnDocumentChecked(object sender, string documentPath, string resultPath)
        {
            if (context.PathUserId.ContainsKey(documentPath))
            {
                vkApi.Reply(context.PathUserId[documentPath], AppData.Strings.ReportIsReady);
            }
        }
        ~SendReportCommand()
        {
            antiplagiatService.OnDocumentChecked -= AntiplagiatService_OnDocumentChecked;
        }
    }
}
