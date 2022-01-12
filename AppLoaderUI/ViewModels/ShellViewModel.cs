using AppLoaderClassLibrary;
using AppLoaderClassLibrary.Endpoints;
using Caliburn.Micro;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLoaderUI.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        public ShellViewModel(IAppEndpoint appEndpoint)
        {
            
            _appEndpoint = appEndpoint;
        }

        protected override async void OnViewLoaded(object view)
        {
            await LoadApps();
        }

        public async Task LoadApps()
        {
            _appEndpoint.MakeAppFolder();
            await Task.Delay(1000);
            var apps = _appEndpoint.GetListOfApps();
            Apps = new BindingList<string>(apps);
        }
        private BindingList<string> _apps = new();
        private readonly IAppEndpoint _appEndpoint;

        public BindingList<string> Apps
        {
            get { return _apps; }
            set 
            { 
                _apps = value;
                NotifyOfPropertyChange(() => Apps);
            }
        }

        public void UploadButton()
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Tools.CreateShortcut(openFileDialog.FileName);
            }
            var apps = _appEndpoint.GetListOfApps();
            Apps = new BindingList<string>(apps);
        }

        public void StartButton()
        {
            foreach (var app in Apps)
            {
                Tools.SendCommand(@$"start Apps\{app}");
            }
        }
    }
}
