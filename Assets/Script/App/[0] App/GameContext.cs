using IGCore.MVCS;
using IGCore.PlatformService.Cloud;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class SimDefines
{
    //public const string CRAFT_REQ_RESOURCES = "craftRequiredResources";
    //public const string SKILLTREE_REQ_RESOURCES = "skillTreeRequiredResources";
    public const string SIM_SKILLTREE_EASIEST_NODES_REQ_RESOURCES = "sim_skillTreeEasiestNodesRequiredResources";
    public const string SIM_SKILLTREE_TARGET_NODE_NAME = "sim_skillTreeEasiestTargetNodeName";
    public const string SIM_RECIPES_IN_CRAFT_CHAIN = "sim_recipesInCraftChain";
}


public sealed class IdleMinerContext : AContext
{   
    MonoBehaviour coRunner;   
    SpritesHolderGroupComp spriteHolder;
    IDataGatewayService metaDataGatewayService, metaDataCloudGatewayService;
    IDataGatewayService gameCoreGatewayService, gameCoreCloudGatewayService;

    // Game Specific Data.
    ResourceDataBuildComp gameResourceDataBuildComp;

    public MonoBehaviour CoRunner => coRunner;
    
    public IDataGatewayService GameCoreGatewayService => gameCoreGatewayService;
    public IDataGatewayService MetaDataGatewayService => metaDataGatewayService;
    public IDataGatewayService GameCoreCloudGatewayService => gameCoreCloudGatewayService;
    public IDataGatewayService MetaDataCloudGatewayService => metaDataCloudGatewayService;

    public const int IDX_LOCA_DATA_SERVICE = 0;
    public const int IDX_CLOUD_DATA_SERVICE = 1;

    List<IDataGatewayService> metaGatewayServiceList;
    public List<IDataGatewayService> MetaGatewayServiceList
    {
        get
        { 
            if(metaGatewayServiceList == null)
            {
                metaGatewayServiceList = new List<IDataGatewayService>() { 
                                                MetaDataGatewayService,             // 0 - Local Data.
                                                MetaDataCloudGatewayService };      // 1 - Cloud Data.
            }
            return metaGatewayServiceList;
        }
    }
    List<IDataGatewayService> gameGatewayServiceList;
    public List<IDataGatewayService> GameGatewayServiceList
    {
        get
        { 
            if(gameGatewayServiceList == null)
            {
                gameGatewayServiceList = new List<IDataGatewayService>() { 
                                                GameCoreGatewayService,             // 0 - Local Data.
                                                GameCoreCloudGatewayService };      // 1 - Cloud Data.
            }
            return gameGatewayServiceList;
        }
    }

    // !!! Service SubScriber PlayerModel should fetch data via this index.
    public int ValidGatewayServiceIndex { get; set; } = -1;


    bool isPlayingGame = false;

    static IdleMinerContext _instance;
    public static string GameKey
    {
        get
        {
#if UNITY_EDITOR
            if(Application.isPlaying)
            {
                Assert.IsTrue(false == string.IsNullOrEmpty( (string)_instance.GetData("gameKey") ));
                return (string)_instance.GetData("gameKey");
            }
            return PlayScreen.EditorGameKey;
#else
            return (string)_instance.GetData("gameKey");
#endif
        }
    }
    

    public IdleMinerContext()
    {
        _instance = this;
        //gameCoreGatewayService = new DataGatewayService();
        //metaDataGatewayService = new DataGatewayService();
    }

    public IdleMinerContext(ICloudService cloudService)
    {
        _instance = this;
        gameCoreGatewayService = new DataGatewayService();
        metaDataGatewayService = new DataGatewayService();
        gameCoreCloudGatewayService = new DataCloudGatewayService(cloudService);
        metaDataCloudGatewayService = new DataCloudGatewayService(cloudService);
    }

    public void Init(MonoBehaviour runner)
    {
        if(!IsSimulationMode()) 
        {
            spriteHolder = GameObject.FindFirstObjectByType<SpritesHolderGroupComp>(FindObjectsInactive.Include);   
            coRunner = runner;
            
            metaDataGatewayService.ClearModels();
            // LoadMetaData();
            
            Assert.IsNotNull(spriteHolder);
        }
    }

    public override async Task InitGame()
    {
        gameCoreGatewayService.ClearModels();
        gameCoreCloudGatewayService.ClearModels();
        isPlayingGame = true;
        await LoadPlayerData(ValidGatewayServiceIndex);
    }

    public override void DisposeGame()
    {
        SavePlayerData(isLocal:true).Forget();
        SavePlayerData(isLocal:false).Forget();

        gameResourceDataBuildComp = null; 
        isPlayingGame = false;
    }


    #region ===> Sprite Accessors

    public Sprite GetSprite(string groupId, string spriteKey)
    {
        groupId = groupId.ToLower();
        if(groupId.Contains("rsc-"))
        {
            if(gameResourceDataBuildComp == null) 
                gameResourceDataBuildComp = GameObject.FindFirstObjectByType<ResourceDataBuildComp>(FindObjectsInactive.Include);
            
            if(gameResourceDataBuildComp != null)
                return gameResourceDataBuildComp?.GetSprite(groupId, spriteKey);
        }
        
        return spriteHolder.GetSprite(groupId, spriteKey.ToLower());
    }


    //public Sprite GetPlanetSprite(int zoneId, int planetId)
    //{
    //    return null;// planetComp.GetPlanetSprite(zoneId, planetId);
    //}
    
    public Sprite GetManagerSprite(string spriteKey)
    {
        return spriteHolder.GetSprite("Manager", spriteKey.ToLower());
    }

    public Sprite GetBoosterSprite(string spriteKey)
    {
        return spriteHolder.GetSprite("Booster", spriteKey.ToLower());
    }
    #endregion



    #region Data Gateway Services.
    async Task<bool> SavePlayerData(bool isLocal)
    {
        string dataKey = $"{GameKey}_PlayerData";
        
        bool ret = false;
        if(isLocal)
        {
            ret = await gameCoreGatewayService.WriteData(dataKey, clearAll:false);
            if(ret)
                Debug.Log("<color=blue>[Data] Storing Player Data in Local has been successed.</color>");
        }
        else
        {
            ret = await gameCoreCloudGatewayService.WriteData(dataKey, clearAll: false);
            if(ret)
                Debug.Log("<color=green>[Data] Storing Player Data in Cloud has been successed.</color>");
        }
        return ret;
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
        bool ret = await dataGatewayService.ReadData(dataKey);
        return ret;
    }
    public async Task ResetPlayerData()
    {
        string dataKey = $"{GameKey}_PlayerData";
        await gameCoreGatewayService.WriteData(dataKey, clearAll:true);
        await gameCoreCloudGatewayService.WriteData(dataKey, clearAll:true);
    }
    public async Task<bool> LoadMetaData(int idxGatewayService)
    {
        if(idxGatewayService<0 || idxGatewayService>=MetaGatewayServiceList.Count)
        {
            Assert.IsTrue(false, "Invalid MetaGateWayService Index.." + idxGatewayService);
            return false;
        }
        
        IDataGatewayService dataGatewayService = MetaGatewayServiceList[idxGatewayService];

        // string playerId = PlayerPrefs.GetString(DataKeys.PLAYER_ID, string.Empty);
        string dataKey = "MetaData";
        bool ret = await dataGatewayService.ReadData(dataKey);
        return ret;
    }
    async Task<bool> SaveMetaData(bool isLocal)
    {
        string dataKey = "MetaData";

        bool ret = false;
        if(isLocal)
        {
            ret = await metaDataGatewayService.WriteData(dataKey, clearAll:false);
            if(ret)
                Debug.Log("<color=blue>[Data] Storing Meta Data in Local has been successed.</color>");
        }
        else
        {
            ret = await metaDataCloudGatewayService.WriteData(dataKey, clearAll: false);
            if(ret)
                Debug.Log("<color=green>[Data] Storing Meta Data in Cloud has been successed.</color>");
        }
        return ret;
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
    // 
    public void SetSignedInPlayerIdForGatewayServices(string playerId)
    {
        // Set account id for the gateway services to nativate file paths.
        MetaDataGatewayService.AccountId = playerId;
        MetaDataCloudGatewayService.AccountId = playerId;
        GameCoreGatewayService.AccountId = playerId;
        GameCoreCloudGatewayService.AccountId = playerId;
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
                appConfig = (AppConfig)GetData("AppConfig", null);

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
        while(Application.isPlaying && isPlayingGame)
        {
            if(appConfig == null)
                appConfig = (AppConfig)GetData("AppConfig", null);

            int delay = 1000;
            if(isLocal) delay = appConfig==null ? 2 * 1000 : appConfig.GameDataSaveLocalInterval * 1000;
            else        delay = appConfig==null ? 5 * 1000 : appConfig.GameDataSaveCloudInterval * 1000;

            await Task.Delay(delay);
            await SavePlayerData(isLocal);
        }
    }

    #endregion
}