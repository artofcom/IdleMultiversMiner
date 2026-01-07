using IGCore.PlatformService.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Assertions;

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
        
        public event Action EventOnInitialized;

        IService Service => service as IService;
        IAuthService AuthService => authService as IAuthService;

        bool isInitialized = false;

        async void Awake()
        {
            Assert.IsNotNull(service);

            AuthService.EventOnSignedIn += OnSignedIn;
        }

        void OnSignedIn(string playerId)
        {
            Debug.Log("[Cloud] Cloud Service has been registered successfully.");
            isInitialized = true;

            EventOnInitialized?.Invoke();
        }


        private void OnDestroy() {}


        #region Interfaces

        public bool IsInitialized()
        {
            return isInitialized;
        }

        // Error Message.
        public async Task<Tuple<ICloudService.ResultType, string>> SaveUserData(string key, string userData) 
        { 
            try
            { 
                if(!isInitialized)
                    return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eServiceNotInitialized, "[Cloud] Module Not initialized.");
                
                string encodedString = userData;
                if(compressData)
                {
                    encodedString = StringCompressor.CompressToEncodedString(userData);
                    Debug.Log($"[Cloud] Original Size {userData.Length} -> Compressed Size {encodedString.Length}");
                }

                var data = new Dictionary<string, object> { {key, encodedString } };

                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eSuccessed, string.Empty);
            }
            catch (CloudSaveException e)
            {
                switch (e.Reason)
                {
                    case CloudSaveExceptionReason.NoInternetConnection:
                        return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eNoNetworkConnection, "[Cloud] No network connection.!");
                    case CloudSaveExceptionReason.ProjectIdMissing:
                        return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eInvalidProjectId, "[Cloud] Invalid Project Id.!");
                    default:
                        return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eUnknownError, "[Cloud] Unknown Error. " + e.ErrorCode);
                }
            }
            catch (RequestFailedException e)
            {
                return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eInvalidAuth, "[Cloud] Invalid Auth.! " + e.ErrorCode);
            }
            catch (Exception ex) 
            {
                Debug.LogWarning("[Cloud] Exception : " + ex.Message);
                return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eUnknownError, ex.Message);
            }
        }

        // UserData.
        public async Task<Tuple<ICloudService.ResultType, string>> LoadUserData(string key) 
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
                    
                    return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eSuccessed, originalJson);
                }
                return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eDataNotFound, "[Cloud] Data Not Found.");
            }
            catch (CloudSaveException e)
            {
                switch (e.Reason)
                {
                    case CloudSaveExceptionReason.NoInternetConnection:
                        return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eNoNetworkConnection, "[Cloud] No network connection.!");
                    case CloudSaveExceptionReason.ProjectIdMissing:
                        return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eInvalidProjectId, "[Cloud] Invalid Project Id.!");
                    default:
                        return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eUnknownError, "[Cloud] Unknown Error. " + e.ErrorCode);
                }
            }
            catch (RequestFailedException e)
            {
                return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eInvalidAuth, "[Cloud] Invalid Auth.! " + e.ErrorCode);
            }
            catch(Exception ex) 
            {
                Debug.LogWarning("[Cloud] Exception : " + ex.Message);
                return new Tuple<ICloudService.ResultType, string>(ICloudService.ResultType.eUnknownError, ex.Message);
            }
        }

        #endregion








        #region Events.


        #endregion



    }
        
}
