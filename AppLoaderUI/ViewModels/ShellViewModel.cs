using AppLoaderClassLibrary;
using AppLoaderClassLibrary.Endpoints;
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
        private readonly IAppEndpoint _appEndpoint;
        public ShellViewModel(IAppEndpoint appEndpoint)
        {
            
            _appEndpoint = appEndpoint;
        }
        protected override async void OnViewLoaded(object view)
        {
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
            _appEndpoint.MakeAppFolder();
            _appEndpoint.CleanAppsFolder();
            //App folder needs time to be made before getting list of apps
            await Task.Delay(1000);
            var apps = _appEndpoint.GetListOfApps();
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
            var baseFilePath = _appEndpoint.GetBaseFilePathForCommands();
            foreach (var app in Apps)
            {
                _appEndpoint.SendCommand($@"start {baseFilePath + @"\Apps" + $@"\{app.FileName}.lnk"}");
            }
        }
        public void StartButton()
        {
            var baseFilePath = _appEndpoint.GetBaseFilePathForCommands();
            _appEndpoint.SendCommand($@"start {baseFilePath + @"\Apps" + $@"\{SelectedApp.FileName}.lnk"}");
        }
        public void AddButton()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var appFolderPath = _appEndpoint.GetBaseFilePath() + @$"\Apps\{_appEndpoint.GetAppFromPath(openFileDialog.FileName)}.lnk";
                    _appEndpoint.CreateShortcut(openFileDialog.FileName, appFolderPath);

                    //Creating an icon file with the same naming as the shortcut to display on the UI
                    //Not sure of the reason of the warning, the icon is still created
                    #pragma warning disable CA1416 // Validate platform compatibility
                    System.Drawing.Icon.ExtractAssociatedIcon(openFileDialog.FileName)
                        .ToBitmap()
                        .Save(_appEndpoint.GetBaseFilePath() + $@"\Apps\{_appEndpoint.GetAppFromPath(openFileDialog.FileName)}.ico");
                    #pragma warning restore CA1416 // Validate platform compatibility
                    ErrorMessage = "";
                }
                catch (Exception ex)
                {

                    ErrorMessage = ex.Message;
                }
            }
            //Refresh binding list of apps
            var apps = _appEndpoint.GetListOfApps();
            Apps = new BindingList<AppModel>(apps);
        }

        
        public void DeleteButton(AppModel selectedApp)
        {
            var appName = SelectedApp.FileName;
            Apps.Remove(SelectedApp);
            SelectedApp = null;
            var baseFilePath = _appEndpoint.GetBaseFilePathForCommands();
            _appEndpoint.SendCommand($"del /f {baseFilePath + @"\Apps" + $@"\{appName}*"}");
        }

        public void AddToStartup(bool condition)
        {
            var baseFilePath = _appEndpoint.GetBaseFilePath();
            var baseFilePathCommand = _appEndpoint.GetBaseFilePathForCommands();
            var userProfilePath = _appEndpoint.GetUserProfilePath();
            var startUpFolderPath = userProfilePath + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";
            if (condition is true)
            {
                foreach (var app in Apps)
                {
                    var shortcutAppPath = $@"{baseFilePath + @"\Apps" + $@"\{app.FileName}.lnk"}";
                    var shortcutAppPathCommand = $@"{baseFilePathCommand + @"\Apps" + $@"\{app.FileName}.lnk"}";
                    var executableAppPath = _appEndpoint.GetShortcutTargetFile(shortcutAppPath);
                    var startUpFolderAppPath = startUpFolderPath + $@"\{app.FileName}.lnk";
                    _appEndpoint.CreateShortcut(executableAppPath, startUpFolderAppPath);
                }
            }
            if (condition is false)
            {
                var startUpShortcuts = Directory.GetFiles(startUpFolderPath).ToList();
                startUpShortcuts = startUpShortcuts.Where(e => !e.Contains("desktop.ini")).ToList();
                foreach (var shortcut in startUpShortcuts)
                {
                    if (Apps.Where(e => shortcut.Contains(e.FileName)).FirstOrDefault() is not null)
                    {
                        var normalizedShortcut = _appEndpoint.NormalizeFilePath(shortcut);
                        _appEndpoint.SendCommand($"del /f {normalizedShortcut}");
                    }
                }

            }
        }

        public bool AreAppsInStartup()
        {
            var userProfilePath = _appEndpoint.GetUserProfilePath();
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
                    if (filePath.Contains(app.FileName))
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