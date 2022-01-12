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
            List<string> dirs = Directory.GetDirectories(Tools.GetBaseFilePath()).ToList();
            foreach (var dir in dirs)
            {
                if (dir == "Apps")
                {
                    break;
                }
                if (dirs[dirs.Count - 1] == dir)
                {
                    Tools.SendCommand($"mkdir Apps");
                }
            }
            _appEndpoint = appEndpoint;
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
