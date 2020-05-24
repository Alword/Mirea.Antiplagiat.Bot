using Mirea.Antiplagiat.Bot.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Model;
using File = System.IO.File;

namespace Mirea.Antiplagiat.Bot.Models.Commands
{
    class CheckDocument : BaseCommand
    {
        public override string Name => throw new NotImplementedException();

        public override async void Execute(Message message, IVkApi client)
        {
            //var file = await client.GetFileAsync(message.do.FileId);

            //string path = Path.Combine(Environment.CurrentDirectory, "docs");
            //if (!Directory.Exists(path))
            //    Directory.CreateDirectory(path);

            //path = Path.Combine(path, file.FileId + "." + file.FilePath.Split('.').Last());

            //using (var saveImageStream = File.Open(path, FileMode.Create))
            //{
            //    await client.GetInfoAndDownloadFileAsync(file.FileId, saveImageStream);
            //}
            //await client.SendTextMessageAsync(message.Chat.Id, AppData.Strings.DocumentSaved);
        }
    }
}
