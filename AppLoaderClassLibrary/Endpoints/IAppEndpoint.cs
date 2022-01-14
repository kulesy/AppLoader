using AppLoaderClassLibrary.Models;
using System.Collections.Generic;

namespace AppLoaderClassLibrary.Endpoints
{
    public interface IAppEndpoint
    {
        void CleanAppsFolder();
        void CreateShortcut(string targetPath, string savePath);
        string GetAppFromPath(string path);
        string GetBaseFilePath();
        string GetBaseFilePathForCommands();
        List<AppModel> GetListOfApps();
        string GetShortcutTargetFile(string shortcutFilename);
        string GetUserProfilePath();
        void MakeAppFolder();
        string NormalizeFilePath(string filePath);
        void SendCommand(string command);
    }
}