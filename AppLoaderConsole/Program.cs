using AppLoaderClassLibrary.Endpoints;
using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AppLoader
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter The App You Want To Run:");
            var appToStart = Console.ReadLine();
            var filePath = Tools.GetBaseFilePath();
            List<string> dirs = Directory.GetDirectories(filePath).ToList();
            foreach (var dir in dirs)
            {
                if (dir == "Apps")
                {
                    break;
                }
                if (dirs[dirs.Count - 1] == dir)
                {
                    Tools.SendCommand($"mkdir Apps");
                }
            }
            Tools.CreateShortcut(@"C:\Users\The Little Monkey\AppData\Roaming\Spotify\Spotify.exe");                                            
            Tools.SendCommand(@$"start Apps\{appToStart}");
        }


    }
}
