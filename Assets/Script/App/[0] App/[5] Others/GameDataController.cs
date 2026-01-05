using App.GamePlay.IdleMiner.Common.Types;
using IGCore.MVCS;
using IGCore.PlatformService;
using IGCore.PlatformService.Cloud;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEditor.ShaderData;

public sealed partial class IdleMinerContext : AContext
{
    //
    // Core class of data handler. 
    //
    // Internal class so it can access to all private members of the IdleMinerContext.
    public class GameDataController
    {
        const int IDX_LOCA_DATA_SERVICE = 0;
        const int IDX_CLOUD_DATA_SERVICE = 1;

        IdleMinerContext contextCache;

        ILocalDataGatewayService metaDataGatewayService, gameCoreGatewayService;
        ICloudDataGatewayService metaDataCloudGatewayService, gameCoreCloudGatewayService;

        // !!! Service SubScriber PlayerModel should fetch data via this index.
        public int TargetMetaDataGatewayServiceIndex { get; private set; } = -1;
        public int TargetGameDataGatewayServiceIndex { get; private set; } = -1;

        List<IDataGatewayService> metaGatewayServiceList;
        public List<IDataGatewayService> MetaGatewayServiceList => metaGatewayServiceList;
        
        List<IDataGatewayService> gameGatewayServiceList;
        public List<IDataGatewayService> GameGatewayServiceList => gameGatewayServiceList;
        
        const string META_DATA_KEY = "MetaData";
        const string DEVICE_GUEST = "device_guest_id";
        //AccountStatus eAccountStatus = AccountStatus.UNKNOWN;
        IAuthService authService;
        ICloudService cloudService;

        public GameDataController(IdleMinerContext context, IAuthService authService, ICloudService cloudService)
        {
            contextCache = context;

            this.authService = authService;
            this.cloudService = cloudService;
            gameCoreGatewayService = new DataGatewayService();
            metaDataGatewayService = new DataGatewayService();
            gameCoreCloudGatewayService = new DataCloudGatewayService(cloudService);
            metaDataCloudGatewayService = new DataCloudGatewayService(cloudService);

            gameGatewayServiceList = new List<IDataGatewayService>() { gameCoreGatewayService, gameCoreCloudGatewayService };      
            metaGatewayServiceList = new List<IDataGatewayService>() { metaDataGatewayService,  metaDataCloudGatewayService }; 
        }

        public void Init()   {}

        public void InitOnSignIn()
        {
            /*string prevSignedPlayerId = PlayerPrefs.GetString(DataKeys.PREV_PLAYER_ID, string.Empty);
            Debug.Log($"<color=green>[AppMain] Singed PlayerId [{prevSignedPlayerId}] / [{curSignedPlayerId}] </color>");

            string dataLocation = isOnline ? "CLOUD" : "LOCAL";
            Debug.Log($"<color=green>[AppMain] Target Data Location : [{dataLocation}]</color>");

            if(!isOnline)
                curSignedPlayerId = DEVICE_GUEST;

            /*
            eAccountStatus = GetAccountStatusById(prevSignedPlayerId, curSignedPlayerId);
            switch(eAccountStatus)
            {
            case AccountStatus.NULL_2_ID_A:             // [Cloud] Signed In Player.
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
                    isOnline = false;
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
        
            // Logging.
            dataLocation = isOnline ? "CLOUD" : "LOCAL";
            */
            // Debug.Log($"<color=green>[AppMain] Aligned Type [{eAccountStatus}], Target [{dataLocation}], Singed PlayerId [{prevSignedPlayerId}] / [{curSignedPlayerId}] </color>");
        
            //bool isOnline = authService.IsSignedIn(); 
            //string curSignedPlayerId = string.IsNullOrEmpty(authService.GetPlayerId()) ? DEVICE_GUEST : authService.GetPlayerId();
            //Assert.IsTrue(!string.IsNullOrEmpty(curSignedPlayerId));

            // Assign Proper Cur Signed In Player Id.
            //contextCache.UpdateData("PlayerId", curSignedPlayerId);

           // Debug.Log($"<color=green>[DataController] IsOnline [{authService.IsSignedIn()}], PlayerId [{curSignedPlayerId}]</color>");

            // Set Target Data Location.
            //contextCache.UpdateData("ShouldUseCloudData", shouldUseCloudData);   
        }

        //  상황                  네트워크           로컬(ID_A)      클라우드       게스트       선택                  판정 및 액션 (Action)
        //  1. 클린 신규           Online   0           X                X            X         Local       [신규 생성] 아무 데이터도 없으므로 새 시작
        //  3. 게스트 연동         Online   0           X                X            O         Local       [마이그레이션] 게스트 -> ID_A 복사 후 게스트 삭제
        //  4. 기기 변경           Online   0           X                O            X         Cloud       [다운로드] 클라우드 데이터를 ID_A에 내려받음
        //  7. 데이터 충돌         Online   0           X                O            O         Latter      [최신데이터 선택]
        
        //  -. 단순 이어하기       Online   0           O                X            X         Local       [업로드] ID_A 를 클라우드 업로드
        //  5. 단순 이어하기       Online   0           O                X            O         Latter      [최신데이터 선택]
        //  -. 단순 이어하기       Online   0           O                O            X         Latter      [최신데이터 선택]
        //  8. 복합 충돌           Online   0           O                O            O         Latter      [최신데이터 선택]
        
        // 클라우드 파일얻기 실패  Online   O           O              미확인         X         Local       로컬에만 업데이트 & timestamp 고정. 이후 온라인시 클라우드가 최신이면 user에게 물어봄.
        //                         Online   O           X              미확인         X          X          Retry 후 네트워크 상태 확인 창.
        //                         Online   O           X              미확인         O          X          Retry 후 네트워크 상태 확인 창.

        //  0. 오프라인 신규       Offline  X           X              미확인         X         Local       [게스트] 신규 Device 게스트 Player.
        //  2. 오프라인 게스트     Offline  X           X              미확인         O         Local       [게스트 유지] 오프라인이므로 게스트 ID로 플레이
        //  6. 오프라인 복귀       Offline  X           O              미확인         X         Local       [로컬 우선] 네트워크 확인 불가하므로 ID_A 로드
        
        

        public async Task SelectDataLocationAsync()
        {
            bool isOnline = authService.IsSignedIn();
            bool isFoundLocalData = false;
            bool isFoundCloudData = true;
            bool isSuccessedFetchingCloudData = true;
            bool isFoundLocalGuestData = false;


            TargetMetaDataGatewayServiceIndex = isOnline ? IDX_CLOUD_DATA_SERVICE : IDX_LOCA_DATA_SERVICE;

            SetSignedInPlayerIdForGatewayServices(DEVICE_GUEST);
            isFoundLocalGuestData = await metaDataGatewayService.ReadData(META_DATA_KEY);

            string curSignedPlayerId = authService.GetPlayerId();             

            if (isOnline) 
            {
                Assert.IsTrue(!string.IsNullOrEmpty(curSignedPlayerId));
                SetSignedInPlayerIdForGatewayServices(curSignedPlayerId);

                var loadTask = metaDataCloudGatewayService.ReadData(META_DATA_KEY);   //CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
                var timeoutTask = Task.Delay(5000);

                var completedTask = await Task.WhenAny(loadTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    Debug.LogWarning("[Cloud] Meta Data Server response timeout !");
                    isSuccessedFetchingCloudData = false;
                }
                else
                {
                    if(loadTask.Result == ICloudService.ResultType.eDataNotFound)
                        isFoundCloudData = false;
                    if(loadTask.Result != ICloudService.ResultType.eSuccessed)
                        isSuccessedFetchingCloudData = false;
                    else
                        isFoundLocalData = await metaDataGatewayService.ReadData(META_DATA_KEY);
                }

                
                if(!isSuccessedFetchingCloudData)
                {
                    TargetMetaDataGatewayServiceIndex = IDX_LOCA_DATA_SERVICE;
                }
                else
                {
                    if(isFoundLocalData)
                    {
                        if(isFoundCloudData)
                        {
                            if(isFoundLocalGuestData)
                                ; //TargetMetaDataGatewayServiceIndex = Compare_Cloud_Guest
                            else 
                                TargetMetaDataGatewayServiceIndex = IDX_CLOUD_DATA_SERVICE;
                        }
                        else 
                            TargetMetaDataGatewayServiceIndex = IDX_LOCA_DATA_SERVICE;
                    }
                    else // ! isFoundLocalData
                    {
                        if(isFoundCloudData)
                            ; // compare.
                        else 
                            TargetMetaDataGatewayServiceIndex = IDX_LOCA_DATA_SERVICE;
                    }
                }
               // string target = TargetMetaDataGatewayServiceIndex == IDX_LOCA_DATA_SERVICE ? "LOCAL" : "CLOUD";
               // Debug.Log($"[AppMain] Data Selector : [{target}] data has been selected for MetaData.");
            }
            else
            {
                if(!string.IsNullOrEmpty(curSignedPlayerId))
                {
                    SetSignedInPlayerIdForGatewayServices(curSignedPlayerId);

                    isFoundLocalData = await FetchLocalData();
                    if(isFoundLocalData == false)
                    {
                        // Time to Reset Your game data.
                        Debug.Log("<color=red>[AppMain] Failed to read local data.. Resetting All Data...</color>");
                    }
                }

                TargetMetaDataGatewayServiceIndex = IDX_LOCA_DATA_SERVICE;
            }
        }

    
        public async Task InitGame()
        {
            gameCoreGatewayService.ClearModels();
            gameCoreCloudGatewayService.ClearModels();

            // fix here = AddData selection logic.
            bool shouldUseCloudData = (bool)contextCache.GetData("ShouldUseCloudData", false);
        
            TargetGameDataGatewayServiceIndex = shouldUseCloudData ? IDX_CLOUD_DATA_SERVICE : IDX_LOCA_DATA_SERVICE;
            if(shouldUseCloudData)
            {
                bool ret = await LoadPlayerData(IDX_CLOUD_DATA_SERVICE);
                while(Application.isPlaying && !ret)
                {
                    await Task.Delay(1000);
                    Debug.Log("[Contex] Try Load GameData in Cloud.");
                    ret = await LoadPlayerData(IDX_CLOUD_DATA_SERVICE);
                }

                bool fetchLocalData = await LoadPlayerData(IDX_LOCA_DATA_SERVICE);
            
                TargetGameDataGatewayServiceIndex = fetchLocalData==false ? IDX_CLOUD_DATA_SERVICE : GetLatestGameDataIndex();

                string target = TargetGameDataGatewayServiceIndex == IDX_LOCA_DATA_SERVICE ? "LOCAL" : "CLOUD";
                Debug.Log($"[Context] Data Selector : [{target}] data has been selected for GamePlayerData.");
            }
            else
                await LoadPlayerData(TargetGameDataGatewayServiceIndex);
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
                bool ret = await gameCoreGatewayService.WriteData(dataKey, clearAll);
                if(ret)
                {
                    Debug.Log("<color=blue>[Data] Storing Player Data in Local has been successed.</color>");
                    return true;
                }
            }
            else
            {
                if(ICloudService.ResultType.eSuccessed == await gameCoreCloudGatewayService.WriteData(dataKey, clearAll))
                {
                    Debug.Log("<color=green>[Data] Storing Player Data in Cloud has been successed.</color>");
                    return true;
                }
            }
            return false;
        }
        async Task<bool> LoadPlayerData(int idxGatewayService)
        {   
            if(idxGatewayService<0 || idxGatewayService>=MetaGatewayServiceList.Count)
            {
                Assert.IsTrue(false, "Invalid MetaGateWayService Index.." + idxGatewayService);
                return false;
            }

            IDataGatewayService dataGatewayService = GameGatewayServiceList[idxGatewayService];

            string dataKey = $"{GameKey}_PlayerData";
            if(idxGatewayService == IDX_LOCA_DATA_SERVICE)
            {
                return await (dataGatewayService as ILocalDataGatewayService).ReadData(dataKey);
            }
            else
            {
                return (ICloudService.ResultType.eSuccessed == await (dataGatewayService as ICloudDataGatewayService).ReadData(dataKey));
            }
        }
        public void ResetPlayerData()
        {
            SavePlayerData(isLocal:true, clearAll:true).Forget();
            SavePlayerData(isLocal:false, clearAll:true).Forget();
        }
        public async Task<bool> LoadMetaData(int idxGatewayService)
        {
            if(idxGatewayService<0 || idxGatewayService>=MetaGatewayServiceList.Count)
            {
                Assert.IsTrue(false, "Invalid MetaGateWayService Index.." + idxGatewayService);
                return false;
            }
        
            IDataGatewayService dataGatewayService = MetaGatewayServiceList[idxGatewayService];

            string dataKey = "MetaData";
            if(idxGatewayService == IDX_LOCA_DATA_SERVICE)
            {
                return await (dataGatewayService as ILocalDataGatewayService).ReadData(dataKey);
            }
            else
            {
                return ICloudService.ResultType.eSuccessed == await (dataGatewayService as ICloudDataGatewayService).ReadData(dataKey);
            }
        }
        async Task<bool> SaveMetaData(bool isLocal)
        {
            string dataKey = "MetaData";

            if(isLocal)
            {
                bool ret = await metaDataGatewayService.WriteData(dataKey, clearAll:false);
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
        public int GetLatestMetaDataIndex()
        {
            Assert.IsNotNull(metaDataGatewayService);
            Assert.IsNotNull((metaDataGatewayService as DataGatewayService));
            Assert.IsNotNull((metaDataGatewayService as DataGatewayService).ServiceData);
            Assert.IsNotNull((metaDataGatewayService as DataGatewayService).ServiceData.Environment);

            Assert.IsNotNull(metaDataCloudGatewayService);
            Assert.IsNotNull((metaDataCloudGatewayService as DataGatewayService));
            Assert.IsNotNull((metaDataCloudGatewayService as DataGatewayService).ServiceData);
            Assert.IsNotNull((metaDataCloudGatewayService as DataGatewayService).ServiceData.Environment);

            long localDataTS = (metaDataGatewayService as DataGatewayService).ServiceData.Environment.TimeStamp;
            long cloudDataTS = (metaDataCloudGatewayService as DataGatewayService).ServiceData.Environment.TimeStamp;

            return localDataTS >= cloudDataTS ? IDX_LOCA_DATA_SERVICE : IDX_CLOUD_DATA_SERVICE; 
        }
        public int GetLatestGameDataIndex()
        {
            Assert.IsNotNull(gameCoreGatewayService);
            Assert.IsNotNull((gameCoreGatewayService as DataGatewayService));
            Assert.IsNotNull((gameCoreGatewayService as DataGatewayService).ServiceData);
            Assert.IsNotNull((gameCoreGatewayService as DataGatewayService).ServiceData.Environment);

            Assert.IsNotNull(gameCoreCloudGatewayService);
            Assert.IsNotNull((gameCoreCloudGatewayService as DataGatewayService));
            Assert.IsNotNull((gameCoreCloudGatewayService as DataGatewayService).ServiceData);
            Assert.IsNotNull((gameCoreCloudGatewayService as DataGatewayService).ServiceData.Environment);

            long localDataTS = (gameCoreGatewayService as DataGatewayService).ServiceData.Environment.TimeStamp;
            long cloudDataTS = (gameCoreCloudGatewayService  as DataGatewayService).ServiceData.Environment.TimeStamp;

            return localDataTS >= cloudDataTS ? IDX_LOCA_DATA_SERVICE : IDX_CLOUD_DATA_SERVICE; 
        }
        // 
        public void SetSignedInPlayerIdForGatewayServices(string playerId)
        {
            // Set account id for the gateway services to nativate file paths.
            metaDataGatewayService.AccountId = playerId;
            metaDataCloudGatewayService.AccountId = playerId;
            gameCoreGatewayService.AccountId = playerId;
            gameCoreCloudGatewayService.AccountId = playerId;
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

        async Task<bool> FetchLocalData()
        {
            await Task.Delay(100);
        
            bool ret = await LoadMetaData( IDX_LOCA_DATA_SERVICE );

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

            bool ret  = await LoadMetaData( IDX_CLOUD_DATA_SERVICE );
            return ret;
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
