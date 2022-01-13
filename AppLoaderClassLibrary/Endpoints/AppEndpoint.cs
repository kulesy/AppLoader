using AppLoaderClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            List<string> dirs = Directory.GetDirectories(Tools.GetBaseFilePath()).ToList();
            foreach (var dir in dirs)
            {
                if (dir == "Apps")
                {
                    break;
                }
                // If this if statement is triggered it means that no App folder has been found
                if (dirs[dirs.Count - 1] == dir)
                {
                    Tools.SendCommand($"mkdir Apps");
                }
            }
        }
        public List<AppModel> GetListOfApps()
        {
            var filePath = Tools.GetBaseFilePath() + @"Apps";
            //Get list of files with only .lnk extension
            List<string> shortcutPaths = Directory.GetFiles(filePath).Where(e => e.Contains(".lnk")).ToList();
            List<AppModel> apps = new();
            //Sort through each file path and map them to the AppModel then Add to the list of apps
            foreach(var path in shortcutPaths)
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
            var filePath = Tools.GetBaseFilePath() + @"Apps";
            //Get list of files with only .lnk extension
            List<string> shortcutPaths = Directory.GetFiles(filePath).Where(e => e.Contains(".lnk")).ToList();
            //Get list of files with only .ico extension
            List<string> iconPaths = Directory.GetFiles(filePath).Where(e => e.Contains(".ico")).ToList();
            if (shortcutPaths.Count != iconPaths.Count)
            {
                //if only icon files in folder delete all
                if (shortcutPaths.Count() == 0)
                {
                    Tools.SendCommand($"del /f {Tools.GetBaseFilePathForCommands() + "Apps" + $@"\*.ico"}");
                }
                //if only shorcut files in folder delete all
                else if (iconPaths.Count() == 0)
                {
                    Tools.SendCommand($"del /f {Tools.GetBaseFilePathForCommands() + "Apps" + $@"\*.lnk"}");
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
                            Tools.SendCommand($"del /f {Tools.GetBaseFilePathForCommands() + "Apps" + $@"\{Tools.GetAppFromPath(iconPath)}.ico"}");
                        }
                    }
                }
            }
        }
    }
}
