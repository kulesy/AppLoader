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
                if (SelectedApp is not null)
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
                if (SelectedApp is not null)
                {
                    output = true;
                }
                return output;
            }
        }

        public async Task LoadApps()
        {
            _appEndpoint.CleanAppsFolder();
            _appEndpoint.MakeAppFolder();
            //App folder needs time to be made before getting list of apps
            await Task.Delay(1000);
            var apps = _appEndpoint.GetListOfApps();
            Apps = new BindingList<AppModel>(apps);
        }

        public void StartAllButton()
        {
            var baseFilePath = _appEndpoint.GetBaseFilePathForCommands();
            foreach (var app in Apps)
            {
                _appEndpoint.SendCommand($@"start {baseFilePath + "Apps" + $@"\{app.FileName}.lnk"}");
            }
        }
        public void StartButton()
        {
            var baseFilePath = _appEndpoint.GetBaseFilePathForCommands();
            _appEndpoint.SendCommand($@"start {baseFilePath + "Apps" + $@"\{SelectedApp.FileName}.lnk"}");
        }
        public void AddButton()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _appEndpoint.CreateShortcut(openFileDialog.FileName);
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
            _appEndpoint.SendCommand($"del /f {baseFilePath + "Apps" + $@"\{appName}*"}");
        }
    }
}
