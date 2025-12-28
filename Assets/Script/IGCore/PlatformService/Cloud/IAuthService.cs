using System;
using System.Threading.Tasks;

namespace IGCore.PlatformService.Cloud
{
    public interface IAuthService 
    {
        event Action<string> EventOnSignedIn;
        event Action<string> EventOnSignInFailed;
        event Action EventOnSignOut;
        event Action EventOnSessionExpired;
        event Action<bool> EventOnLinkAccount;

        Task SignIn();
        Task LinkAccountWithPlatform();
        Task<bool> UnlinkAccountWithPlatform();
        void SignOut();
        bool IsAccountLinkedWithIdentity(string identityName);
        
    }
}
