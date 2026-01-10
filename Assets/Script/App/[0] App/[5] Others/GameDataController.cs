using App.GamePlay.IdleMiner.Common.Types;
using IGCore.MVCS;
using IGCore.PlatformService;
using IGCore.PlatformService.Cloud;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
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

        ILocalDataGatewayService guestMetaDataGatewayService;
        ILocalDataGatewayService metaDataGatewayService, gameDataGatewayService;
        ICloudDataGatewayService metaDataCloudGatewayService, gameDataCloudGatewayService;

        // !!! Service SubScriber PlayerModel should fetch data via this index.
        public int TargetMetaDataGatewayServiceIndex { get; private set; } = -1;
        public int TargetGameDataGatewayServiceIndex { get; private set; } = -1;

        List<IDataGatewayService> metaGatewayServiceList;
        public List<IDataGatewayService> MetaGatewayServiceList => metaGatewayServiceList;
        
        List<IDataGatewayService> gameGatewayServiceList;
        public List<IDataGatewayService> GameGatewayServiceList => gameGatewayServiceList;
        
        CancellationTokenSource metaGatewayDogCTS;
        CancellationTokenSource gameGatewayDogCTS;

        const string META_DATA_KEY = "MetaData";
        const string LAST_PLAYER_ID_KEY = "LastSignedPlayerId";
        string DEVICE_GUEST => "GUEST_" + SystemInfo.deviceUniqueIdentifier;
        
        string GameDataKey => IdleMinerContext.GameKey + "_PlayerData";
        public string PlayerId { get; private set; } = string.Empty;

        IAuthService authService;
        ICloudService cloudService;

        bool isAccountLinked = false;
        public GameDataController(IdleMinerContext context, IAuthService authService, ICloudService cloudService)
        {
            contextCache = context;

            this.authService = authService;
            this.cloudService = cloudService;
            guestMetaDataGatewayService = new DataGatewayService();
            gameDataGatewayService = new DataGatewayService();
            metaDataGatewayService = new DataGatewayService();
            gameDataCloudGatewayService = new DataCloudGatewayService(cloudService);
            metaDataCloudGatewayService = new DataCloudGatewayService(cloudService);

            gameGatewayServiceList = new List<IDataGatewayService>() { gameDataGatewayService, gameDataCloudGatewayService };      
            metaGatewayServiceList = new List<IDataGatewayService>() { metaDataGatewayService,  metaDataCloudGatewayService }; 

            authService.EventOnSignedIn += OnSignedIn;
            authService.EventOnSignOut += OnSignedOut;
        }

        public void Init()   {}

        // isMetaData : false => GameData.
        public async Task<bool> LoadUserDataAsync(bool isMetaData)
        {
            try
            {
                PlayerId = isMetaData ? PlayerPrefs.GetString(LAST_PLAYER_ID_KEY, string.Empty) : PlayerId;

                SetTargetGatewayServiceIndex(isMetaData, authService.IsSignedIn() ? CLOUD_DATA_SERVICE_IDX : LOCAL_DATA_SERVICE_IDX);

                var loadRet = await TryLoadAllUserData(isMetaData);

                AccountStatus eAccStatus = GetAccountStatus(loadRet.Item1, loadRet.Item2, loadRet.Item3, loadRet.Item4);

                long cloudTimeStampOffset = 10000000L;      // 1 sec delay. -> Helps to select local data when they have similr timestamp.
                int selectedIndex = 0;
                switch( eAccStatus )
                {
                case AccountStatus.SignedIn_X_Local_O_Cloud_X_Guest_O:          // 16. Offline Player
                    if(isMetaData)  DeletePlayerDataFiles(DEVICE_GUEST);
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;

                case AccountStatus.SignedIn_X_Local_X_Cloud_X_Guest_X:          // 13. Offline New Device Guest
                    Debug.Log("<color=red>[DataController][NewPlayer] No Data Found. Creating DeviceGuest player...</color>");
                    if(isMetaData) PlayerId = DEVICE_GUEST;
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;

                case AccountStatus.SignedIn_X_Local_X_Cloud_X_Guest_O:          // 14. Offline Device-Guest                    
                    if(isMetaData) PlayerId = DEVICE_GUEST;
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;

                case AccountStatus.SignedIn_X_Local_O_Cloud_X_Guest_X:          // 15. Offline Player
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;


                case AccountStatus.SignedIn_O_Local_X_Cloud_X_Guest_X:          // 01. New Player Account.
                    Debug.Log("<color=red>[DataController][NewPlayer] No Data Found. Creating Cloud Player...</color>");
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
                    if(isMetaData)
                    {
                        var guestGWS = isMetaData ? (guestMetaDataGatewayService as DataGatewayService) : null;
                        var cloudGWS = isMetaData ? (metaDataCloudGatewayService as DataCloudGatewayService) : (gameDataCloudGatewayService as DataCloudGatewayService);
                        long guestTS = guestGWS==null || guestGWS.ServiceData==null || guestGWS.ServiceData.Environment==null ? 0 : guestGWS.ServiceData.Environment.TimeStamp;
                        long cloudTS = cloudGWS==null || cloudGWS.ServiceData==null || cloudGWS.ServiceData.Environment==null ? 0 : cloudGWS.ServiceData.Environment.TimeStamp - cloudTimeStampOffset;
                        selectedIndex = SelectLatestDataGatewayService( guestTS, cloudTS );
                        if(selectedIndex == 0)
                        {
                            MigrateDataFilesToPlayer(DEVICE_GUEST, PlayerId);
                            await metaDataGatewayService.ReadData(PlayerId, META_DATA_KEY); // Meta data only here.
                        }
                        SetTargetGatewayServiceIndex(isMetaData, selectedIndex==0 ? LOCAL_DATA_SERVICE_IDX : CLOUD_DATA_SERVICE_IDX);
                    }
                    else 
                        SetTargetGatewayServiceIndex(isMetaData, CLOUD_DATA_SERVICE_IDX);
                    break;
                }
                case AccountStatus.SignedIn_O_Local_O_Cloud_X_Guest_X:          // 05. Use Local Data.
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;
                case AccountStatus.SignedIn_O_Local_O_Cloud_X_Guest_O:          // 06. Select [DeviceGuest VS Local]
                {
                    if(isMetaData)
                    { 
                        var guestGWS = isMetaData ? (guestMetaDataGatewayService as DataGatewayService) : null;
                        var localGWS = isMetaData ? (metaDataGatewayService as DataGatewayService) : (gameDataGatewayService as DataGatewayService);
                        long guestTS = guestGWS==null || guestGWS.ServiceData==null || guestGWS.ServiceData.Environment==null ? 0 : guestGWS.ServiceData.Environment.TimeStamp;
                        long localTS = localGWS==null || localGWS.ServiceData==null || localGWS.ServiceData.Environment==null ? 0 : localGWS.ServiceData.Environment.TimeStamp;
                        selectedIndex = SelectLatestDataGatewayService(guestTS, localTS);
                        if(selectedIndex == 0)
                        {
                            MigrateDataFilesToPlayer(DEVICE_GUEST, PlayerId);
                            await metaDataGatewayService.ReadData(PlayerId, META_DATA_KEY);// Meta data only here.
                        }
                    }
                    SetTargetGatewayServiceIndex(isMetaData, LOCAL_DATA_SERVICE_IDX);
                    break;
                }
                case AccountStatus.SignedIn_O_Local_O_Cloud_O_Guest_X:          // 07. Select [Local VS Cloud]
                {
                    var localGWS = isMetaData ? (metaDataGatewayService as DataGatewayService) : (gameDataGatewayService as DataGatewayService);
                    var cloudGWS = isMetaData ? (metaDataCloudGatewayService as DataCloudGatewayService) : (gameDataCloudGatewayService as DataCloudGatewayService);
                    long localTS = localGWS==null || localGWS.ServiceData==null || localGWS.ServiceData.Environment==null ? 0 : localGWS.ServiceData.Environment.TimeStamp;
                    long cloudTS = cloudGWS==null || cloudGWS.ServiceData==null || cloudGWS.ServiceData.Environment==null ? 0 : cloudGWS.ServiceData.Environment.TimeStamp - cloudTimeStampOffset;
                    selectedIndex = SelectLatestDataGatewayService(localTS, cloudTS);
                    SetTargetGatewayServiceIndex(isMetaData, selectedIndex==0 ? LOCAL_DATA_SERVICE_IDX : CLOUD_DATA_SERVICE_IDX);
                    break;
                }
                case AccountStatus.SignedIn_O_Local_O_Cloud_O_Guest_O:          // 08. Select [Guest VS Local VS Cloud]
                {
                    var guestGWS = isMetaData ? (guestMetaDataGatewayService as DataGatewayService) : null;
                    var localGWS = isMetaData ? (metaDataGatewayService as DataGatewayService) : (gameDataGatewayService as DataGatewayService);
                    var cloudGWS = isMetaData ? (metaDataCloudGatewayService as DataCloudGatewayService) : (gameDataCloudGatewayService as DataCloudGatewayService);
                    long guestTS = guestGWS==null || guestGWS.ServiceData==null || guestGWS.ServiceData.Environment==null ? 0 : guestGWS.ServiceData.Environment.TimeStamp;
                    long localTS = localGWS==null || localGWS.ServiceData==null || localGWS.ServiceData.Environment==null ? 0 : localGWS.ServiceData.Environment.TimeStamp;
                    long cloudTS = cloudGWS==null || cloudGWS.ServiceData==null || cloudGWS.ServiceData.Environment==null ? 0 : cloudGWS.ServiceData.Environment.TimeStamp - cloudTimeStampOffset;

                    selectedIndex = SelectLatestDataGatewayService(guestTS, localTS, cloudTS);
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
                    Assert.IsTrue(false, $"Unknown case {eAccStatus} found ???" );
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
                Debug.Log("Try Selecting Load Game Data....Local / Cloud / Guest..");
            }
        }

        public void DisposeGame()
        {
            //SavePlayerData(isLocal:true).Forget();
            //SavePlayerData(isLocal:false).Forget();
        }

        public void LockGatewayService(bool isMetaData, bool lock_it)
        {
            if(isMetaData)
            {
                metaDataGatewayService.IsLocked = lock_it;
                metaDataCloudGatewayService.IsLocked = lock_it;
            }
            else
            {
                gameDataGatewayService.IsLocked = lock_it;
                gameDataCloudGatewayService.IsLocked = lock_it;
            }
        }

        void OnSignedIn(string playerId)
        {
            SaveLastSignedPrevPlayerId(playerId);
            isAccountLinked = authService.IsAccountLinkedWithPlayer("unity");
        }
        async void OnSignedOut()
        {
            if(!isAccountLinked && !string.IsNullOrEmpty(PlayerId) && !PlayerId.Contains("GUEST"))
            {
                var appConfig = (AppConfig)contextCache.GetData("AppConfig", null);
                var deleteTask = DeletePlayerDataFilesAsync(PlayerId);
                var timeoutTask = Task.Delay(appConfig!=null ? appConfig.MaxServiceSignInWaitTime*1000 : 5000);

                await Task.WhenAny(deleteTask, timeoutTask);
            }

            SaveLastSignedPrevPlayerId(string.Empty);
        }

        async Task DeletePlayerDataFilesAsync(string playerId)
        {
            bool deleteSuccessed = false;
            long maxTrySec = 10;
            long tick = 0;
            while(Application.isPlaying && !deleteSuccessed && tick<(maxTrySec*1000))
            {
                deleteSuccessed = DeletePlayerDataFiles(PlayerId);
                await Task.Delay(200);
                tick += 200;
            }
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
            if(isLocal)
            {
                if(true == await gameDataGatewayService.WriteData((string)contextCache.GetData("PlayerId"), GameDataKey, clearAll))
                {
                    Debug.Log("<color=blue>[Data] Storing Player Data in Local has been successed.</color>");
                    return true;
                }
                else
                {
                    if(gameDataGatewayService.IsDirty)
                        Debug.Log("<color=red>[Data] Storing Local Player Data has been failed..</color>");
                }
            }
            else
            {
                if(ICloudService.ResultType.eSuccessed == await gameDataCloudGatewayService.WriteData(GameDataKey, clearAll))
                {
                    Debug.Log("<color=green>[Data] Storing Player Data in Cloud has been successed.</color>");
                    return true;
                }
                else
                {
                    if(gameDataCloudGatewayService.IsDirty)
                        Debug.Log("<color=red>[Data] Storing Cloud Player Data has been failed..</color>");
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
            if(isLocal)
            {
                if(true == await metaDataGatewayService.WriteData((string)contextCache.GetData("PlayerId"), META_DATA_KEY, clearAll:false))
                {
                    Debug.Log("<color=blue>[Data] Storing Meta Data in Local has been successed.</color>");
                    return true;
                }
                else
                {
                    if(metaDataGatewayService.IsDirty)
                        Debug.Log("<color=red>[Data] Storing Local Meta Data has been failed..</color>");
                }
            }
            else
            {
                if(ICloudService.ResultType.eSuccessed == await metaDataCloudGatewayService.WriteData(META_DATA_KEY, clearAll:false))
                {
                    Debug.Log("<color=green>[Data] Storing Meta Data in Cloud has been successed.</color>");
                    return true;
                }
                else
                {
                    if(metaDataCloudGatewayService.IsDirty)
                        Debug.Log("<color=red>[Data] Storing Cloud Meta Data has been failed..</color>");
                }
            }
            return false;
        }
        

        public void RunMetaDataSaveDog()
        {
            metaGatewayDogCTS?.Cancel();
            metaGatewayDogCTS = new CancellationTokenSource();

            UpdateMetaDataSaveAsync(isLocal:true).Forget();
            UpdateMetaDataSaveAsync(isLocal:false).Forget();
        }
        public void StopMetaDataSaveDog()
        {
            metaGatewayDogCTS?.Cancel();
        }

        async Task UpdateMetaDataSaveAsync(bool isLocal)
        {
            AppConfig appConfig = null;
            while(Application.isPlaying)
            {
                metaGatewayDogCTS.Token.ThrowIfCancellationRequested();

                if(appConfig == null)
                    appConfig = (AppConfig)contextCache.GetData("AppConfig", null);

                int delay = 1000;
                if(isLocal) delay = appConfig==null ? 2 * 1000 : appConfig.MetaDataSaveLocalInterval * 1000;
                else        delay = appConfig==null ? 5 * 1000 : appConfig.MetaDataSaveCloudInterval * 1000;
            
                await Task.Delay(delay, metaGatewayDogCTS.Token);
                await SaveMetaData(isLocal);
            }
        }

        public void RunGameDataSaveDog()
        {
            gameGatewayDogCTS?.Cancel();
            gameGatewayDogCTS = new CancellationTokenSource();

            UpdateGameDataSaveAsync(isLocal:true).Forget();
            UpdateGameDataSaveAsync(isLocal:false).Forget();
        }
    
        public void StopGameDataSaveDog()
        {
            gameGatewayDogCTS?.Cancel();
        }
        async Task UpdateGameDataSaveAsync(bool isLocal)
        {
            Assert.IsNotNull(gameGatewayDogCTS);

            AppConfig appConfig = null;
            while(Application.isPlaying && contextCache.isPlayingGame)
            {
                gameGatewayDogCTS.Token.ThrowIfCancellationRequested();

                if(appConfig == null)
                    appConfig = (AppConfig)contextCache.GetData("AppConfig", null);

                int delay = 1000;
                if(isLocal) delay = appConfig==null ? 2 * 1000 : appConfig.GameDataSaveLocalInterval * 1000;
                else        delay = appConfig==null ? 5 * 1000 : appConfig.GameDataSaveCloudInterval * 1000;

                await Task.Delay(delay, gameGatewayDogCTS.Token);
                await SavePlayerData(isLocal);
            }
        }






        void SaveLastSignedPrevPlayerId(string playerId)
        {
            PlayerPrefs.SetString(LAST_PLAYER_ID_KEY, playerId);
            PlayerPrefs.Save();
        }

        bool MigrateDataFilesToPlayer(string sourceDir, string destDir)
        {
            try
            {
                sourceDir = Path.Combine(Application.persistentDataPath, sourceDir);
                destDir = Path.Combine(Application.persistentDataPath, destDir);

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
        
        public bool DeletePlayerDataFiles(string playerId)
        {
            try
            {
                string sourceDir = Path.Combine(Application.persistentDataPath, playerId);

                if (!Directory.Exists(sourceDir))
                    return false;

                if (Directory.Exists(sourceDir))
                {
                    Directory.Delete(sourceDir, recursive:true); 
                    Debug.Log($"<color=red>[DataController] : Player Data {playerId} has been DELETED !!!</color>");
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
            bool isFoundCloudData = true;
            bool hadErrorWhenFetchingCloudData = false;

            string loadingTarget = isMetaData ? "META_DATA" : "GAME_DATA";
            Debug.Log($"<color=yellow>[DataCtrler] Loading {loadingTarget}.</color>");

            Debug.Log("<color=yellow>[DataCtrler] Try Loading Guest Data..</color>");
            string gameDataKey = isMetaData ? string.Empty : GameDataKey;
            bool isFoundLocalGuestData = isMetaData ? await guestMetaDataGatewayService.ReadData(DEVICE_GUEST, META_DATA_KEY) : false;
            Debug.Log($"<color=yellow>[DataCtrler] Guest Data Loading : {isFoundLocalGuestData}.</color>");

            var appConfig = (AppConfig)contextCache.GetData("AppConfig", null);
            bool shouldLoadLocalData = false;

            if (authService.IsSignedIn()) 
            {
                Debug.Log("<color=yellow>[DataCtrler] Try Loading Cloud Data... </color>");
                var cloudDownloadTask = isMetaData ? metaDataCloudGatewayService.ReadData(META_DATA_KEY) : gameDataCloudGatewayService.ReadData(gameDataKey); 
                var timeoutTask = Task.Delay(appConfig!=null ? appConfig.MaxServiceSignInWaitTime*1000 : 5000);

                var completedTask = await Task.WhenAny(cloudDownloadTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    hadErrorWhenFetchingCloudData = true;
                    Debug.Log($"<color=red>[DataCtrler] Cloud Loading has been failed due to time-out ! [5 sec] .</color>");
                }
                else
                {
                    if(cloudDownloadTask.Result == ICloudService.ResultType.eDataNotFound)
                    {
                        isFoundCloudData = false;
                        shouldLoadLocalData = true;
                        Debug.Log($"<color=red>[DataCtrler] Cloud Loading has been done, but no data found.</color>");
                    }
                    else if(cloudDownloadTask.Result != ICloudService.ResultType.eSuccessed)
                    {
                        hadErrorWhenFetchingCloudData = true;
                        Debug.Log($"<color=red>[DataCtrler] Cloud Loading has been failed for unknown reason. - Network?.</color>");
                    }
                    else
                    {
                        shouldLoadLocalData = true;
                        Debug.Log("<color=yellow>[DataCtrler] Cloud Loading has been done successfully. </color>");
                    }
                }
            }
            else
            {
                // Offline but try loading local data.
                if(!string.IsNullOrEmpty(PlayerId))
                    shouldLoadLocalData = true;
            }

            if(shouldLoadLocalData)
            {
                Assert.IsTrue(!string.IsNullOrEmpty(PlayerId));
                Debug.Log($"<color=yellow>[DataCtrler] Try loading local player data...[{PlayerId}] </color>");
                isFoundLocalData = isMetaData ? await metaDataGatewayService.ReadData(PlayerId, META_DATA_KEY) : await gameDataGatewayService.ReadData(PlayerId, gameDataKey);
                Debug.Log($"<color=yellow>[DataCtrler] Local player data load has been {isFoundCloudData}. </color>");
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
                if(isFoundLocalData)
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
