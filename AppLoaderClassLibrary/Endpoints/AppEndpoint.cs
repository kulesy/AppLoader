using AppLoaderClassLibrary.Models;
using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppLoaderClassLibrary.Endpoints
{
    public class AppEndpoint : IAppEndpoint
    {
        public void MakeAppFolder()
        {
            // Get directories in current base directory and check if App folder is avalible
            // If not then create folder
            List<string> dirs = Directory.GetDirectories(GetBaseFilePath()).ToList();
            foreach (var dir in dirs)
            {
                if (dir.Contains("Apps"))
                {
                    break;
                }
                // If this if statement is triggered it means that no App folder has been found
                if (dirs[dirs.Count - 1] == dir)
                {
                    SendCommand($"mkdir Apps");
                }
            }
        }
        public List<AppModel> GetListOfApps()
        {
            var filePath = GetBaseFilePath() + @"Apps";
            //Get list of files with only .lnk extension
            List<string> shortcutPaths = Directory.GetFiles(filePath).Where(e => e.Contains(".lnk")).ToList();
            List<AppModel> apps = new();
            //Sort through each file path and map them to the AppModel then Add to the list of apps
            foreach (var path in shortcutPaths)
            {
                AppModel app = new();
                var appNameWithExtension = path.Split(@"\").Last();
                var appName = appNameWithExtension.Remove(appNameWithExtension.Count() - 4);
                app.FileName = appName;
                app.FileIcon = filePath + @$"\{appName}" + ".ico";
                apps.Add(app);
            }
            return apps;
        }

        public void CleanAppsFolder()
        {
            //This is used for cleaning up any icon files that are unassigned to a shortcut.
            //It is neccessary as the icons cannot be deleted when the app is running.
            var filePath = GetBaseFilePath() + @"Apps";
            //Get list of files with only .lnk extension
            List<string> shortcutPaths = Directory.GetFiles(filePath).Where(e => e.Contains(".lnk")).ToList();
            //Get list of files with only .ico extension
            List<string> iconPaths = Directory.GetFiles(filePath).Where(e => e.Contains(".ico")).ToList();
            if (shortcutPaths.Count != iconPaths.Count)
            {
                //if only icon files in folder delete all
                if (shortcutPaths.Count() == 0)
                {
                    SendCommand($"del /f {GetBaseFilePathForCommands() + "Apps" + $@"\*.ico"}");
                }
                //if only shorcut files in folder delete all
                else if (iconPaths.Count() == 0)
                {
                    SendCommand($"del /f {GetBaseFilePathForCommands() + "Apps" + $@"\*.lnk"}");
                }
                //Reflect each icon paths against the shortcut paths to find any unassigned icons
                foreach (var iconPath in iconPaths)
                {
                    foreach (var shortcutPath in shortcutPaths)
                    {
                        // Finding equality through taking the extension off the paths 
                        if (iconPath.Split('.')[0] == shortcutPath.Split('.')[0])
                        {
                            break;
                        }
                        // If this if statement is triggered it means that the icon file is unassigned to a shortcut file, therefore delete it
                        if (shortcutPaths[shortcutPaths.Count - 1] == shortcutPath)
                        {
                            SendCommand($"del /f {GetBaseFilePathForCommands() + "Apps" + $@"\{GetAppFromPath(iconPath)}.ico"}");
                        }
                    }
                }
            }
        }


        // Tools

        public string GetBaseFilePath()
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

        public string GetBaseFilePathForCommands()
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
        public void SendCommand(string command)
        {
            var normalizedCommand = "/C " + command;
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = normalizedCommand,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true // Hide console window
                }
            };

            proc.Start();
            proc.WaitForExit();//May need to wait for the process to exit too
        }

        public void CreateShortcut(string appFilePath)
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
                System.Drawing.Icon.ExtractAssociatedIcon(appFilePath).ToBitmap().Save(GetBaseFilePath() + $@"Apps\{GetAppFromPath(appFilePath)}.ico");
                #pragma warning restore CA1416 // Validate platform compatibility
            }
            else
            {
                throw new("Executable cannot be an update");
            }
        }

        public string GetAppFromPath(string path)
        {
            //Getting just the app name
            var appNameWithExtension = path.Split(@"\").Last();
            var appName = appNameWithExtension.Split(".").First();
            return appName;
        }
    }
}
