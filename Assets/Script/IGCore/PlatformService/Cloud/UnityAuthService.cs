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

        public event Action EventOnPlayerAccountSignedIn;
        public event Action EventOnPlayerAccountSignInFailed;
        public event Action EventOnPlayerAccountSignedOut;


        string playerId;
        public string PlayerId => playerId;

        IService Service => service as IService;

        bool isInitialized = false;
        bool isConnected = false;
        public bool IsConnected => isConnected;

        bool isLinkingAccount = false;

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

            Debug.Log("[Auth] Auth Sign Events have been registered successfully.");

            isInitialized = true;

            while(PlayerAccountService.Instance == null)
                await Task.Delay(1000);

            PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;
            PlayerAccountService.Instance.SignInFailed += OnPlayerAccountSignInFailed;
            PlayerAccountService.Instance.SignedOut += OnPlayerAccountSignedOut;

            Debug.Log("[Auth] Auth Player Sign Events have been registered successfully.");
        }

        private void OnDestroy()
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
            AuthenticationService.Instance.SignedOut -= OnSignedOut;
            AuthenticationService.Instance.Expired -= OnExpired;

            PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn;
            PlayerAccountService.Instance.SignInFailed -= OnPlayerAccountSignInFailed;
            PlayerAccountService.Instance.SignedOut -= OnPlayerAccountSignedOut;
        }


        #region Interfaces

        public async Task SignInAsync()             // EventOnSignedIn or EventOnSignInFailed
        {
            while(!isInitialized)
                await Task.Delay(1000);

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[Auth] Loop has been canceled!");
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
        }

        
        public async Task PlayerSignInAsync()       // EventOnPlayAccountSignedIn or EventOnPlayAccountSignInFailed.
        {
            try
            {
                if(PlayerAccountService.Instance!=null && PlayerAccountService.Instance.IsSignedIn)
                    PlayerAccountService.Instance.SignOut();

                await PlayerAccountService.Instance.StartSignInAsync();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }


        public async Task LinkAccountWithPlayer()   // EventOnLinkAccount(bool)
        {
            try
            {
                isLinkingAccount = true;

                if(false == PlayerAccountService.Instance.IsSignedIn)
                    await PlayerAccountService.Instance.StartSignInAsync();
                else 
                    OnPlayerAccountSignedIn();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }


        public async Task<bool> UnlinkAccountWithPlayer()
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
        
        
        public void SignOut()
        {
            AuthenticationService.Instance.SignOut();
            if(PlayerAccountService.Instance.IsSignedIn)
                PlayerAccountService.Instance.SignOut();

            AuthenticationService.Instance.ClearSessionToken();

            Debug.Log("<color=red>Signning out.... Session Token has been removed successfully!</color>");
        }
        
        
        public bool IsAccountLinkedWithPlayer(string playerId)
        {
            if(!IsConnected)    return false;

            if(AuthenticationService.Instance==null || AuthenticationService.Instance.PlayerInfo==null)
                return false;

            var identities = AuthenticationService.Instance.PlayerInfo.Identities;
            for(int q = 0; q < identities.Count; ++q)
            {
                if(identities[q].TypeId == playerId)
                    return true;
            }
            return false;
        }


        public string GetManagementURL()
        {
            // Application.OpenURL( PlayerAccountService.Instance.AccountPortalUrl );
            return PlayerAccountService.Instance.AccountPortalUrl;
        }


        #endregion








        #region Events.

        
        async void OnPlayerAccountSignedIn()
        {
            EventOnPlayerAccountSignedIn?.Invoke();

            try
            {
                string pasAccessToken = PlayerAccountService.Instance.AccessToken;
                if (string.IsNullOrEmpty(pasAccessToken))
                {
                    Debug.LogError("[Auth] Fetching PAS Token has been failed..");
                    if(isLinkingAccount)
                        EventOnLinkAccount?.Invoke(false);

                    isLinkingAccount = false;
                    return;
                }
                Debug.Log($"[Auth] Unity Web Sign In has been successed. PAS AccessToken : {pasAccessToken}");
                    
                if(isLinkingAccount)
                {
                    await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                    Debug.Log("[Auth] Unity Account Link has been successed.");
                    EventOnLinkAccount?.Invoke(true);
                }
                else
                    await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                
                //var identities = AuthenticationService.Instance.PlayerInfo.Identities;
                //for(int q = 0; q < identities.Count; ++q)
                //    Debug.Log($"[Auth] identity : {identities[q].TypeId}.");
            }
            catch(Exception e) 
            {
                Debug.LogException(e);
                if(isLinkingAccount)
                    EventOnLinkAccount?.Invoke(false);
            }

            isLinkingAccount = false;
        }

        void OnPlayerAccountSignedOut()
        { 
            EventOnPlayerAccountSignedOut?.Invoke();
        }
        
        void OnPlayerAccountSignInFailed(RequestFailedException e)
        {
            EventOnPlayerAccountSignInFailed?.Invoke();
            
            if(isLinkingAccount) 
                EventOnLinkAccount?.Invoke(false);

            isLinkingAccount = false;
        }

        void OnSignedIn()
        {
            playerId = AuthenticationService.Instance.PlayerId;
            
            Debug.Log($"<color=green>[Auth][PlayerId] [{playerId}] has logined in successfully.</color>");
            Debug.Log($"<color=green>[Auth][PlayerName] [{AuthenticationService.Instance.PlayerName}].</color>");
            Debug.Log($"<color=green>[Auth][AccessToken] [{AuthenticationService.Instance.AccessToken}].</color>");

            isConnected = true;
            EventOnSignedIn?.Invoke(playerId);
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
            
            EventOnSignOut?.Invoke();
        }

        void OnExpired()
        {
            Debug.Log("[Auth] Session has been expired.");
            EventOnSessionExpired?.Invoke();
        }

        #endregion









        

        async Task LoginLoop()
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
    }
}
