using System;
using System.Threading.Tasks;

namespace IGCore.PlatformService.Cloud
{
    public interface ICloudService 
    {
        // event Action<string> EventOnSignedIn;
        
        // Ret : string - Error Message.
        Task<string> SaveUserData(string key, string jsonString);
        
        // Ret : string - data.
        Task<string> LoadUserData(string key);
    }
}
