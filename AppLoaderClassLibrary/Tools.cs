using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppLoaderClassLibrary
{
    public static class Tools
    {
        public static string GetBaseFilePath()
        {
            var fullBasePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var fullBasePathSplit = fullBasePath.Split(@"\");
            var basePathSplit = fullBasePathSplit.SkipLast(4);
            var basePath = "";
            foreach (var path in basePathSplit)
            {
                basePath += path + @"\";
            }
            return basePath;
        }
        public static void SendCommand(string command)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C {command}";
            startInfo.WorkingDirectory = GetBaseFilePath();
            process.StartInfo = startInfo;
            process.Start();
        }

        public static void CreateShortcut(string appFilePath)
        {
            WshShell shell = new WshShell();
            string shortcutAddress = GetBaseFilePath() + @$"Apps\{GetAppFromPath(appFilePath)}.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.TargetPath = appFilePath;
            shortcut.Save();
        }

        public static string GetAppFromPath(string path)
        {
            var appNameWithExtension = path.Split(@"\").Last();
            var appName = appNameWithExtension.Split(".").First();
            return appName;
        }
    }
}
