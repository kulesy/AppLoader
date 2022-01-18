using AppLoaderClassLibrary;
using AppLoaderClassLibrary.Models;
using Caliburn.Micro;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AppLoaderUI.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        protected override async void OnViewLoaded(object view)
        {
            // Setting directory to documents folder
            string appLoaderFolder = Tools.GetAppLoaderPath();
            Directory.SetCurrentDirectory(appLoaderFolder);
            await LoadApps();
        }

        private BindingList<AppModel> _apps = new();
        public BindingList<AppModel> Apps
        {
            get { return _apps; }
            set 
            { 
                _apps = value;
                NotifyOfPropertyChange(() => Apps);
            }
        }

        private AppModel _selectedApp;
        public AppModel SelectedApp
        {
            get { return _selectedApp; }
            set 
            { 
                _selectedApp = value;
                NotifyOfPropertyChange(() => SelectedApp);
                NotifyOfPropertyChange(() => CanDeleteButton);
                NotifyOfPropertyChange(() => CanStartButton);
            }
        }

        private bool _isOnStartup;
        public bool IsOnStartup
        {
            get { return _isOnStartup; }
            set
            {
                _isOnStartup = value;
                NotifyOfPropertyChange(() => IsOnStartup);
                NotifyOfPropertyChange(() => CanDeleteButton);
                NotifyOfPropertyChange(() => CanStartButton);
                AddToStartup(value);
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { 
                    _errorMessage = value;
                    NotifyOfPropertyChange(() => IsErrorVisible);
                    NotifyOfPropertyChange(() => ErrorMessage);
                }
        }

        public bool IsErrorVisible 
        {
            get 
            {
                bool output = false;
                if (ErrorMessage?.Count() > 0)
                {
                    output = true;
                }
                return output;
            }
        }

        public bool CanDeleteButton
        {
            get
            {
                bool output = false;
                if (SelectedApp is not null || IsOnStartup == false)
                {
                    output = true;
                }
                return output;
            }
        }

        public bool CanStartButton
        {
            get
            {
                bool output = false;
                if (SelectedApp is not null || IsOnStartup == false)
                {
                    output = true;
                }
                return output;
            }
        }

        

        public async Task LoadApps()
        {
            Tools.MakeAppFolder();
            Tools.CleanAppsFolder();
            //App folder needs time to be made before getting list of apps
            await Task.Delay(1000);
            var apps = Tools.GetListOfApps();
            Apps = new BindingList<AppModel>(apps);
            if (AreAppsInStartup() is true)
            {
                IsOnStartup = true;
            }
            else
            {
                IsOnStartup = false;
            }
        }

        public void StartAllButton()
        {
            foreach (var app in Apps)
            {
                Tools.SendCommand($@"start Apps\{app.AppName}.lnk");
            }
        }
        public void StartButton()
        {
            var baseFilePath = Tools.GetBaseFilePathForCommands();
            Tools.SendCommand($@"start {baseFilePath + @"\Apps" + $@"\{SelectedApp.AppName}.lnk"}");
        }
        public void AddButton()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                var appName = Tools.GetAppFromPath(openFileDialog.FileName);
                try
                {   // Check if app exists in Apps Folder
                    if (Apps.Where(e => e.AppName == appName).FirstOrDefault() is null)
                    {
                        var appFolderPath =  Directory.GetCurrentDirectory() + @$"\Apps\{appName}.lnk";
                        Tools.CreateShortcut(openFileDialog.FileName, appFolderPath);

                        //Creating an icon file with the same naming as the shortcut to display on the UI
                        //Not sure of the reason of the warning, the icon is still created
                        #pragma warning disable CA1416 // Validate platform compatibility
                        System.Drawing.Icon.ExtractAssociatedIcon(openFileDialog.FileName)
                            .ToBitmap()
                            .Save(Directory.GetCurrentDirectory()+ $@"\Apps\{appName}.ico");
                        #pragma warning restore CA1416 // Validate platform compatibility
                        ErrorMessage = "";
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("A generic error") is false)
                    {
                        ErrorMessage = ex.Message;
                    }
                }
            }
            //Refresh binding list of apps
            var apps = Tools.GetListOfApps();
            Apps = new BindingList<AppModel>(apps);
        }

        
        public void DeleteButton()
        {
            var appName = SelectedApp.AppName;
            Apps.Remove(SelectedApp);
            SelectedApp = null;
            var baseFilePath = Tools.GetBaseFilePathForCommands();
            Tools.SendCommand($"del /f {baseFilePath + @"\Apps" + $@"\{appName}*"}");
        }

        public void AddToStartup(bool condition)
        {
            var userProfilePath = Tools.GetUserProfilePath();
            var startUpFolderPath = userProfilePath + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
            if (condition is true)
            {
                foreach (var app in Apps)
                {
                    var shortcutAppPath = Directory.GetCurrentDirectory()+ $@"\Apps\{app.AppName}.lnk";
                    var executableAppPath = Tools.GetShortcutTargetFile(shortcutAppPath);
                    var startUpFolderAppPath = startUpFolderPath + $@"\{app.AppName}.lnk";
                    Tools.CreateShortcut(executableAppPath, startUpFolderAppPath);
                }
            }
            if (condition is false)
            {
                var startUpShortcuts = Directory.GetFiles(startUpFolderPath).ToList();
                startUpShortcuts = startUpShortcuts.Where(e => !e.Contains("desktop.ini")).ToList();
                foreach (var shortcut in startUpShortcuts)
                {
                    if (Apps.Where(e => shortcut.Contains(e.AppName)).FirstOrDefault() is not null)
                    {
                        var normalizedShortcut = Tools.NormalizeFilePath(shortcut);
                        Tools.SendCommand($"del /f {normalizedShortcut}");
                    }
                }

            }
        }

        public bool AreAppsInStartup()
        {
            var userProfilePath = Tools.GetUserProfilePath();
            var startUpFolderPath = userProfilePath + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
            var startUpFilePaths = Directory.GetFiles(startUpFolderPath).ToList();
            var counter = startUpFilePaths.Count - 1;
            if (counter == 0)
            {
                return false;
            }
            foreach (var app in Apps)
            {
                foreach(var filePath in startUpFilePaths)
                {
                    if (filePath.Contains(app.AppName))
                    {
                        counter--;
                    }
                }
            }
            if (counter == 0)
            {
                return true;
            }
            return false;
        }
    }
}

//https://stackoverflow.com/questions/9414152/get-target-of-shortcut-folder