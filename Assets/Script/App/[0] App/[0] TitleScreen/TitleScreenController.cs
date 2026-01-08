using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using Core.Events;
using Core.Util;
using IGCore.MVCS;
using IGCore.PlatformService.Cloud;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class TitleScreenController : AController
{
    const float WAIT_TIME_SEC = 1.5f;

    IdleMinerContext IMContext => (IdleMinerContext)context;

    TitleScreen titleScreen => unit as TitleScreen;
    bool isWaitingForSignIn = true;

    // bool isBacgroundLoginWorking = false;

    public TitleScreenController(AUnit unit, AView view, AModel model, AContext ctx)
        : base(unit, view, model, ctx)
    {}

    public override void Init()
    {
        // isBacgroundLoginWorking = false;
        titleScreen.AuthService.EventOnSignedIn += OnSignedIn;
        titleScreen.AuthService.EventOnSignInFailed += OnSignInFailed;
        titleScreen.AuthService.EventOnSignOut += OnSignOut;
    }

    protected async override void OnViewEnable()
    {
        Debug.Log("============================= Title Enter ");

        bool isSignInAfterSignOutRequired = (bool)context.GetData("IsTitleViewLoginRequired", false);
        
        // Lock DataGateways.
        IMContext.LockGatewayService(isMetaData:true, lock_it:true);
        IMContext.LockGatewayService(isMetaData:false, lock_it:true);

        if(false == isSignInAfterSignOutRequired)   // General Case.
        {
            await ConductSignInProcess();

            EventSystem.DispatchEvent(EventID.PLAYER_HAS_SIGNEDIN_OR_TIMED_OUT, (Action)TransitToLobbyScene); 
        }
        else
        {
            // 
            // In this case, [ConductSignInProcess] will be done in the dialog.
            //
            DelayedAction.TriggerActionWithDelay(IMContext.CoRunner, WAIT_TIME_SEC, () =>
            {
                context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayUnitPopupDialog", (errMsg, ret) => { },
                    "LoginDialog",
                    new Action<APopupDialog>((popupDlg) =>
                    {
                        Debug.Log("Login Dialog has been closed.");
                        EventSystem.DispatchEvent(EventID.PLAYER_HAS_SIGNEDIN_OR_TIMED_OUT, (Action)TransitToLobbyScene);

                    }));
            });
        }
    }
    
    protected override void OnViewDisable() 
    { 
        context.UpdateData("IsTitleViewLoginRequired", false);
    }

    public override void Resume(int awayTimeInSec) { }
    
    public override void Pump() { }
    
    public override void WriteData() { }

    public override void Dispose()
    {
        base.Dispose();

        titleScreen.AuthService.EventOnSignedIn -= OnSignedIn;
        titleScreen.AuthService.EventOnSignInFailed -= OnSignInFailed;
        titleScreen.AuthService.EventOnSignOut -= OnSignOut;
    }

    void TransitToLobbyScene()
    {
        if(titleScreen.IsAttached)
            titleScreen.SwitchUnit("LobbyScreen");
    }

    async Task ConductSignInProcess()
    {
        isWaitingForSignIn = true;
        titleScreen.AuthService.SignInAsync().Forget();

        var appConfig = (AppConfig)context.GetData("AppConfig", null);
        int maxWaitTime = appConfig != null ? appConfig.MaxServiceSignInWaitTime : 5;
        
        var signInTask = WaitUntil( () => isWaitingForSignIn==false );
        var timeOut = Task.Delay(maxWaitTime * 1000);

        await Task.WhenAny(signInTask, timeOut);
    }
    


    void OnSignedIn(string playerId) 
    {
        Debug.Log($"<color=green>[TitleScreen] SignIn Successed. PlayerId : [{playerId}]</color>");
        
        isWaitingForSignIn = false;
        context.UpdateData("PlayerId", playerId);
        context.AddData("IsAccountLinked", titleScreen.AuthService.IsAccountLinkedWithPlayer("unity"));

        string playerType = titleScreen.AuthService.IsAccountLinkedWithPlayer("unity") ? "Player" : "Guest";
        var presentData = new ToastMessageDialog.PresentInfo( message :  $"{playerType} [{playerId}] has signed in.", duration:3.0f, ToastMessageDialog.Type.INFO);
        context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
            "ToastMessageDialog", presentData,
            new Action<APopupDialog>( (popupDlg) => 
            { 
                Debug.Log("ToastMessage Dialog has been closed.");
            } ) ); 
    }
    void OnSignInFailed(string errorMessage)
    {
        Debug.Log($"<color=red>[TitleScreen] SignInFailed : {errorMessage}</color>");
        
        isWaitingForSignIn = false;
    }
    void OnSignOut()
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
    
    /*
    async Task TryBackgroundLogin(int interval)
    {
        isBacgroundLoginWorking = true;

        while(!titleScreen.AuthService.IsSignedIn())
        {
            if(Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("[BackgroundLogin] : No internet connection.");
                await Task.Delay(5000);
                continue;
            }

            Debug.Log("[BackgroundLogin] : Try Sign In....");
            await Task.Delay(interval * 1000);

            try
            {
                await titleScreen.AuthService.SignInAsync();
            }
            catch(Exception ex) 
            {
                Debug.Log($"[BackgroundLogin] : Login failed... {ex.Message}");
            }
        }

        isBacgroundLoginWorking = false;
    }*/

    async Task TestAsync()
    {
        string testData = "{\r\n    \"data\": [\r\n        {\r\n            \"key\": \"GamePlayPlayerModel_ZoneGroupStatusInfo\",\r\n            \"value\": \"{\\\"zones\\\":[{\\\"zoneId\\\":100,\\\"planets\\\":[{\\\"planetId\\\":1,\\\"isUnlocked\\\":true,\\\"level\\\":[1,23,23,23,20],\\\"minedRscInfo\\\":[{\\\"resourceId\\\":\\\"iceshard\\\",\\\"countF3\\\":\\\"634\\\"},{\\\"resourceId\\\":\\\"acorn\\\",\\\"countF3\\\":\\\"976\\\"},{\\\"resourceId\\\":\\\"blueberries\\\",\\\"countF3\\\":\\\"976\\\"}],\\\"deliveryInfos\\\":[{\\\"amountx1000\\\":\\\"1464\\\",\\\"posRate\\\":0.0,\\\"isHeadingToPlanet\\\":false},{\\\"amountx1000\\\":\\\"1464\\\",\\\"posRate\\\":0.0,\\\"isHeadingToPlanet\\\":false}],\\\"battleInfo\\\":{\\\"eventStartedTicke\\\":0,\\\"damageX1000\\\":\\\"\\\",\\\"isCleared\\\":false},\\\"isBoosterUnlocked\\\":true,\\\"boostState\\\":0,\\\"boostingDuration\\\":60.0,\\\"boostRemainTimeInSec\\\":60.0,\\\"boostCoolTimeDuration\\\":10.0,\\\"boosterRate\\\":5.0},{\\\"planetId\\\":2,\\\"isUnlocked\\\":true,\\\"level\\\":[1,22,11,9,8],\\\"minedRscInfo\\\":[{\\\"resourceId\\\":\\\"iceshard\\\",\\\"countF3\\\":\\\"432\\\"},{\\\"resourceId\\\":\\\"pileofstones\\\",\\\"countF3\\\":\\\"345\\\"},{\\\"resourceId\\\":\\\"blueberries\\\",\\\"countF3\\\":\\\"432\\\"},{\\\"resourceId\\\":\\\"cherry\\\",\\\"countF3\\\":\\\"432\\\"}],\\\"deliveryInfos\\\":[{\\\"amountx1000\\\":\\\"864\\\",\\\"posRate\\\":0.0,\\\"isHeadingToPlanet\\\":false},{\\\"amountx1000\\\":\\\"864\\\",\\\"posRate\\\":0.0,\\\"isHeadingToPlanet\\\":false}],\\\"battleInfo\\\":{\\\"eventStartedTicke\\\":0,\\\"damageX1000\\\":\\\"\\\",\\\"isCleared\\\":false},\\\"isBoosterUnlocked\\\":true,\\\"boostState\\\":0,\\\"boostingDuration\\\":60.0,\\\"boostRemainTimeInSec\\\":60.0,\\\"boostCoolTimeDuration\\\":10.0,\\\"boosterRate\\\":5.0},{\\\"planetId\\\":3,\\\"isUnlocked\\\":true,\\\"level\\\":[1,1,1,1,1],\\\"minedRscInfo\\\":[{\\\"resourceId\\\":\\\"pileofstones\\\",\\\"countF3\\\":\\\"1355\\\"},{\\\"resourceId\\\":\\\"cherry\\\",\\\"countF3\\\":\\\"984269\\\"}],\\\"deliveryInfos\\\":[{\\\"amountx1000\\\":\\\"2500\\\",\\\"posRate\\\":0.0,\\\"isHeadingToPlanet\\\":false}],\\\"battleInfo\\\":{\\\"eventStartedTicke\\\":0,\\\"damageX1000\\\":\\\"\\\",\\\"isCleared\\\":false},\\\"isBoosterUnlocked\\\":true,\\\"boostState\\\":0,\\\"boostingDuration\\\":60.0,\\\"boostRemainTimeInSec\\\":60.0,\\\"boostCoolTimeDuration\\\":10.0,\\\"boosterRate\\\":5.0}]}]}\"\r\n        },\r\n        {\r\n            \"key\": \"ResourcePlayerModel_ResourceData\",\r\n            \"value\": \"{\\\"resourceCollectInfos\\\":[{\\\"RscId\\\":\\\"iceshard\\\",\\\"CountX1000Str\\\":\\\"1118\\\",\\\"AutoSell\\\":true},{\\\"RscId\\\":\\\"pileofstones\\\",\\\"CountX1000Str\\\":\\\"999\\\",\\\"AutoSell\\\":true},{\\\"RscId\\\":\\\"droplet\\\",\\\"CountX1000Str\\\":\\\"19000\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"spring\\\",\\\"CountX1000Str\\\":\\\"14000\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"beer\\\",\\\"CountX1000Str\\\":\\\"28000\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"bloodwine\\\",\\\"CountX1000Str\\\":\\\"104000\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"coal\\\",\\\"CountX1000Str\\\":\\\"694\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"copper\\\",\\\"CountX1000Str\\\":\\\"1142\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"eternalpyramid\\\",\\\"CountX1000Str\\\":\\\"1000\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"iron\\\",\\\"CountX1000Str\\\":\\\"0\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"acorn\\\",\\\"CountX1000Str\\\":\\\"3680008\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"blueberries\\\",\\\"CountX1000Str\\\":\\\"8703584\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"cherry\\\",\\\"CountX1000Str\\\":\\\"7007\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"bluejuice\\\",\\\"CountX1000Str\\\":\\\"29000\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"greenjuice\\\",\\\"CountX1000Str\\\":\\\"720000\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"blackjuice\\\",\\\"CountX1000Str\\\":\\\"0\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"creamsoup\\\",\\\"CountX1000Str\\\":\\\"245000\\\",\\\"AutoSell\\\":false},{\\\"RscId\\\":\\\"eggtoast\\\",\\\"CountX1000Str\\\":\\\"12000\\\",\\\"AutoSell\\\":false}]}\"\r\n        },\r\n        {\r\n            \"key\": \"CraftPlayerModel_CraftCompData\",\r\n            \"value\": \"{\\\"isFeatureOpened\\\":true,\\\"craftingSlots\\\":[{\\\"RecipeId\\\":\\\"Comp_1\\\",\\\"ProgressedTime\\\":4},{\\\"RecipeId\\\":\\\"Comp_2\\\",\\\"ProgressedTime\\\":-1}],\\\"purchasedRecipeIndex\\\":6}\"\r\n        },\r\n        {\r\n            \"key\": \"CraftPlayerModel_CraftItemData\",\r\n            \"value\": \"{\\\"isFeatureOpened\\\":true,\\\"craftingSlots\\\":[{\\\"RecipeId\\\":\\\"Item_0\\\",\\\"ProgressedTime\\\":-1},{\\\"RecipeId\\\":\\\"Item_1\\\",\\\"ProgressedTime\\\":-1}],\\\"purchasedRecipeIndex\\\":1}\"\r\n        },\r\n        {\r\n            \"key\": \"SkillTreePlayerModel_SkillTreeData\",\r\n            \"value\": \"{\\\"categoryProcInfo\\\":[{\\\"categoryId\\\":\\\"mining\\\",\\\"learningSkillIdList\\\":[\\\"mining_1_zonediscovery2\\\"]},{\\\"categoryId\\\":\\\"goal\\\",\\\"learningSkillIdList\\\":[\\\"goalsection_0_reset\\\"]},{\\\"categoryId\\\":\\\"compcraft\\\",\\\"learningSkillIdList\\\":[\\\"compcraft_1_swiftquenching\\\"]},{\\\"categoryId\\\":\\\"itemcraft\\\",\\\"learningSkillIdList\\\":[\\\"itemcraft_1_swiftquenching\\\"]}]}\"\r\n        },\r\n        {\r\n            \"key\": \"IdleMinerPlayerModel_MoneyData\",\r\n            \"value\": \"{\\\"money\\\":[{\\\"amount\\\":\\\"500\\\",\\\"type\\\":0}]}\"\r\n        },\r\n        {\r\n            \"key\": \"IdleMinerPlayerModel_TimeStamp\",\r\n            \"value\": \"639026305637835539\"\r\n        },\r\n        {\r\n            \"key\": \"IdleMinerPlayerModel_OpenedTabBtns\",\r\n            \"value\": \"planet:resource:craft:skillTree\"\r\n        }\r\n    ]\r\n}";
        await titleScreen.CloudService?.SaveUserData("AAA", testData);// "login successed.");

        //string data = await titleScreen.CloudService.LoadUserData("AAA");
        //Debug.Log("[CloudData] Fetch result : " + data);
    }

}