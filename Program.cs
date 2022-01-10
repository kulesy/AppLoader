using IWshRuntimeLibrary;
using System;
using System.IO;
using System.Linq;

namespace AppLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter The App You Want To Run:");
            var appToStart = Console.ReadLine();
            var filePath = Tools.GetBaseFilePath();
            string[] dirs = Directory.GetDirectories(filePath);
            foreach (var dir in dirs)
            {
                if (dir == "Apps")
                {
                    break;
                }
                if (dirs.ToList().Last() == dir)
                {
                    Tools.SendCommand($"mkdir Apps");
                }
            }
            Tools.SendCommand($"start Apps\\{appToStart}");

            
        }


    }
}
