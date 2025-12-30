using UnityEngine;
using System;
using UnityEngine.Assertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using IGCore.PlatformService.Util;

namespace IGCore.PlatformService.Cloud
{
    public class UnityCloudService : MonoBehaviour, ICloudService
    {
        [ImplementsInterface(typeof(IService))]
        [SerializeField] MonoBehaviour service;
        [ImplementsInterface(typeof(IAuthService))]
        [SerializeField] MonoBehaviour authService;

        [SerializeField] bool compressData = true;

        // public event Action<string> EventOnSignedIn;
        
        IService Service => service as IService;
        IAuthService AuthService => authService as IAuthService;

        bool isInitialized = false;
        public bool IsInitialized => isInitialized;

        async void Awake()
        {
            Assert.IsNotNull(service);

            await InitAsync();
        }

        async Task InitAsync()
        {
            while(!Service.IsInitialized() || !AuthService.IsSignedIn() || CloudSaveService.Instance==null)
                await Task.Delay(1000);

            Debug.Log("[Cloud] Cloud Service has been registered successfully.");
            isInitialized = true;
        }

        private void OnDestroy()
        {
            //AuthenticationService.Instance.SignedIn -= OnSignedIn;
        }


        #region Interfaces

        // Error Message.
        public async Task<string> SaveUserData(string key, string userData) 
        { 
            if(!isInitialized)
                return "[Cloud] SaveUserData has been failed due to the unfinished initialization.";

            string encodedString = userData;
            if(compressData)
            {
                encodedString = StringCompressor.CompressToEncodedString(userData);
                Debug.Log($"[Cloud] Original Size {userData.Length} -> Compressed Size {encodedString.Length}");
            }

            var data = new Dictionary<string, object> { {key, encodedString } };
            try
            { 
                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                return string.Empty;
            }
            //catch (CloudSaveValidationException ex) { }
            //catch (CloudSaveRateLimitedException ex) { }
            //catch (CloudSaveConflictException ex) { }
            //catch (CloudSaveException ex) { }
            catch (Exception ex) 
            {
                Debug.LogException(ex);
                return ex.Message;
            }
        }

        // UserData.
        public async Task<string> LoadUserData(string key) 
        {
            if(!isInitialized)
                return string.Empty;

            try
            {
                var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>{{key}});

                if(data.TryGetValue(key, out var value))
                {
                    string originalJson = compressData ? StringCompressor.DecompressFromEncodedString(value.Value.GetAsString()) : value.Value.GetAsString();
                    Debug.Log($"[Cloud] Fetching the UserData has been successed. : {originalJson}");
                    return originalJson;
                }
            }
            catch(Exception ex) 
            {
                Debug.LogException(ex);
            }
            return string.Empty;
        }

        #endregion








        #region Events.


        #endregion



    }
        
}
