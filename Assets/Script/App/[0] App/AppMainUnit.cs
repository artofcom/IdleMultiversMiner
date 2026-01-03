using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using IGCore.MVCS;
using IGCore.PlatformService.Cloud;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

public class AppMainUnit : AUnit
{
    const string DEVICE_GUEST = "device_guest_id";

    [SerializeField] AppConfig appConfig;
    [SerializeField] UnitSwitcherComp unitSwitcher;

    // MetaSystems.
    [SerializeField] List<AUnit> metaSystems;

    [ImplementsInterface(typeof(IAuthService))]
    [SerializeField] MonoBehaviour authService;
    [ImplementsInterface(typeof(ICloudService))]
    [SerializeField] MonoBehaviour cloudService;

    [SerializeField] int MaxNetworkWaitSec = 5;

    IAuthService AuthService => authService as IAuthService;
    ICloudService CloudService => cloudService as ICloudService;

    AContext _minerContext = null;
    AppPlayerModel playerModel;

    IdleMinerContext IMContext => _minerContext as IdleMinerContext;

    enum AccountStatus
    {
        UNKNOWN,
        NULL_2_ID_A, 
        NULL_2_NULL, 
        DEVICE_ID_2_NULL, 
        DEVICE_ID_2_ID_A,
        ID_A_2_NULL, 
        ID_A_2_ID_A, 
        ID_A_2_ID_B,
    }
    AccountStatus eAccountStatus = AccountStatus.UNKNOWN;

    protected override void Awake() 
    { 
        base.Awake();
        
        AuthService.EventOnSignedIn += OnSignedIn;
        AuthService.EventOnSignInFailed += OnSignInFailed;
        AuthService.EventOnSignOut += OnSignedOut;

        _minerContext = new IdleMinerContext(cloudService as ICloudService);
        
        IMContext.Init(this);
        IMContext.AddData("AppConfig", appConfig);

        Init(_minerContext);

        unitSwitcher.Init(_minerContext);
    }
    protected void Start()
    {
        Application.targetFrameRate = 61;
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application Quit.");
    }

    async Task LoadAppMetaPlayerDataModel(bool isSignedIn, string curSignedPlayerId)
    {   
        string prevSignedPlayerId = PlayerPrefs.GetString(DataKeys.PREV_PLAYER_ID, string.Empty);
        Debug.Log($"<color=green>[AppMain] Singed PlayerId [{prevSignedPlayerId}] / [{curSignedPlayerId}] </color>");

        bool shouldUseCloudData = !string.IsNullOrEmpty(curSignedPlayerId);
        string dataLocation = shouldUseCloudData ? "CLOUD" : "LOCAL";
        Debug.Log($"<color=green>[AppMain] Target Data Location : [{dataLocation}]</color>");
        
        eAccountStatus = GetAccountStatusById(prevSignedPlayerId, curSignedPlayerId);
        switch(eAccountStatus)
        {
        case AccountStatus.NULL_2_ID_A:             // [Cloud] New Signed In Guest Player.
            SavePrevPlayerId(curSignedPlayerId);
            break;
        case AccountStatus.NULL_2_NULL:             // [Local] Offline Guest. => DEVICE_ID
            curSignedPlayerId = DEVICE_GUEST;
            SavePrevPlayerId(curSignedPlayerId);
            break;
        case AccountStatus.DEVICE_ID_2_NULL:        // [Local] Local Data of Device_id
            curSignedPlayerId = DEVICE_GUEST;    
            break;

        case AccountStatus.DEVICE_ID_2_ID_A:        // [Cloud] Migrate device_id guest data to ID_A
        {
            bool ret = MigrateDataFilesToPlayer(Path.Combine(Application.persistentDataPath, DEVICE_GUEST), Path.Combine(Application.persistentDataPath, curSignedPlayerId));
            if(false == ret)
            { 
                Debug.Log($"<color=red>[Migration] Data migration has been failed. Let's Keep using device_id for now...</color>");
                curSignedPlayerId = DEVICE_GUEST;
                shouldUseCloudData = false;
            }
            else 
                SavePrevPlayerId(curSignedPlayerId);
            break;
        }
        case AccountStatus.ID_A_2_NULL:             // [Local] Local Data of ID_A
            curSignedPlayerId = prevSignedPlayerId;    
            break;
        case AccountStatus.ID_A_2_ID_A:             // [Any] Local Data of ID_A
            break;
        case AccountStatus.ID_A_2_ID_B:             // [Cloud] Local Data of ID_B
            SavePrevPlayerId(curSignedPlayerId);
            break;
        default:
            Assert.IsTrue(false, "Unknown Status !!! " + eAccountStatus);
            break;
        }
        //
        
        Assert.IsTrue(!string.IsNullOrEmpty(curSignedPlayerId));
        dataLocation = shouldUseCloudData ? "CLOUD" : "LOCAL";
        Debug.Log($"<color=green>[AppMain] Aligned Type [{eAccountStatus}], Target [{dataLocation}], Singed PlayerId [{prevSignedPlayerId}] / [{curSignedPlayerId}] </color>");

        IMContext.SetSignedInPlayerIdForGatewayServices(curSignedPlayerId);
        
        
        playerModel = new AppPlayerModel(_minerContext, IMContext.MetaGatewayServiceList);
        model = new AppModel(_minerContext, playerModel);
        controller = new AppController(this, view, model, _minerContext);

        IMContext.ValidGatewayServiceIndex = shouldUseCloudData ? IdleMinerContext.IDX_CLOUD_DATA_SERVICE : IdleMinerContext.IDX_LOCA_DATA_SERVICE;

        if (shouldUseCloudData) 
        {
            bool fetchData = await FetchCloudData();
            while(Application.isPlaying && !fetchData)
            {
                await Task.Delay(1000);
                Debug.Log("[AppMain] Try Fetching Data From Cloud....");
                fetchData = await FetchCloudData();
            }
            await FetchLocalData();

            // case 1 : Signed-in to Same Device - Mostly Local should be the latest one. - So should upload to cloud.
            // case 2 : Signed-in to another device - Cloud data should/could be the lastest one.
            IMContext.ValidGatewayServiceIndex = IMContext.GetLatestMetaDataIndex();
            
            string target = IMContext.ValidGatewayServiceIndex == IdleMinerContext.IDX_LOCA_DATA_SERVICE ? "LOCAL" : "CLOUD";            
            Debug.Log($"[AppMain] Data Selector : [{target}] data has been selected for {curSignedPlayerId}.");
        }
        else 
            await FetchLocalData();

        // Init Modulels.
        model.Init();
        controller.Init();
        playerModel.Init();
        
        if(metaSystems != null)
        {
            for(int q = 0; q < metaSystems.Count; q++) 
                metaSystems[q].Init(_minerContext);
        }

        IMContext.RunMetaDataSaveDog();

        // Make sure to sync cloud data with the local one.
        if(shouldUseCloudData && IMContext.ValidGatewayServiceIndex==IdleMinerContext.IDX_LOCA_DATA_SERVICE)
        {
            await Task.Delay(1000);
            playerModel.SetDirty();
        }

        EventSystem.DispatchEvent(EventID.APPLICATION_PLAYERDATA_INITIALIZEDD);
    }

    async Task<bool> FetchLocalData()
    {
        await Task.Delay(100);
        
        await IMContext.LoadMetaData( IdleMinerContext.IDX_LOCA_DATA_SERVICE );

        return true;
    }

    async Task<bool> FetchCloudData() 
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("<color=red>[AppMain] Fetching Data From Cloud has been failed due to internet connection. </color>");
            return false;
        }

        long elTime = 0;
        int interval = 500;
        while (Application.isPlaying && !CloudService.IsInitialized() && elTime<=MaxNetworkWaitSec*1000)
        {
            await Task.Delay(interval);
            elTime += interval;
        }

        if(!CloudService.IsInitialized())
        {
            Debug.Log("<color=red>[AppMain] Fetching Data From Cloud has been failed due to time-out. </color>");
            return false;
        }

        bool ret  = await IMContext.LoadMetaData( IdleMinerContext.IDX_CLOUD_DATA_SERVICE );
        return ret;
    }


    //      ID recognition matrix.
    // 
    //       prev_player_id       cur_player_id            Lcation   Data Exists in Local?( Y / N )
    //
    // 1.        No                    ID_A             => [Cloud]     ID_A / New Guest
    //
    // 2.        No                     No              => [Local]     Dev_Id / New Device_Id Guest
    //
    // 3.      Device_Id                No              => [Local]     Dev_Id / New Device_Id Guest
    //
    // 4.      Device_Id               ID_A             => [Cloud]     Migrate device guest data to ID_A
    //
    // 5.       ID_A                    No              => [Local]     ID_A / New Device_Id Guest
    //
    // 6.       ID_A                   ID_A             => [Cloud]     Timestamp comparison for ID_A / Cloud
    // 
    // 7.       ID_A                   ID_B             => [Cloud]     Timestamp comparison for ID_B / New Guest
    //   
    AccountStatus GetAccountStatusById(string prevSignedPlayerId, string curSignedPlayerId)
    {
        if(string.IsNullOrEmpty(prevSignedPlayerId))
        {   
            if(string.IsNullOrEmpty(curSignedPlayerId))     
            {
                // Matrix case 2. NULL -> NULL              // [Local] Offline Guest. => DEVICE_ID
                return AccountStatus.NULL_2_ID_A;
            }
            else                                            
            {
                // Matrix case 1. NULL -> ID_A              // [Cloud] New Signed In Guest Player.
                return AccountStatus.NULL_2_ID_A;
            }
        }
        else   
        {
            if(prevSignedPlayerId.ToLower().Contains(DEVICE_GUEST.ToLower()))
            {   
                // Matrix case 3. DEVICE_ID -> NULL         // [Local] Local Data of Device_id
                if(string.IsNullOrEmpty(curSignedPlayerId)) 
                {
                    return AccountStatus.DEVICE_ID_2_NULL;
                }
                // Matrix case 4. DEVICE_ID -> ID_A         // [Cloud] Migrate device_id guest data to ID_A
                else                                        
                {
                    return AccountStatus.DEVICE_ID_2_ID_A;
                }
            }
            else
            {
                // Matrix case 5. ID_A -> NULL              // [Local] Local Data of ID_A
                if(string.IsNullOrEmpty(curSignedPlayerId)) 
                {
                    return AccountStatus.ID_A_2_NULL;
                }
                // Matrix case 6. ID_A -> ID_A              // [Any] Local Data of ID_A
                else if(curSignedPlayerId == prevSignedPlayerId)
                {   
                    return AccountStatus.ID_A_2_ID_A;
                }
                // Matrix case 7. ID_A -> ID_B              // [Cloud] Local Data of ID_B
                else
                {   
                    return AccountStatus.ID_A_2_ID_B;
                }
            }
        }
    }

    void SavePrevPlayerId(string playerId)
    {
        PlayerPrefs.SetString(DataKeys.PREV_PLAYER_ID, playerId);
        PlayerPrefs.Save();
    }

    bool MigrateDataFilesToPlayer(string sourceDir, string destDir)
    {
        try
        {
            if (!Directory.Exists(sourceDir))
                return false;

            if(!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
        
            string[] files = Directory.GetFiles(sourceDir);
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(destDir, fileName);
                File.Copy(filePath, destFilePath, true);
                
                Debug.Log($"[Migration] File Copy has been done. : {fileName} -> {destDir}");
            }
            
            if (Directory.Exists(sourceDir))
            {
                Directory.Delete(sourceDir, recursive:true); 
                Debug.Log("[Migration] : sourceDir has been deleted.");
            }

            Debug.Log("<color=green>[Migration] All has been done.</color>");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Migration] Expection has happened during copy process : {e.Message}");
            return false;
        }
    }

    

    async void OnSignedIn(string playerId)
    {
        context.UpdateData("PlayerId", playerId);

        await Task.Delay(100);

        LoadAppMetaPlayerDataModel(isSignedIn:true, playerId).Forget();
    }
    void OnSignInFailed(string reason)
    {
        LoadAppMetaPlayerDataModel(isSignedIn:false, string.Empty).Forget();
    }
    void OnSignedOut() 
    { 
        SavePrevPlayerId(string.Empty);
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("PlasticGames/Clear PlayerData/All")]
    private static void ClearPlayerPrefab()
    {
        AppPlayerModel.ClearAllData();
        IdleMinerPlayerModel.ClearAllData();
        PlayerPrefs.DeleteAll();

        Debug.Log("Deleting All PlayerPrefab...");
    }
#endif
}


        /*
        isWaitingForSignIn = false;
        isSignInSuccessed = false;

        if(Application.internetReachability != NetworkReachability.NotReachable)
        {
            isWaitingForSignIn = true;
            long elTick = 0;
            while(isWaitingForSignIn && elTick <= (MaxNetworkWaitSec*1000))
            {
                await Task.Delay(500);
                elTick += 500;
            }
            await Task.Delay(100);

            if(isSignInSuccessed)
                curSignedPlayerId = (string)context.GetData("PlayerId", string.Empty);
        }*/