using App.GamePlay.IdleMiner.Common.Types;
using IGCore.MVCS;
using IGCore.PlatformService;
using IGCore.PlatformService.Cloud;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

public sealed partial class IdleMinerContext : AContext
{
    //
    // Core class of data handler. 
    //
    // Internal class so it can access to all private members of the IdleMinerContext.
    public class GameDataController
    {

        //  상황                   Signed In        로컬(ID_A)      클라우드       게스트       선택                  판정 및 액션 (Action)
        //  1. 클린 신규           Online   0           X                X            X         Local       [신규 생성] 아무 데이터도 없으므로 새 시작
        //  2. 게스트 연동         Online   0           X                X            O         Local       [마이그레이션] 게스트 -> ID_A 복사 후 게스트 삭제
        //  3. 기기 변경           Online   0           X                O            X         Cloud       [다운로드] 클라우드 데이터를 ID_A에 내려받음
        //  4. 데이터 충돌         Online   0           X                O            O         Latter      [최신데이터 선택]
        
        //  5. 단순 이어하기       Online   0           O                X            X         Local       [업로드] ID_A 를 클라우드 업로드
        //  6. 단순 이어하기       Online   0           O                X            O         Latter      [최신데이터 선택]
        //  7. 단순 이어하기       Online   0           O                O            X         Latter      [최신데이터 선택]
        //  8. 복합 충돌           Online   0           O                O            O         Latter      [최신데이터 선택]
        
        //  9. Network Error       Online   O           O         미확인(Error)       X         Local       로컬에만 업데이트 & timestamp 고정. 이후 온라인시 클라우드가 최신이면 user에게 물어봄.
        // 10.                     Online   O           O         미확인(Error)       O         Local       Act like 9 and Guest 삭제.
        // 11                      Online   O           X         미확인(Error)       X          X          Retry 후 네트워크 상태 확인 창.
        // 12                      Online   O           X         미확인(Error)       O          X          Retry 후 네트워크 상태 확인 창.

        //  13 오프라인 신규       Offline  X           X              미확인         X         Local       [게스트] 신규 Device 게스트 Player.
        //  14 오프라인 게스트     Offline  X           X              미확인         O         Local       [게스트 유지] 오프라인이므로 게스트 ID로 플레이
        //  15 오프라인 복귀       Offline  X           O              미확인         X         Local       [로컬 우선] 네트워크 확인 불가하므로 ID_A 로드
        //  16 Offline Player      Offline  X           O              미확인         O         Local       ID_A 로드, 게스트 삭제.
        
        enum AccountStatus
        {
            SignedIn_X_Local_X_Cloud_X_Guest_X, 
            SignedIn_X_Local_X_Cloud_X_Guest_O, 
            SignedIn_X_Local_O_Cloud_X_Guest_X, 
            SignedIn_X_Local_O_Cloud_X_Guest_O, 

            SignedIn_O_Local_X_Cloud_X_Guest_X, 
            SignedIn_O_Local_X_Cloud_X_Guest_O, 
            SignedIn_O_Local_X_Cloud_O_Guest_X, 
            SignedIn_O_Local_X_Cloud_O_Guest_O, 

            SignedIn_O_Local_O_Cloud_X_Guest_X, 
            SignedIn_O_Local_O_Cloud_X_Guest_O, 
            SignedIn_O_Local_O_Cloud_O_Guest_X, 
            SignedIn_O_Local_O_Cloud_O_Guest_O, 

            SignedIn_O_Local_X_Cloud_Err_Guest_X, 
            SignedIn_O_Local_X_Cloud_Err_Guest_O, 
            SignedIn_O_Local_O_Cloud_Err_Guest_X, 
            SignedIn_O_Local_O_Cloud_Err_Guest_O, 
        }


        const int LOCAL_DATA_SERVICE_IDX = 0;
        const int CLOUD_DATA_SERVICE_IDX = 1;

        IdleMinerContext contextCache;

        ILocalDataGatewayService guestMetaDataGatewayService, guestGameDataGatewayService;
        ILocalDataGatewayService metaDataGatewayService, gameDataGatewayService;
        ICloudDataGatewayService metaDataCloudGatewayService, gameDataCloudGatewayService;

        // !!! Service SubScriber PlayerModel should fetch data via this index.
        public int TargetMetaDataGatewayServiceIndex { get; private set; } = -1;
        public int TargetGameDataGatewayServiceIndex { get; private set; } = -1;

        List<IDataGatewayService> metaGatewayServiceList;
        public List<IDataGatewayService> MetaGatewayServiceList => metaGatewayServiceList;
        
        List<IDataGatewayService> gameGatewayServiceList;
        public List<IDataGatewayService> GameGatewayServiceList => gameGatewayServiceList;
        
        const string META_DATA_KEY = "MetaData";
        const string DEVICE_GUEST = "device_guest_id";
        
        public string PlayerId { get; private set; } = "";

        IAuthService authService;
        ICloudService cloudService;

        public GameDataController(IdleMinerContext context, IAuthService authService, ICloudService cloudService)
        {
            contextCache = context;

            this.authService = authService;
            this.cloudService = cloudService;
            guestMetaDataGatewayService = new DataGatewayService();
            guestGameDataGatewayService = new DataGatewayService();
            gameDataGatewayService = new DataGatewayService();
            metaDataGatewayService = new DataGatewayService();
            gameDataCloudGatewayService = new DataCloudGatewayService(cloudService);
            metaDataCloudGatewayService = new DataCloudGatewayService(cloudService);

            gameGatewayServiceList = new List<IDataGatewayService>() { gameDataGatewayService, gameDataCloudGatewayService };      
            metaGatewayServiceList = new List<IDataGatewayService>() { metaDataGatewayService,  metaDataCloudGatewayService }; 
        }

        public void Init()   {}

        // isMetaData : false => GameData.
        public async Task<bool> LoadUserDataAsync(bool isMetaData)
        {
            try
            {
                var taskCloudInit = WaitUntil(() => cloudService.IsInitialized());
                var timeOut = Task.Delay(5000);
                var completedTask = await Task.WhenAny(taskCloudInit, timeOut);

                PlayerId = isMetaData ? authService.GetPlayerId() : PlayerId;

                SetTargetGatewayServiceIndex(isMetaData, authService.IsSignedIn() ? CLOUD_DATA_SERVICE_IDX : LOCAL_DATA_SERVICE_IDX);

                var loadRet = await TryLoadAllUserData(isMetaData);

                AccountStatus eAccStatus = GetAccountStatus(loadRet.Item1, loadRet.Item2, loadRet.Item3, loadRet.Item4);

                int selectedIndex = 0;
                switch( eAccStatus )
                {
                case AccountStatus.SignedIn_X_Local_O_Cloud_X_Guest_O:          // 16. Offline Player
                    if(isMetaData)  DeleteDeviceGuestDataFiles();
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;

                case AccountStatus.SignedIn_X_Local_X_Cloud_X_Guest_X:          // 13. Offline New Device Guest
                case AccountStatus.SignedIn_X_Local_X_Cloud_X_Guest_O:          // 14. Offline Device-Guest                    
                    PlayerId = DEVICE_GUEST;
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;

                case AccountStatus.SignedIn_X_Local_O_Cloud_X_Guest_X:          // 15. Offline Player
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;


                case AccountStatus.SignedIn_O_Local_X_Cloud_X_Guest_X:          // 01. New Player Account.
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;
                case AccountStatus.SignedIn_O_Local_X_Cloud_X_Guest_O:          // 02. Promote Guest to Player.
                    if(isMetaData)
                    { 
                        MigrateDataFilesToPlayer(DEVICE_GUEST, PlayerId);
                        await metaDataGatewayService.ReadData(PlayerId, META_DATA_KEY);
                    }
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;
                case AccountStatus.SignedIn_O_Local_X_Cloud_O_Guest_X:          // 03. Use Cloud Data.
                    SetTargetGatewayServiceIndex(isMetaData, CLOUD_DATA_SERVICE_IDX);
                    break;
                case AccountStatus.SignedIn_O_Local_X_Cloud_O_Guest_O:          // 04. Select [DeviceGuest VS Cloud]
                {
                    var guestGWS = isMetaData ? guestMetaDataGatewayService : guestGameDataGatewayService;
                    var cloudGWS = isMetaData ? metaDataCloudGatewayService : gameDataCloudGatewayService;
                    selectedIndex = SelectLatestDataGatewayService( (
                                guestGWS as DataGatewayService).ServiceData.Environment.TimeStamp, 
                                (cloudGWS as DataCloudGatewayService).ServiceData.Environment.TimeStamp);
                    if(isMetaData && selectedIndex==0)
                    {
                        MigrateDataFilesToPlayer(DEVICE_GUEST, PlayerId);
                        await metaDataGatewayService.ReadData(PlayerId, META_DATA_KEY);
                    }
                    SetTargetGatewayServiceIndex(isMetaData, selectedIndex==0 ? LOCAL_DATA_SERVICE_IDX : CLOUD_DATA_SERVICE_IDX);
                    break;
                }
                case AccountStatus.SignedIn_O_Local_O_Cloud_X_Guest_X:          // 05. Use Local Data.
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;
                case AccountStatus.SignedIn_O_Local_O_Cloud_X_Guest_O:          // 06. Select [DeviceGuest VS Local]
                {
                    var guestGWS = isMetaData ? guestMetaDataGatewayService : guestGameDataGatewayService;
                    var localGWS = isMetaData ? metaDataGatewayService : gameDataGatewayService;

                    selectedIndex = SelectLatestDataGatewayService(
                                (guestGWS as DataGatewayService).ServiceData.Environment.TimeStamp, 
                                (localGWS as DataGatewayService).ServiceData.Environment.TimeStamp);
                    if(isMetaData && selectedIndex==0)
                    {
                        MigrateDataFilesToPlayer(DEVICE_GUEST, PlayerId);
                        await metaDataGatewayService.ReadData(PlayerId, META_DATA_KEY);
                    }
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;
                }
                case AccountStatus.SignedIn_O_Local_O_Cloud_O_Guest_X:          // 07. Select [Local VS Cloud]
                {
                    var localGWS = isMetaData ? metaDataGatewayService : gameDataGatewayService;
                    var cloudGWS = isMetaData ? metaDataCloudGatewayService : gameDataCloudGatewayService;
                    selectedIndex = SelectLatestDataGatewayService(
                                (localGWS as DataGatewayService).ServiceData.Environment.TimeStamp, 
                                (cloudGWS as DataCloudGatewayService).ServiceData.Environment.TimeStamp);
                    SetTargetGatewayServiceIndex(isMetaData, selectedIndex==0 ? LOCAL_DATA_SERVICE_IDX : CLOUD_DATA_SERVICE_IDX);
                    break;
                }
                case AccountStatus.SignedIn_O_Local_O_Cloud_O_Guest_O:          // 08. Select [Guest VS Local VS Cloud]
                {
                    var guestGWS = isMetaData ? guestMetaDataGatewayService : guestGameDataGatewayService;
                    var localGWS = isMetaData ? metaDataGatewayService : gameDataGatewayService;
                    var cloudGWS = isMetaData ? metaDataCloudGatewayService : gameDataCloudGatewayService;

                    selectedIndex = SelectLatestDataGatewayService(
                                (guestGWS as DataGatewayService).ServiceData.Environment.TimeStamp,
                                (localGWS as DataGatewayService).ServiceData.Environment.TimeStamp, 
                                (cloudGWS as DataCloudGatewayService).ServiceData.Environment.TimeStamp);
                    if(selectedIndex == 0)
                    {
                        if(isMetaData)
                        { 
                            MigrateDataFilesToPlayer(DEVICE_GUEST, PlayerId);
                            await metaDataGatewayService.ReadData(PlayerId, META_DATA_KEY);
                        }
                        SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    }
                    else if(selectedIndex == 1)
                        SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    else 
                        SetTargetGatewayServiceIndex(isMetaData, CLOUD_DATA_SERVICE_IDX);
                    break;
                }
                case AccountStatus.SignedIn_O_Local_X_Cloud_Err_Guest_X:        // 11. Cloud data 얻기 실패 after sign in ==> Retry for now.
                case AccountStatus.SignedIn_O_Local_X_Cloud_Err_Guest_O:        // 12
                    // UNACCEPTABLE CASE - RETRY !!!
                    return false;
                case AccountStatus.SignedIn_O_Local_O_Cloud_Err_Guest_O:        // 10
                    // DeleteDeviceGuestDataFiles();
                    return false;
                case AccountStatus.SignedIn_O_Local_O_Cloud_Err_Guest_X:        // 09
                    // Write Data on Local with No TimeStamp Updates, and then once back to online, compare timestamp. -> if cloud is later then ask player what to select.
                    //TargetMetaDataGatewayServiceIndex = IDX_LOCA_DATA_SERVICE;
                    return false;
                default:
                    Assert.IsTrue(false, $"Unacceptable case {eAccStatus} found !!!" );
                    break;
                }

                if(isMetaData)  {   Assert.IsTrue(TargetMetaDataGatewayServiceIndex >= 0, $"Invalid Target Meta GatewayValue : {TargetMetaDataGatewayServiceIndex}");  }
                else            {   Assert.IsTrue(TargetGameDataGatewayServiceIndex >= 0, $"Invalid Target Game GatewayValue : {TargetGameDataGatewayServiceIndex}");  }

                Assert.IsTrue(!string.IsNullOrEmpty(PlayerId), "Player Id is empty !");
                contextCache.UpdateData("PlayerId", PlayerId);

                // Logging.
                int curIdx = isMetaData ? TargetMetaDataGatewayServiceIndex : TargetGameDataGatewayServiceIndex;
                string target = curIdx==LOCAL_DATA_SERVICE_IDX ? "LOCAL" : "CLOUD";
                Debug.Log($"<color=green>[DataCtrl][Info] status [{eAccStatus}], Target [{target}], PlayerId [{PlayerId}] </color>");
                return true;
            }
            catch( Exception ex ) 
            {
                Debug.LogWarning("[DataCotnroller] : " + ex.Message);
                return false;
            }
        }

    
        public async Task InitGame()
        {
            gameDataGatewayService.ClearModels();
            gameDataCloudGatewayService.ClearModels();

            bool isDone = false;
            while(Application.isPlaying && !isDone)
            {
                isDone = await LoadUserDataAsync(isMetaData:false);
                await Task.Delay(1000);
                Debug.Log("Try Selecting Load Meta Data....Local / Cloud / Guest..");
            }
        }

        public void DisposeGame()
        {
            SavePlayerData(isLocal:true).Forget();
            SavePlayerData(isLocal:false).Forget();
        }






        public void SavePlayerDataInstantly()
        {
            SavePlayerData(isLocal:true).Forget();
            SavePlayerData(isLocal:false).Forget();
        }
        public void SaveMetaDataInstantly()
        {
            SaveMetaData(isLocal:true).Forget();
            SaveMetaData(isLocal:false).Forget();
        }
        async Task<bool> SavePlayerData(bool isLocal, bool clearAll = false)
        {
            string dataKey = $"{GameKey}_PlayerData";
       
            if(isLocal)
            {
                bool ret = await gameDataGatewayService.WriteData((string)contextCache.GetData("PlayerId"), dataKey, clearAll);
                if(ret)
                {
                    Debug.Log("<color=blue>[Data] Storing Player Data in Local has been successed.</color>");
                    return true;
                }
            }
            else
            {
                if(ICloudService.ResultType.eSuccessed == await gameDataCloudGatewayService.WriteData(dataKey, clearAll))
                {
                    Debug.Log("<color=green>[Data] Storing Player Data in Cloud has been successed.</color>");
                    return true;
                }
            }
            return false;
        }
        
        public void ResetPlayerData()
        {
            SavePlayerData(isLocal:true, clearAll:true).Forget();
            SavePlayerData(isLocal:false, clearAll:true).Forget();
        }
        
        async Task<bool> SaveMetaData(bool isLocal)
        {
            string dataKey = "MetaData";

            if(isLocal)
            {
                bool ret = await metaDataGatewayService.WriteData((string)contextCache.GetData("PlayerId"),dataKey, clearAll:false);
                if(ret)
                {
                    Debug.Log("<color=blue>[Data] Storing Meta Data in Local has been successed.</color>");
                    return true;
                }
            }
            else
            {
                if(ICloudService.ResultType.eSuccessed == await metaDataCloudGatewayService.WriteData(dataKey, clearAll:false))
                {
                    Debug.Log("<color=green>[Data] Storing Meta Data in Cloud has been successed.</color>");
                    return true;
                }
            }
            return false;
        }
        

        public void RunMetaDataSaveDog()
        {
            UpdateMetaDataSaveAsync(isLocal:true).Forget();
            UpdateMetaDataSaveAsync(isLocal:false).Forget();
        }

        async Task UpdateMetaDataSaveAsync(bool isLocal)
        {
            AppConfig appConfig = null;
            while(Application.isPlaying)
            {
                if(appConfig == null)
                    appConfig = (AppConfig)contextCache.GetData("AppConfig", null);

                int delay = 1000;
                if(isLocal) delay = appConfig==null ? 2 * 1000 : appConfig.MetaDataSaveLocalInterval * 1000;
                else        delay = appConfig==null ? 5 * 1000 : appConfig.MetaDataSaveCloudInterval * 1000;
            
                await Task.Delay(delay);
                await SaveMetaData(isLocal);
            }
        }

        public void RunGameDataSaveDog()
        {
            UpdateGameDataSaveAsync(isLocal:true).Forget();
            UpdateGameDataSaveAsync(isLocal:false).Forget();
        }
    
        async Task UpdateGameDataSaveAsync(bool isLocal)
        {
            AppConfig appConfig = null;
            while(Application.isPlaying && contextCache.isPlayingGame)
            {
                if(appConfig == null)
                    appConfig = (AppConfig)contextCache.GetData("AppConfig", null);

                int delay = 1000;
                if(isLocal) delay = appConfig==null ? 2 * 1000 : appConfig.GameDataSaveLocalInterval * 1000;
                else        delay = appConfig==null ? 5 * 1000 : appConfig.GameDataSaveCloudInterval * 1000;

                await Task.Delay(delay);
                await SavePlayerData(isLocal);
            }
        }






        public void SavePrevPlayerId(string playerId)
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
        
        bool DeleteDeviceGuestDataFiles()
        {
            try
            {
                const string sourceDir = DEVICE_GUEST;

                if (!Directory.Exists(sourceDir))
                    return false;

                if (Directory.Exists(sourceDir))
                {
                    Directory.Delete(sourceDir, recursive:true); 
                    Debug.Log("[DataController] : Guest Folder has been deleted.");
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataController] Expection has happened during deletion process : {e.Message}");
                return false;
            }
        }

        // foundLocal, foundCloud, foundGuest, failedToLoadCloudDueToNetwork
        //
        async Task<Tuple<bool, bool, bool, bool>> TryLoadAllUserData(bool isMetaData)
        {
            bool isFoundLocalData = false;
            bool isFoundCloudData = false;
            bool hadErrorWhenFetchingCloudData = false;

            // Try Load Local Guest.
            string gameDataKey = isMetaData ? string.Empty : $"{GameKey}_PlayerData";
            bool isFoundLocalGuestData = isMetaData ? await guestMetaDataGatewayService.ReadData(DEVICE_GUEST, META_DATA_KEY) : await guestGameDataGatewayService.ReadData(DEVICE_GUEST, gameDataKey);
            
            if (authService.IsSignedIn()) 
            {
                // Try Load Cloud.
                var cloudTask = isMetaData ? metaDataCloudGatewayService.ReadData(META_DATA_KEY) : gameDataCloudGatewayService.ReadData(gameDataKey); 
                var timeoutTask = Task.Delay(5000);

                var completedTask = await Task.WhenAny(cloudTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    Debug.LogWarning("[Cloud] Meta Data Server response timeout !");
                    hadErrorWhenFetchingCloudData = true;
                }
                else
                {
                    if(cloudTask.Result == ICloudService.ResultType.eDataNotFound)
                    {
                        isFoundCloudData = false;
                        Debug.LogWarning("[Cloud] No Meta Cloud Data found.");
                    }
                    else if(cloudTask.Result != ICloudService.ResultType.eSuccessed)
                    {
                        Debug.LogWarning("[Cloud] Meta Cloud Data load failed due to network reason.!");
                        hadErrorWhenFetchingCloudData = true;
                    }
                    else
                    {
                        isFoundCloudData = true;

                        // Try Local Load.
                        isFoundLocalData = isMetaData ? await metaDataGatewayService.ReadData(PlayerId, META_DATA_KEY) : await gameDataGatewayService.ReadData(PlayerId, gameDataKey);
                    }
                }
            }
            else
            {
                // Try Local Load.
                if(!string.IsNullOrEmpty(PlayerId))
                {
                    isFoundLocalData = isMetaData ? await metaDataGatewayService.ReadData(PlayerId, META_DATA_KEY) : await gameDataGatewayService.ReadData(PlayerId, gameDataKey);
                }
            }

            return new Tuple<bool, bool, bool, bool>(isFoundLocalData, isFoundCloudData, isFoundLocalGuestData, hadErrorWhenFetchingCloudData);
        }

        void SetTargetGatewayServiceIndex(bool isMetaData, int idx)
        {
            if(isMetaData)  
                TargetMetaDataGatewayServiceIndex = idx;
            else            
                TargetGameDataGatewayServiceIndex = idx;
        }

        async Task WaitUntil(Func<bool> predicate)
        {
            while (Application.isPlaying && !predicate())
            {
                await Task.Delay(100); 
            }
        }

        AccountStatus GetAccountStatus(bool isFoundLocalData, bool isFoundCloudData, bool isFoundLocalGuestData, bool failedToGetCloudDataDueToNetwork)
        {            
            AccountStatus eStatus = AccountStatus.SignedIn_X_Local_X_Cloud_X_Guest_X;
            
            if (authService.IsSignedIn()) 
            {
                if(failedToGetCloudDataDueToNetwork)
                {
                    if(isFoundLocalData)
                    {
                        if(isFoundLocalGuestData)
                            eStatus = AccountStatus.SignedIn_O_Local_O_Cloud_Err_Guest_O;
                        else 
                            eStatus = AccountStatus.SignedIn_O_Local_O_Cloud_Err_Guest_X;
                    }
                    else
                    {
                        if(isFoundLocalGuestData)
                            eStatus = AccountStatus.SignedIn_O_Local_X_Cloud_Err_Guest_O;
                        else 
                            eStatus = AccountStatus.SignedIn_O_Local_X_Cloud_Err_Guest_X;
                    }
                }
                else
                {
                    if(isFoundLocalData)
                    {
                        if(isFoundCloudData)
                        {
                            if(isFoundLocalGuestData)
                                eStatus = AccountStatus.SignedIn_O_Local_O_Cloud_O_Guest_O;
                            else 
                                eStatus = AccountStatus.SignedIn_O_Local_O_Cloud_O_Guest_X;
                        }
                        else
                        {
                            if(isFoundLocalGuestData)
                                eStatus = AccountStatus.SignedIn_O_Local_O_Cloud_X_Guest_O;
                            else 
                                eStatus = AccountStatus.SignedIn_O_Local_O_Cloud_X_Guest_X;
                        }
                    }
                    else
                    {
                        if(isFoundCloudData)
                        {
                            if(isFoundLocalGuestData)
                                eStatus = AccountStatus.SignedIn_O_Local_X_Cloud_O_Guest_O;
                            else 
                                eStatus = AccountStatus.SignedIn_O_Local_X_Cloud_O_Guest_X;
                        }
                        else
                        {
                            if (isFoundLocalGuestData)
                                eStatus = AccountStatus.SignedIn_O_Local_X_Cloud_X_Guest_O;
                            else
                                eStatus = AccountStatus.SignedIn_O_Local_X_Cloud_X_Guest_X;
                        }
                    }
                }
            }
            else
            {
                if(isFoundLocalGuestData)
                {
                    if (isFoundLocalGuestData)
                        eStatus = AccountStatus.SignedIn_X_Local_O_Cloud_X_Guest_O;
                    else
                        eStatus = AccountStatus.SignedIn_X_Local_O_Cloud_X_Guest_X;
                }
                else
                {
                    if (isFoundLocalGuestData)
                        eStatus = AccountStatus.SignedIn_X_Local_O_Cloud_X_Guest_O;
                    else
                        eStatus = AccountStatus.SignedIn_X_Local_X_Cloud_X_Guest_X;
                }
            }
            return eStatus;
        }


        int SelectLatestDataGatewayService(long timestamp1, long timestamp2)
        {
            return timestamp1>=timestamp2 ? 0 : 1;
        }
        int SelectLatestDataGatewayService(long timestamp1, long timestamp2, long timestamp3)
        {
            if(timestamp1 >= timestamp2)
            {
                int ret = SelectLatestDataGatewayService(timestamp1, timestamp3);
                return ret==0 ? 0 : 2;
            }
            else
            {
                int ret = SelectLatestDataGatewayService(timestamp2, timestamp3);
                return ret==0 ? 1 : 2;
            }
        }



        /*
         * 
         * 
        
        async Task<bool> LoadPlayerData(int idxGatewayService)
        {   
            if(idxGatewayService<0 || idxGatewayService>=MetaGatewayServiceList.Count)
            {
                Assert.IsTrue(false, "Invalid MetaGateWayService Index.." + idxGatewayService);
                return false;
            }

            IDataGatewayService dataGatewayService = GameGatewayServiceList[idxGatewayService];

            string dataKey = $"{GameKey}_PlayerData";
            if(idxGatewayService == LOCAL_DATA_SERVICE_IDX)
            {
                return await (dataGatewayService as ILocalDataGatewayService).ReadData((string)contextCache.GetData("PlayerId"), dataKey);
            }
            else
            {
                return (ICloudService.ResultType.eSuccessed == await (dataGatewayService as ICloudDataGatewayService).ReadData(dataKey));
            }
        }


        async Task<bool> LoadMetaData(int idxGatewayService)
        {
            if(idxGatewayService<0 || idxGatewayService>=MetaGatewayServiceList.Count)
            {
                Assert.IsTrue(false, "Invalid MetaGateWayService Index.." + idxGatewayService);
                return false;
            }
        
            IDataGatewayService dataGatewayService = MetaGatewayServiceList[idxGatewayService];

            string dataKey = "MetaData";
            if(idxGatewayService == LOCAL_DATA_SERVICE_IDX)
            {
                return await (dataGatewayService as ILocalDataGatewayService).ReadData((string)contextCache.GetData("PlayerId"), dataKey);
            }
            else
            {
                return ICloudService.ResultType.eSuccessed == await (dataGatewayService as ICloudDataGatewayService).ReadData(dataKey);
            }
        }


        async Task<bool> FetchLocalData()
        {
            await Task.Delay(100);
        
            bool ret = await LoadMetaData( LOCAL_DATA_SERVICE_IDX );

            return ret;
        }

        async Task<bool> FetchCloudData() 
        {
            long elTime = 0;
            int interval = 500;
            int MaxNetworkWaitSec = 5;
            while (Application.isPlaying && !cloudService.IsInitialized() && elTime<=MaxNetworkWaitSec*1000)
            {
                await Task.Delay(interval);
                elTime += interval;
            }

            if(!cloudService.IsInitialized())
            {
                Debug.Log("<color=red>[AppMain] Fetching Data From Cloud has been failed due to time-out. </color>");
                return false;
            }

           // return ICloudService.ResultType.eSuccessed == await (dataGatewayService as ICloudDataGatewayService).ReadData(dataKey);

            bool ret  = await LoadMetaData( CLOUD_DATA_SERVICE_IDX );
            return ret;
        }



        /*
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
        }*/
    }
}
