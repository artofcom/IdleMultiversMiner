using App.GamePlay.IdleMiner;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using IGCore.SubSystem.Analytics;
using System.Threading.Tasks;

[Serializable]
public class GameKeyPathSet
{
    [SerializeField] string key;
    [SerializeField] string path;

    public string Key => key;
    public string Path => path;
}

public class PlayScreen : AUnit
{
    [ImplementsInterface(typeof(IAnalyticsService))]
    [SerializeField] ScriptableObject analyticsService;

    [SerializeField] List<GameKeyPathSet> gameKeyPathSets;
    [SerializeField] Transform transformParent;

    [ImplementsInterface(typeof(IUnitSwitcher))]
    [SerializeField] MonoBehaviour unitSwitcher;
    
    AUnit idleMinerModule;
    IUnitSwitcher UnitSwitcher => unitSwitcher as IUnitSwitcher;
    Dictionary<string, string> dictGameKeyPathSets = new Dictionary<string, string>();
    IdleMinerContext IMContext;

    bool isGameDataLoadingCompleted = false;
    bool IsGameDataLoadingCompleted()  { return isGameDataLoadingCompleted; }

    // DictorMain.Start() -> AUnitSwitcher.Init() -> PlayerScreen.Init()
    public override void Init(AContext ctx)
    {
        base.Init(ctx);

        IMContext = ctx as IdleMinerContext;
        model = new PlayScreenModel(ctx, null);
        controller = new PlayScreenController(this, view, model, ctx);

        model.Init();
        controller.Init();

        dictGameKeyPathSets.Clear();
        Assert.IsTrue(gameKeyPathSets!=null &&  gameKeyPathSets.Count>0);
        for(int q = 0; q < gameKeyPathSets.Count; ++q)
        {
            dictGameKeyPathSets.Add(gameKeyPathSets[q].Key.ToLower(), gameKeyPathSets[q].Path);
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        IMContext = null;
        dictGameKeyPathSets?.Clear();
    }

    // LobbyScreenController.EventOnBtnStart() -> AUnitSwitcher.OnEventClose() -> Attach()
    public override void Attach()
    {
        // Temp Code. 
        string gameKey = (string) this.context.GetData("gameKey");
        if(!dictGameKeyPathSets.ContainsKey(gameKey))
        {
            Assert.IsTrue(false, "Invalid Game Key..." + gameKey);
            return;
        }
        
        IMContext.AddData("gamePath", dictGameKeyPathSets[gameKey]);

        string mainPrefabPath = dictGameKeyPathSets[gameKey] + "/MainUnit";
        GameObject prefabGamePlayModule = Resources.Load<GameObject>(mainPrefabPath);
    
        //await context.InitGame();

        idleMinerModule = (Instantiate(prefabGamePlayModule, transformParent)).GetComponent<AUnit>();
        Assert.IsNotNull(idleMinerModule);   
        ((IdleMinerContext)context).AddData("gamePlayModule", idleMinerModule);

        base.Attach();

        // Idle Miner Module.
        idleMinerModule.Init(context);
        idleMinerModule.Attach();
    
        IMContext.LockGatewayService(isMetaData:false, lock_it:false);
        IMContext.RunGameDataSaveDog();

        (analyticsService as IAnalyticsService)?.SendEvent($"GamePlayStarted_{gameKey}");
    }

    public override void Detach()
    {
        context.DisposeGame();

        base.Detach();   
    }

    public void SwitchUnit(string nextUnit)
    {
        if(idleMinerModule != null )
        {
            idleMinerModule.Detach();

            // DO NOT allow to save any game data after this point.
            IMContext.LockGatewayService(isMetaData:false, lock_it:true);
            IMContext.StopGatewaySaveDog(isMetaData:false);

            DestroyImmediate(idleMinerModule.gameObject);
        }
        idleMinerModule = null;

        if(nextUnit.ToLower().Contains("playscreen"))
        {
            ConductGameInitProcess().Forget();
            UnitSwitcher.SwitchUnit(nextUnit, (Func<bool>)IsGameDataLoadingCompleted);
        }
        else 
            UnitSwitcher.SwitchUnit(nextUnit, null);
    }

    async Task ConductGameInitProcess()
    {
        isGameDataLoadingCompleted = false;

        try
        {
            await IMContext.InitGame();
        }
        finally
        { 
            isGameDataLoadingCompleted = true;
        }
    }


#if UNITY_EDITOR

    static string[] gameKeys = { "Gravewardens_Harvest_Test0", "Swamp", "Desert" };
    static int idxCurGameKey = 0;
    
    public static string EditorGameKey =>
        (idxCurGameKey>=0 && idxCurGameKey<gameKeys.Length) ?  gameKeys[idxCurGameKey].ToLower() : string.Empty;
    

    [InitializeOnLoadMethod]
    static void InitializeMenuState()   
    {   
       // Debug.Log($"Initializing editor selected game key......");
      //  SetCurGameKeyIndex(idxCurGameKey);  
    }

    /*
    [MenuItem("PlasticGames/Active Game/Gravewardens_Harvest", false, 0)]
    public static void SetGravesMode()  { SetCurGameKeyIndex(0); }
    
    [MenuItem("PlasticGames/Active Game/Swamp", false, 1)]
    public static void SetSwampMode()  { SetCurGameKeyIndex(1); }

    [MenuItem("PlasticGames/Active Game/Desert", false, 2)]
    public static void SetDesertMode()  { SetCurGameKeyIndex(2); }
    

    static void SetCurGameKeyIndex(int idx)
    {
        if(idx>=0 && idx<gameKeys.Length) 
        {
            idxCurGameKey = idx;

            for(int q = 0; q < gameKeys.Length; q++) 
            {
                Menu.SetChecked($"PlasticGames/Active Game/{gameKeys[q]}", idxCurGameKey==q);
                Debug.Log($"{q} set to {idxCurGameKey==q}");
            }
            Debug.Log($"Active mode set to : {gameKeys[idxCurGameKey]}");
        }
    }*/
#endif

}
