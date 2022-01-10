using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLoader
{
    public static class Tools
    {
        public static string GetBaseFilePath()
        {
            return Path.Combine("C:", "Users", "The Little Monkey", "source", "repos", "AppLoader");
        }
        public static void SendCommand(string command)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C {command}";
            startInfo.WorkingDirectory = "C:\\Users\\The Little Monkey\\source\\repos\\AppLoader";
            process.StartInfo = startInfo;
            process.Start();
        }

        public static void CreateShortcut(string appFilePath)
        {
            WshShell shell = new WshShell();
            string shortcutAddress = GetBaseFilePath() + @"\Apps";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.TargetPath = appFilePath;
            shortcut.Save();
        }
    }
}
