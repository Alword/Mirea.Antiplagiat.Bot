using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mirea.Antiplagiat.Bot.Extentions
{
    public static class Folders
    {
        public static string Docs()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "docs");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}
