using System;
using System.Threading.Tasks;

namespace IGCore.PlatformService.Cloud
{
    public interface ICloudService 
    {
        // event Action<string> EventOnSignedIn;
        bool IsInitialized();

        // Ret : isSuccessed , Error Message.
        Task<Tuple<bool, string>> SaveUserData(string key, string jsonString);
        
        // Ret : isSuccessed, string data.
        Task<Tuple<bool, string>> LoadUserData(string key);
    }
}
