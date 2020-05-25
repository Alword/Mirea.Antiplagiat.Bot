using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Mirea.Antiplagiat.Bot.Models.AntiplagiatService;

namespace Mirea.Antiplagiat.Bot.Controllers
{
    public interface IAntiplagiatService
    {
        public event DocumentCheckedEvent OnDocumentChecked;
        public void EnqueueDocument(string path);
        public void Run(CancellationToken token);
    }
}
