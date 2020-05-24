using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Mirea.Antiplagiat.Bot.Data
{
    public class MireaAntiplagiatDataContext
    {
        public Dictionary<string, int> PathUserId { get; }
        public MireaAntiplagiatDataContext()
        {
            this.PathUserId = new Dictionary<string, int>();
        }
    }
}
