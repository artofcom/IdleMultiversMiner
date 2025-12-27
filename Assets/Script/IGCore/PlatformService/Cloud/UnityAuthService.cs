using UnityEngine;
using System;
using UnityEngine.Assertions;
using System.Threading.Tasks;
using Unity.Services.Authentication;
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


        string playerId;
        public string PlayerId => playerId;

        IService Service => service as IService;

        bool isConnected = false;
        public bool IsConnected => isConnected;

        async void Awake()
        {
            Assert.IsNotNull(service);

            while(!Service.IsInitialized())
                await Task.Delay(1000);

            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;
            AuthenticationService.Instance.SignedOut += OnSignedOut;
            AuthenticationService.Instance.Expired += OnExpired;
            
            await TrySignIn();
        }

        private void OnDestroy()
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
            AuthenticationService.Instance.SignedOut -= OnSignedOut;
            AuthenticationService.Instance.Expired -= OnExpired;
        }

        async Task TrySignIn()
        {
            var waitSec = new WaitForSeconds(retryInterval);
            
            while(IsConnected == false)
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
                    Debug.Log("[UnityService] Loop has been canceled!");
                    break;
                }
                catch(Exception e) 
                {
                    Debug.LogException(e);   
                }

                await Task.Delay(retryInterval * 1000);
            }
        }



        #region Events.

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
    }
}
