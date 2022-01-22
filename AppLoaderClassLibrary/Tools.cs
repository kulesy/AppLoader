using AppLoaderClassLibrary.Models;
using IWshRuntimeLibrary;
using Shell32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppLoaderClassLibrary
{
    public static class Tools
    {
        public static void SendCommand(string command)
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

        public static string GetAppLoaderPath()
        {
            //The base directory starts in a weird folder
            //Therefore some editing of the path needs to be done to bring it to the AppLoader folder
            var basePath = Directory.GetCurrentDirectory();
            var basePathSplit = basePath.Split(@"\").ToList();
            var appLoaderIndex = basePathSplit.IndexOf("AppLoader");
            // Determine the number of folders/files need to be skipped
            var offset = basePathSplit.Count() - appLoaderIndex;
            offset--;
            var appLoaderPathSplit = basePathSplit.SkipLast(offset);
            var appLoaderPath = string.Join(@"\", appLoaderPathSplit);
            return appLoaderPath;
        }

        public static string GetBaseFilePathForCommands()
        {
            return NormalizeFilePath(Directory.GetCurrentDirectory());
        }

        public static string GetUserProfilePath()
        {
            // Need User Profile to direct to startup folder
            var basePath = Directory.GetCurrentDirectory();
            var basePathSplit = basePath.Split(@"\").ToList();
            // Determine the number of folders/files need to be skipped
            var offset = basePathSplit.Count() - 2;
            offset--;
            var profilePathSplit = basePathSplit.SkipLast(offset);
            var profilePath = string.Join(@"\", profilePathSplit);
            return profilePath;
        }

        public static string NormalizeFilePath(string filePath)
        {
            // Putting spaced folders/files in quotes for commands
            var filePathSplit = filePath.Split(@"\");
            var basePath = "";

            foreach (var path in filePathSplit)
            {
                var normalizedPath = path;
                if (path.Contains(" "))
                {
                    normalizedPath = $"\"{path}\"";
                }
                if (filePathSplit.Last() == path)
                {
                    basePath += normalizedPath;
                    break;
                }
                basePath += normalizedPath + @"\";
            }

            return basePath;
        }

        [STAThread]
        public static string GetExePathFromShortcut(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell shell = new Shell();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);

            if (folderItem != null)
            {
                ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return string.Empty;
        }

        public static string GetAppNameFromPath(string path)
        {
            //Getting just the app name
            var appNameWithExtension = path.Split(@"\").Last();
            var appName = appNameWithExtension.Split(".").First();
            return appName;
        }
        public static List<AppModel> GetListOfApps()
        {
            var filePath = Directory.GetCurrentDirectory() + @"\Apps";
            //Get list of files with only .lnk extension
            List<string> shortcutPaths = Directory.GetFiles(filePath).Where(e => e.Contains(".lnk")).ToList();
            List<AppModel> apps = new();
            //Sort through each file path and map them to the AppModel then Add to the list of apps
            foreach (var path in shortcutPaths)
            {
                AppModel app = new();
                app.AppPath = path;
                app.AppIcon = filePath + @$"\{app.AppName}" + ".ico";
                apps.Add(app);
            }
            return apps;
        }

        public static void MakeAppFolder()
        {
            // Get directories in current base directory 
            List<string> dirs = Directory.GetDirectories(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ToList();
            // Check if App folder is avalible, if not then create folder
            foreach (var dir in dirs)
            {
                if (dir.Contains("Apps"))
                {
                    break;
                }
                // If this if statement is triggered it means that no App folder has been found
                if (dirs[dirs.Count - 1] == dir)
                {
                    SendCommand($@"mkdir {GetBaseFilePathForCommands()}\Apps");
                }
            }
        }

        public static void CleanAppsFolder()
        {
            //This is used for cleaning up any icon files that are unassigned to a shortcut.
            //It is neccessary as the icons cannot be deleted when the app is running.
            var filePath = Directory.GetCurrentDirectory() + @"\Apps";
            //Get list of files with only .lnk extension
            List<string> shortcutPaths = Directory.GetFiles(filePath).Where(e => e.Contains(".lnk")).ToList();
            //Get list of files with only .ico extension
            List<string> iconPaths = Directory.GetFiles(filePath).Where(e => e.Contains(".ico")).ToList();

            if (shortcutPaths.Count != iconPaths.Count)
            {
                //if only icon files in folder delete all
                if (shortcutPaths.Count() == 0)
                {
                    SendCommand($"del /f {GetBaseFilePathForCommands() + @"\Apps" + $@"\*.ico"}");
                }
                //if only shorcut files in folder delete all
                else if (iconPaths.Count() == 0)
                {
                    SendCommand($"del /f {GetBaseFilePathForCommands() + @"\Apps" + $@"\*.lnk"}");
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
                            SendCommand($"del /f {GetBaseFilePathForCommands() + @"\Apps" + $@"\{GetAppNameFromPath(iconPath)}.ico"}");
                        }
                    }
                }
            }
        }

        public static void CreateShortcut(string targetPath, string savePath)
        {
            //For some reason apps redirects to an Update.exe file which cannot launch the desired app
            //Therefore apps that do redirect to an Update.exe file must throw an error
            if (targetPath.Contains("Update.exe") is false)
            {
                WshShell shell = new WshShell();

                //Where the shortcut of the app will be saved
                string shortcutAddress = savePath;
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);

                //The path of the app that you want to make into a shortcut
                shortcut.TargetPath = targetPath;
                shortcut.Save();
            }

            else
            {
                throw new("Executable cannot be an update");
            }
        }

        public static void AddToStartup(bool condition, BindingList<AppModel> apps)
        {
            var userProfilePath = GetUserProfilePath();
            var startUpFolderPath = userProfilePath + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";

            // If checkbox is checked
            if (condition is true)
            {
                foreach (var app in apps)
                {
                    var shortcutAppPath = Directory.GetCurrentDirectory() + $@"\Apps\{app.AppName}.lnk";
                    var executableAppPath = GetExePathFromShortcut(shortcutAppPath);
                    var startUpFolderAppPath = startUpFolderPath + $@"\{app.AppName}.lnk";
                    CreateShortcut(executableAppPath, startUpFolderAppPath);
                }
            }

            // If checkbox is empty
            if (condition is false)
            {
                var startUpShortcuts = Directory.GetFiles(startUpFolderPath).ToList();
                foreach (var shortcut in startUpShortcuts)
                {
                    // If app is contained in the shortcut 
                    if (apps.Where(e => shortcut.Contains(e.AppName)).FirstOrDefault() is not null)
                    {
                        var normalizedShortcut = NormalizeFilePath(shortcut);
                        SendCommand($"del /f {normalizedShortcut}");
                    }
                }
            }
        }

        public static bool AreAppsInStartup(BindingList<AppModel> apps)
        {
            // This method determines whether the checkbox should be checked based on the apps in the startup
            var userProfilePath = GetUserProfilePath();
            var startUpFolderPath = userProfilePath + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
            var startUpFilePaths = Directory.GetFiles(startUpFolderPath).ToList();
            var counter = apps.Count;

            // If there is no apps in the startup
            if (counter == 0)
            {
                return false;
            }
            // Remove only from startup the apps reflected in the project apps folder
            foreach (var app in apps)
            {
                foreach (var filePath in startUpFilePaths)
                {
                    if (filePath.Contains(app.AppName))
                    {
                        counter--;
                    }
                }
            }
            // If all apps are reflected in startup folder then return true
            if (counter == 0)
            {
                return true;
            }

            return false;
        }
    }
}
