using UnityEngine;
using System;
using UnityEngine.Assertions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;

namespace IGCore.PlatformService.Cloud
{
    public class UnityAuthService : MonoBehaviour, IAuthService
    {
        [ImplementsInterface(typeof(IService))]
        [SerializeField] MonoBehaviour service;

        [SerializeField] int retryInterval = 5;



        public event Action<string> EventOnSignedIn;
        public event Action<string> EventOnSignInFailed;
        public event Action EventOnSignOut;
        public event Action EventOnSessionExpired;
        public event Action<bool> EventOnLinkAccount;

        string playerId;
        public string PlayerId => playerId;

        IService Service => service as IService;

        bool isInitialized = false;
        bool isConnected = false;
        public bool IsConnected => isConnected;

        async void Awake()
        {
            Assert.IsNotNull(service);

            await InitAsync();
        }

        async Task InitAsync()
        {
            while(!Service.IsInitialized())
                await Task.Delay(1000);

            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;
            AuthenticationService.Instance.SignedOut += OnSignedOut;
            AuthenticationService.Instance.Expired += OnExpired;

            isInitialized = true;
        }

        private void OnDestroy()
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
            AuthenticationService.Instance.SignedOut -= OnSignedOut;
            AuthenticationService.Instance.Expired -= OnExpired;
        }

        public async Task SignIn()
        {
            isConnected = false;

            while(!isInitialized)
                await Task.Delay(1000);

            var waitSec = new WaitForSeconds(retryInterval);
            
            while(false==IsConnected && false==destroyCancellationToken.IsCancellationRequested)
            {
                if(Application.internetReachability == NetworkReachability.NotReachable)
                {
                    await Task.Delay(retryInterval * 1000 * 5);
                    continue;
                }
                
                try
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("[Auth] Loop has been canceled!");
                    break;
                }
                catch(RequestFailedException ex)  
                {
                    Debug.LogException(ex);   

                    if(ex.ErrorCode == 401 || ex.ErrorCode == 403)
                    {
                        if (AuthenticationService.Instance.SessionTokenExists)
                        {
                            AuthenticationService.Instance.ClearSessionToken();
                            Debug.Log("[Auth] Request has ben failed - Resetting sesstion token...");
                        }
                    }
                    else if(ex.ErrorCode == 0 || ex.ErrorCode >= 500)
                        Debug.Log("[Auth] Request has ben failed due to the unstable connection.");
                }
                catch(Exception e) 
                {
                    Debug.LogException(e);
                }

                await Task.Delay(retryInterval * 1000);
            }
        }


        public async Task LinkAccountWithPlatform()
        {
            try
            {
                if(false == PlayerAccountService.Instance.IsSignedIn)
                    await PlayerAccountService.Instance.StartSignInAsync();
                else 
                    OnPlayerAccountSignedIn();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                OnPlayerAccountSignInFailed(null);
            }
        }

        public void SignOut()
        {
            AuthenticationService.Instance.SignOut();
            if(PlayerAccountService.Instance.IsSignedIn)
                PlayerAccountService.Instance.SignOut();

            AuthenticationService.Instance.ClearSessionToken();

            Debug.Log("<color=red>Signning out.... Session Token has been removed successfully!</color>");
        }

        async Task TryLinkAccount()
        {
            PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;
            PlayerAccountService.Instance.SignInFailed += OnPlayerAccountSignInFailed;

            try
            {
                await PlayerAccountService.Instance.StartSignInAsync();
                
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn;

                EventOnLinkAccount?.Invoke(false);
            }
        }
        async Task TryLoginAccount()
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
        }

        

        public bool IsAccountLinkedWithIdentity(string identity)
        {
            if(!IsConnected)    return false;

            if(AuthenticationService.Instance==null || AuthenticationService.Instance.PlayerInfo==null)
                return false;

            var identities = AuthenticationService.Instance.PlayerInfo.Identities;
            for(int q = 0; q < identities.Count; ++q)
            {
                if(identities[q].TypeId == identity)
                    return true;
            }
            return false;
        }

        public async Task<bool> UnlinkAccountWithPlatform()
        {
            try
            {
                await AuthenticationService.Instance.UnlinkUnityAsync();
                Debug.Log("[Auth] Unlink with Unity has been successed.");

                return true;
            }
            catch(Exception e) 
            {
                Debug.LogException(e);

                return false;
            }
        }
        





        #region Events.

        void OnPlayerAccountSignInFailed(RequestFailedException e)
        {
            EventOnLinkAccount?.Invoke(false);
        }

        async void OnPlayerAccountSignedIn()
        {
            try
            {
                string pasAccessToken = PlayerAccountService.Instance.AccessToken;
                if (string.IsNullOrEmpty(pasAccessToken))
                {
                    Debug.LogError("[Auth] Fetching PAS Token has been failed..");
                    EventOnLinkAccount?.Invoke(false);
                    return;
                }
                Debug.Log($"[Auth] Unity Web Sign In has been successed. PAS AccessToken : {pasAccessToken}");
                await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);

                Debug.Log("[Auth] Unity Account Link has been successed.");
                var identities = AuthenticationService.Instance.PlayerInfo.Identities;
                for(int q = 0; q < identities.Count; ++q)
                    Debug.Log($"[Auth] identity : {identities[q].TypeId}.");

                EventOnLinkAccount?.Invoke(true);
            }
            catch(Exception e) 
            {
                Debug.LogException(e);
                EventOnLinkAccount?.Invoke(false);
            }
        }

        void OnSignedIn()
        {
            playerId = AuthenticationService.Instance.PlayerId;
            
            Debug.Log($"<color=green>[Auth][PlayerId] [{playerId}] has logined in successfully.</color>");
            Debug.Log($"<color=green>[Auth][PlayerName] [{AuthenticationService.Instance.PlayerName}].</color>");
            Debug.Log($"<color=green>[Auth][AccessToken] [{AuthenticationService.Instance.AccessToken}].</color>");

            isConnected = true;
            EventOnSignedIn?.Invoke(playerId);

            PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;
            PlayerAccountService.Instance.SignInFailed += OnPlayerAccountSignInFailed;
        }

        void OnSignInFailed(RequestFailedException reqExp)
        {
            Debug.LogWarning(reqExp.Message);
            EventOnSignInFailed?.Invoke(reqExp.Message);
        }

        void OnSignedOut()
        {
            playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"[Auth] Anonym [{playerId}] has been signed out successfully.");
            
            PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn;
            PlayerAccountService.Instance.SignInFailed -= OnPlayerAccountSignInFailed;

            EventOnSignOut?.Invoke();
        }

        void OnExpired()
        {
            Debug.Log("[Auth] Session has been expired.");
            EventOnSessionExpired?.Invoke();
        }

        #endregion
    }
}
