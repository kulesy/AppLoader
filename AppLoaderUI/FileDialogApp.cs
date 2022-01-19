using AppLoaderClassLibrary.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLoaderClassLibrary
{
    public static class FileDialogApp
    {
        public static string OpenFileDialogApp(BindingList<AppModel> apps)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog.Title = "Open Image Files";

            if (openFileDialog.ShowDialog() == true)
            {
                var appName = Tools.GetAppNameFromPath(openFileDialog.FileName);
                if (appName.Contains("Update"))
                {
                    return "App's executable file cannot be Update.exe";
                }
                try
                {   // Check if app exists in Apps Folder
                    if (apps.Where(e => e.AppName == appName).FirstOrDefault() is null)
                    {
                        var appFolderPath = Directory.GetCurrentDirectory() + @$"\Apps\{appName}.lnk";
                        Tools.CreateShortcut(openFileDialog.FileName, appFolderPath);

                        //Creating an icon file with the same naming as the shortcut to display on the UI
                        //Not sure of the reason of the warning, the icon is still created
                        #pragma warning disable CA1416 // Validate platform compatibility
                        System.Drawing.Icon.ExtractAssociatedIcon(openFileDialog.FileName)
                            .ToBitmap()
                            .Save(Directory.GetCurrentDirectory() + $@"\Apps\{appName}.ico");
                        #pragma warning restore CA1416 // Validate platform compatibility
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            return null;
        }
    }
}
