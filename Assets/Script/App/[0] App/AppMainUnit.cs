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
    EventsGroup Events = new EventsGroup();

    IdleMinerContext IMContext => _minerContext as IdleMinerContext;
   

    protected override void Awake() 
    { 
        base.Awake();
        
        _minerContext = new IdleMinerContext(AuthService, CloudService);
        
        IMContext.Init(this);
        IMContext.AddData("AppConfig", appConfig);

        Init(_minerContext);
        Events.RegisterEvent(EventID.PLAYER_HAS_SIGNEDIN_OR_TIMED_OUT, EventOnSignedInOrTimeOut);

        unitSwitcher.Init(_minerContext);

        Application.targetFrameRate = 61;
    }
    

    private void OnApplicationQuit()
    {
        Debug.Log("Application Quit.");
    }

    void EventOnSignedInOrTimeOut(object data)
    {
        LoadAppMetaDataModel((Action)data).Forget();
    }

    async Task LoadAppMetaDataModel(Action onFinished)
    {   
        Dispose();

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

        onFinished?.Invoke();
    }

    public override void Dispose()
    {
        playerModel?.Dispose();
        base.Dispose();
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