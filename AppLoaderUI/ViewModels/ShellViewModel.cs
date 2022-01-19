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
                Tools.AddToStartup(value, Apps);
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

        public async Task LoadApps()
        {
            Tools.MakeAppFolder();
            Tools.CleanAppsFolder();
            //App folder needs time to be made before getting list of apps
            await Task.Delay(1000);
            var apps = Tools.GetListOfApps();
            Apps = new BindingList<AppModel>(apps);

            if (Tools.AreAppsInStartup(Apps) is true)
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
            var returnMessage = FileDialogApp.OpenFileDialogApp(Apps);
            if (returnMessage is null)
            {
                var apps = Tools.GetListOfApps();
                Apps = new BindingList<AppModel>(apps);
            }
            else
            {
                ErrorMessage = returnMessage;
            }
        }

        
        public void DeleteButton()
        {
            var appName = SelectedApp.AppName;
            Apps.Remove(SelectedApp);
            SelectedApp = null;
            var baseFilePath = Tools.GetBaseFilePathForCommands();
            Tools.SendCommand($"del /f {baseFilePath + @"\Apps" + $@"\{appName}*"}");
        }
    }
}