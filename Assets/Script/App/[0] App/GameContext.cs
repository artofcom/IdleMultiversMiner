using IGCore.MVCS;
using IGCore.PlatformService.Cloud;
using UnityEngine;
using UnityEngine.Assertions;
using System.Threading.Tasks;

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
    IDataGatewayService metaDataGatewayService;
    IDataGatewayService gameCoreGatewayService;

    // Game Specific Data.
    ResourceDataBuildComp gameResourceDataBuildComp;

    public MonoBehaviour CoRunner => coRunner;
    
    public IDataGatewayService GameCoreGatewayService => gameCoreGatewayService;
    public IDataGatewayService MetaDataGatewayService => metaDataGatewayService;
    
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
        gameCoreGatewayService = new DataGatewayService();
        metaDataGatewayService = new DataGatewayService();
    }

    public IdleMinerContext(ICloudService cloudService)
    {
        _instance = this;
        gameCoreGatewayService = new DataCloudGatewayService(cloudService);
        metaDataGatewayService = new DataCloudGatewayService(cloudService);
    }

    public void Init(MonoBehaviour runner)
    {
        if(!IsSimulationMode()) 
        {
            spriteHolder = GameObject.FindFirstObjectByType<SpritesHolderGroupComp>(FindObjectsInactive.Include);   
            coRunner = runner;
            
            metaDataGatewayService.ClearModels();
            LoadMetaData();
            
            Assert.IsNotNull(spriteHolder);
        }
    }

    public override async Task InitGame()
    {
        gameCoreGatewayService.ClearModels();
        await LoadPlayerData();
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
    }
    async Task LoadPlayerData()
    {   
        string dataKey = $"{GameKey}_PlayerData";
        await gameCoreGatewayService.ReadData(dataKey);
    }
    public async Task ResetPlayerData()
    {
        string dataKey = $"{GameKey}_PlayerData";
        await gameCoreGatewayService.WriteData(dataKey, clearAll:true);
    }
    async Task LoadMetaData()
    {
        string dataKey = "MetaData";
        await metaDataGatewayService.ReadData(dataKey);
    }
    public async Task SaveMetaData()
    {
        string dataKey = "MetaData";
        await metaDataGatewayService.WriteData(dataKey, clearAll:false);
    }
    #endregion
}