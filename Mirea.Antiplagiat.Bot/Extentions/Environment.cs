using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mirea.Antiplagiat.Bot.Extentions
{
    public static class Folders
    {
        private static string Folder(string folder)
        {
            string path = Path.Combine(Environment.CurrentDirectory, folder);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public static string Docs()
        {
            return Folder("docs");
        }

        public static string Repots()
        {
            return Folder("reports");
        }
    }
}
