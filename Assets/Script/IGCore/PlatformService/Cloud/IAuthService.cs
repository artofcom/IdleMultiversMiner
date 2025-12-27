using System;

namespace IGCore.PlatformService.Cloud
{
    public interface IAuthService 
    {
        event Action<string> EventOnSignedIn;
        event Action<string> EventOnSignInFailed;
        event Action EventOnSignOut;
        event Action EventOnSessionExpired;
    }
}
