using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLoaderClassLibrary.Models
{
    public class AppModel
    {
        public string AppName
        {
            get { return GetAppName(); }
        }
        public string AppPath { get; set; }
        public string AppIcon { get; set; }


        private string GetAppName()
        {
            if (AppPath is null)
            {
                return "unknown";
            }
            var appNameWithExtension = AppPath.Split(@"\").Last();
            var appName = appNameWithExtension.Remove(appNameWithExtension.Count() - 4);
            return appName;
        }
    }
}
