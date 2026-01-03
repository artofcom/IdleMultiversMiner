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

        async void Awake()
        {
            Assert.IsNotNull(service);

            await InitAsync();
        }

        async Task InitAsync()
        {
            while(!Service.IsInitialized() || !AuthService.IsSignedIn() || CloudSaveService.Instance==null)
                await Task.Delay(500);

            Debug.Log("[Cloud] Cloud Service has been registered successfully.");
            isInitialized = true;
        }

        private void OnDestroy() {}


        #region Interfaces

        public bool IsInitialized()
        {
            return isInitialized;
        }

        // Error Message.
        public async Task<Tuple<bool, string>> SaveUserData(string key, string userData) 
        { 
            try
            { 
                if(!isInitialized)
                    throw new Exception("[Cloud] Module Not initialized.");
                
                string encodedString = userData;
                if(compressData)
                {
                    encodedString = StringCompressor.CompressToEncodedString(userData);
                    Debug.Log($"[Cloud] Original Size {userData.Length} -> Compressed Size {encodedString.Length}");
                }

                var data = new Dictionary<string, object> { {key, encodedString } };

                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                return new Tuple<bool, string>(true, string.Empty);
            }
            //catch (CloudSaveValidationException ex) { }
            //catch (CloudSaveRateLimitedException ex) { }
            //catch (CloudSaveConflictException ex) { }
            //catch (CloudSaveException ex) { }
            catch (Exception ex) 
            {
                Debug.LogException(ex);
                //throw ex;
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        // UserData.
        public async Task<Tuple<bool, string>> LoadUserData(string key) 
        {
            try
            {
                if(!isInitialized)
                    throw new Exception("[Cloud] Module Not initialized.");

                var dictData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>{{key}});

                if(dictData!=null && dictData.TryGetValue(key, out var value))
                {
                    string originalJson = compressData ? StringCompressor.DecompressFromEncodedString(value.Value.GetAsString()) : value.Value.GetAsString();
                    Debug.Log($"[Cloud] Fetching the UserData has been successed. : {originalJson}");
                    
                    return new Tuple<bool, string>(true, originalJson);
                }
                return new Tuple<bool, string>(false, $"[Cloud] Fetch data has been failed. [{dictData}]");
            }
            catch(Exception ex) 
            {
                Debug.LogException(ex);
                return new Tuple<bool, string>(false, ex.Message);
            }
        }

        #endregion








        #region Events.


        #endregion



    }
        
}
