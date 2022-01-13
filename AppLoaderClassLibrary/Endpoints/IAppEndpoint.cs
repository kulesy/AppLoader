using AppLoaderClassLibrary.Models;
using System.Collections.Generic;

namespace AppLoaderClassLibrary.Endpoints
{
    public interface IAppEndpoint
    {
        void CleanAppsFolder();
        void CreateShortcut(string appFilePath);
        string GetAppFromPath(string path);
        string GetBaseFilePath();
        string GetBaseFilePathForCommands();
        List<AppModel> GetListOfApps();
        void MakeAppFolder();
        void SendCommand(string command);
    }
}