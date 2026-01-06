using App.GamePlay.IdleMiner.Common.Types;
using IGCore.MVCS;
using IGCore.PlatformService;
using IGCore.PlatformService.Cloud;
using System.Collections.Generic;

//using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
//using App.GamePlay.IdleMiner.Common.Types;
//using IGCore.PlatformService;

public class SimDefines
{
    //public const string CRAFT_REQ_RESOURCES = "craftRequiredResources";
    //public const string SKILLTREE_REQ_RESOURCES = "skillTreeRequiredResources";
    public const string SIM_SKILLTREE_EASIEST_NODES_REQ_RESOURCES = "sim_skillTreeEasiestNodesRequiredResources";
    public const string SIM_SKILLTREE_TARGET_NODE_NAME = "sim_skillTreeEasiestTargetNodeName";
    public const string SIM_RECIPES_IN_CRAFT_CHAIN = "sim_recipesInCraftChain";
}


public sealed partial class IdleMinerContext : AContext
{   
    MonoBehaviour coRunner;   
    SpritesHolderGroupComp spriteHolder;

    // Game Specific Data.
    ResourceDataBuildComp gameResourceDataBuildComp;

    public MonoBehaviour CoRunner => coRunner;
    
    public List<IDataGatewayService> MetaGatewayServiceList => dataController?.MetaGatewayServiceList;
    public List<IDataGatewayService> GameGatewayServiceList => dataController?.GameGatewayServiceList;
    public int TargetMetaDataGatewayServiceIndex => dataController.TargetMetaDataGatewayServiceIndex;
    public int TargetGameDataGatewayServiceIndex => dataController.TargetGameDataGatewayServiceIndex;

    GameDataController dataController;
    bool isPlayingGame = false;

    static IdleMinerContext _instance;
    public static string GameKey
    {
        get
        {
#if UNITY_EDITOR
            if(Application.isPlaying)
            {
                Assert.IsTrue(false==string.IsNullOrEmpty( (string)_instance.GetData("gameKey") ), "GameKey should not be empty!" );
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
        //dataController = new GameDataController(this);
        //gameCoreGatewayService = new DataGatewayService();
        //metaDataGatewayService = new DataGatewayService();
    }

    public IdleMinerContext(IAuthService authService, ICloudService cloudService)
    {
        _instance = this;
        dataController = new GameDataController(this, authService, cloudService);
       
    }

    public void Init(MonoBehaviour runner)
    {
        if(!IsSimulationMode()) 
        {
            spriteHolder = GameObject.FindFirstObjectByType<SpritesHolderGroupComp>(FindObjectsInactive.Include);   
            coRunner = runner;
            
            dataController.Init();

            // LoadMetaData();            
            Assert.IsNotNull(spriteHolder);
        }
    }

    public override async Task InitGame()
    {
        isPlayingGame = true;
        await dataController.InitGame();
    }

    public override void DisposeGame()
    {
        dataController.DisposeGame();

        gameResourceDataBuildComp = null; 
        isPlayingGame = false;
    }

    
    
    #region DataController Wrapper
    
    public void SavePrevPlayerId(string playerId)
    {
        dataController?.SavePrevPlayerId(playerId);
    }

    public async Task<bool> LoadUserDataAsync(bool isMetaData)
    {
        return await dataController.LoadUserDataAsync(isMetaData);
    }
    public void SaveMetaDataInstantly()
    {
        dataController?.SaveMetaDataInstantly();
    }
    public void SavePlayerDataInstantly()
    {
        dataController?.SavePlayerDataInstantly();
    }
    public void RunGameDataSaveDog()
    {
        dataController.RunGameDataSaveDog();
    }
    public void ResetPlayerData()
    {
        dataController?.ResetPlayerData();
    }
    #endregion


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

}