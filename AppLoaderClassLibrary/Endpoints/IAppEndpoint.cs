using System.Collections.Generic;

namespace AppLoaderClassLibrary.Endpoints
{
    public interface IAppEndpoint
    {
        List<string> GetListOfApps();
    }
}