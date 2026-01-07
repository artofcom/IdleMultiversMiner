using System;
using System.Threading.Tasks;

namespace IGCore.PlatformService.Cloud
{
    public interface ICloudService 
    {
        public enum ResultType
        {
            eSuccessed, eServiceNotInitialized, eInvalidAuth, eDataNotFound, eNoNetworkConnection, eInvalidProjectId, eUnknownError
        }

        event Action EventOnInitialized;

        // event Action<string> EventOnSignedIn;
        bool IsInitialized();

        // Ret : isSuccessed , Error Message.
        Task<Tuple<ResultType, string>> SaveUserData(string key, string jsonString);
        
        // Ret : isSuccessed, string data.
        Task<Tuple<ResultType, string>> LoadUserData(string key);
    }
}
