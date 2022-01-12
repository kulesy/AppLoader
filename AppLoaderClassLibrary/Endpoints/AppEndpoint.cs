using System;
using System.Collections.Generic;
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
        }
        public List<string> GetListOfApps()
        {
            var filePath = Tools.GetBaseFilePath() + @"\Apps";
            List<string> filePaths = Directory.GetFiles(filePath).ToList();
            List<string> appNames = new();
            foreach(var path in filePaths)
            {
                var appName = path.Split(@"\").Last();
                appNames.Add(appName);
            }
            return appNames;
        }
    }
}
