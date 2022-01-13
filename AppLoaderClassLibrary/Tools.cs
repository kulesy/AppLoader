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
            //The base directory starts in a weird folder
            //Therefore some editing of the path needs to be done to bring it to the AppLoader folder
            var fullBasePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var fullBasePathSplit = fullBasePath.Split(@"\");
            var basePathSplit = fullBasePathSplit.SkipLast(4);
            var basePath = "";
            foreach (var path in basePathSplit)
            {
                var normalizedPath = path;
                basePath += normalizedPath + @"\";
            }
            return basePath;
        }

        public static string GetBaseFilePathForCommands()
        {
            //Boiler plate code from GetBaseFilePath()
            //But is neccesary some commands in the cmd require folders that contain spaces to be in quotes
            var fullBasePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var fullBasePathSplit = fullBasePath.Split(@"\");
            var basePathSplit = fullBasePathSplit.SkipLast(4);
            var basePath = "";
            foreach (var path in basePathSplit)
            {
                var normalizedPath = path;
                if (path.Contains(" "))
                {
                    normalizedPath = $"\"{path}\"";
                }
                basePath += normalizedPath + @"\";
            }
            return basePath;
        }
        public static void SendCommand(string command)
        {
            //Sends command through cmd
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //This is to make sure the console is not shown
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            // /C is vital for the command to execute
            startInfo.Arguments = $"/C {command}";
            startInfo.WorkingDirectory = GetBaseFilePath();
            process.StartInfo = startInfo;
            process.Start();
        }

        public static void CreateShortcut(string appFilePath)
        {
            //For some reason apps redirects to an Update.exe file which cannot launch the desired app
            //Therefore apps that do redirect to an Update.exe file must throw an error
            if (appFilePath.Contains("Update.exe") is false)
            {
                WshShell shell = new WshShell();
                //Where the shortcut of the app will be saved
                string shortcutAddress = GetBaseFilePath() + @$"Apps\{GetAppFromPath(appFilePath)}.lnk";
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                //The path of the app that you want to make an shortcut
                shortcut.TargetPath = appFilePath;
                shortcut.Save();

                //Creating an icon file with the same naming as the shortcut to display on the UI
                //Not sure of the reason of the warning, the icon is still created
                #pragma warning disable CA1416 // Validate platform compatibility
                System.Drawing.Icon.ExtractAssociatedIcon(appFilePath).ToBitmap().Save(Tools.GetBaseFilePath() + $@"Apps\{GetAppFromPath(appFilePath)}.ico");
                #pragma warning restore CA1416 // Validate platform compatibility
            }
            else
            {
                throw new("Executable cannot be an update");
            }
        }

        public static string GetAppFromPath(string path)
        {
            //Getting just the app name
            var appNameWithExtension = path.Split(@"\").Last();
            var appName = appNameWithExtension.Split(".").First();
            return appName;
        }
    }
}
