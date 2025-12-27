using UnityEngine;
using Unity.Services.Core;
using System;
using System.Threading.Tasks;

namespace IGCore.PlatformService.Cloud
{
    public class UnityService : MonoBehaviour, IService
    {
        [SerializeField] int retryInterval = 5;

        bool isInitialized = false;

        async void Awake()
        {
            isInitialized = false;

            await TryConnectAsync();
        }
        
        public bool IsInitialized()   { return isInitialized; }

        async Task TryConnectAsync()
        {
            var waitSec = new WaitForSeconds(retryInterval);
            
            while(isInitialized == false)
            {
                if(Application.internetReachability == NetworkReachability.NotReachable)
                {
                    await Task.Delay(retryInterval * 1000 * 5);
                    continue;
                }
                
                try
                {
                    await UnityServices.InitializeAsync();
                    Debug.Log("<color=green>[UnityService] Connection has been completed.</color>");
                    
                    isInitialized = true;
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
    }
}
