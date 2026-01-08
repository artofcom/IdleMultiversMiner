using App.GamePlay.IdleMiner;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using IGCore.SubSystem.Analytics;

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

    // LobbyScreenController.EventOnBtnStart() -> AUnitSwitcher.OnEventClose() -> Attach()
    public override async void Attach()
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
    
        await context.InitGame();

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
        UnitSwitcher.SwitchUnit(nextUnit);

        // Detach and Destory Game Module.
        if(idleMinerModule != null )
        {
            idleMinerModule.Detach();

            // DO NOT allow to save any game data after this point.
            IMContext.SavePlayerDataInstantly();
            IMContext.LockGatewayService(isMetaData:false, lock_it:true);
            IMContext.StopGatewaySaveDog(isMetaData:false);

            Destroy(idleMinerModule.gameObject);
        }
        idleMinerModule = null;
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

    /*
    PlayScreenView _view;
    PlayScreenController _controller;
    GameContext _context;


    //TopUIScreen _TopUIScreen;
    //BottomUIScreen _BottomUIScreen;

    public PlayScreen(AView view, IContext context)
    {
        _view = view as PlayScreenView;
        _context = context as GameContext;
    }


    public void Initialize()
    {
        // _context.Gametype == something...
        //_controller = new WordTileSearchController(_view, _context);
        //else
        //_controller = new XXXController...();

        _controller = new PlayScreenController(_view, _context);

        //_TopUIScreen = new TopUIScreen(_view.TopUIScreenView, _context);
        //_TopUIScreen.Initialize();
        //_BottomUIScreen = new BottomUIScreen(_view.BottomUIScreenView, _context);
        //_BottomUIScreen.Initialize();
    }*/
}
