using AppLoaderClassLibrary.Models;
using System.Collections.Generic;

namespace AppLoaderClassLibrary.Endpoints
{
    public interface IAppEndpoint
    {
        void CleanAppsFolder();
        List<AppModel> GetListOfApps();
        void MakeAppFolder();
    }
}