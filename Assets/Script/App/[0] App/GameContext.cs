using IGCore.MVCS;
using IGCore.PlatformService.Cloud;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

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
        int idxGateway = ValidGatewayServiceIndex;

        gameCoreGatewayService.ClearModels();
        await LoadPlayerData(idxGateway);
    }

    public override void DisposeGame()
    {
        gameResourceDataBuildComp = null; 
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
    public async Task SavePlayerData()
    {
        string dataKey = $"{GameKey}_PlayerData";
        
        await gameCoreGatewayService.WriteData(dataKey, clearAll:false);
        Debug.Log("<color=blue>[Data] Storing Player Data in Local has been successed.</color>");

        bool ret = await gameCoreCloudGatewayService.WriteData(dataKey, clearAll:false);
        if(ret)
            Debug.Log("<color=green>[Data] Storing Player Data in Cloud has been successed.</color>");
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
    public async Task SaveMetaData()
    {
        string dataKey = "MetaData";

        await metaDataGatewayService.WriteData(dataKey, clearAll:false);
        Debug.Log("<color=blue>[Data] Storing Meta Data in Local has been successed.</color>");

        bool ret = await metaDataCloudGatewayService.WriteData(dataKey, clearAll:false);
        if(ret)
            Debug.Log("<color=green>[Data] Storing Meta Data in Cloud has been successed.</color>");
    }
    public int GetLatestMetaDataIndex()
    {
        long localDataTS = (metaDataGatewayService as DataGatewayService).ServiceData.Environment.TimeStamp;
        long cloudDataTS = (metaDataCloudGatewayService as DataGatewayService).ServiceData.Environment.TimeStamp;

        return localDataTS >= cloudDataTS ? IDX_LOCA_DATA_SERVICE : IDX_CLOUD_DATA_SERVICE; 
    }
    // 
    public void SetSignedInPlayerId(string playerId)
    {
        // Set account id for the gateway services to nativate file paths.
        MetaDataGatewayService.AccountId = playerId;
        MetaDataCloudGatewayService.AccountId = playerId;
        GameCoreGatewayService.AccountId = playerId;
        GameCoreCloudGatewayService.AccountId = playerId;
    }
    #endregion
}