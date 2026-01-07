using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using IGCore.MVCS;
using IGCore.PlatformService.Cloud;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

public class AppMainUnit : AUnit
{    
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
    bool isWaitingForSignIn = true;
   

    protected override void Awake() 
    { 
        base.Awake();
        
        AuthService.EventOnSignedIn += OnSignedIn;
        AuthService.EventOnSignInFailed += OnSignInFailed;
        AuthService.EventOnSignOut += OnSignedOut;

        _minerContext = new IdleMinerContext(AuthService, CloudService);
        
        IMContext.Init(this);
        IMContext.AddData("AppConfig", appConfig);

        Init(_minerContext);

        unitSwitcher.Init(_minerContext);
    }
    protected async void Start()
    {
        Application.targetFrameRate = 61;

        var signInTask = WaitUntil( () => isWaitingForSignIn==false );
        var timeOut = Task.Delay(MaxNetworkWaitSec * 1000);

        await Task.WhenAny(signInTask, timeOut);

        LoadAppMetaDataModel( (string)context.GetData("PlayerId", string.Empty) ).Forget();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application Quit.");
    }

    async Task LoadAppMetaDataModel(string curSignedPlayerId)
    {   
        playerModel = new AppPlayerModel(_minerContext, IMContext.MetaGatewayServiceList);
        model = new AppModel(_minerContext, playerModel);
        controller = new AppController(this, view, model, _minerContext);

        bool isDone = false;
        while(Application.isPlaying && !isDone)
        {
            isDone = await IMContext.LoadUserDataAsync(isMetaData:true);
            await Task.Delay(1000);
            Debug.Log("Try Selecting Load Meta Data....Local / Cloud / Guest..");
        }

        // Init Modulels with loading data.
        model.Init();
        controller.Init();
        playerModel.Init();
        
        if(metaSystems != null)
        {
            for(int q = 0; q < metaSystems.Count; q++) 
                metaSystems[q].Init(_minerContext);
        }

        //IMContext.RunMetaDataSaveDog();
        // Make sure to sync cloud data with the local one.
        //if(shouldUseCloudData && IMContext.TargetMetaDataGatewayServiceIndex==IdleMinerContext.IDX_LOCA_DATA_SERVICE)
        //{
        //    await Task.Delay(1000);
        //    playerModel.SetDirty();
        //}

        EventSystem.DispatchEvent(EventID.APPLICATION_PLAYERDATA_INITIALIZEDD);
    }



    async void OnSignedIn(string playerId)
    {
        context.UpdateData("PlayerId", playerId);
        context.AddData("IsAccountLinked", AuthService.IsAccountLinkedWithPlayer("unity"));

        await Task.Delay(100);

        isWaitingForSignIn = false;
    }
    void OnSignInFailed(string reason)
    {
        isWaitingForSignIn = false;
    }
    void OnSignedOut() 
    { 
        isWaitingForSignIn = true;
    }
    async Task WaitUntil(Func<bool> predicate)
    {
        while (Application.isPlaying && !predicate())
        {
            await Task.Delay(100); 
        }
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