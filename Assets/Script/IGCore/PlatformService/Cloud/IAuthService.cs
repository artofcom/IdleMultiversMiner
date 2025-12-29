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

        event Action<bool, string> EventOnLinkAccount;

        event Action EventOnPlayerAccountSignedIn;
        event Action EventOnPlayerAccountSignInFailed;
        event Action EventOnPlayerAccountSignedOut;

        bool IsSignedIn();
        Task SignInAsync();
        Task PlayerSignInAsync();
        Task LinkAccountWithPlayer();
        Task<bool> UnlinkAccountWithPlayer();
        void SignOut();
        bool IsAccountLinkedWithPlayer(string playerId);
        string GetManagementURL();
        
    }
}
