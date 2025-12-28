using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IGCore.MVCS;
using IGCore.SubSystem.Analytics;
using IGCore.PlatformService.Cloud;


public class TitleScreen : AUnit
{
    [ImplementsInterface(typeof(IAnalyticsService))]
    [SerializeField] ScriptableObject analyticsService;

    [ImplementsInterface(typeof(IAuthService))]
    [SerializeField] MonoBehaviour authService;

    [ImplementsInterface(typeof(IUnitSwitcher))]
    [SerializeField] MonoBehaviour unitSwitcher;

    IAuthService AuthService => authService as IAuthService;
    IUnitSwitcher UnitSwitcher => unitSwitcher as IUnitSwitcher;

    // DictorMain.Start() -> AUnitSwitcher.Init() -> TitleScreen.Init()
    public override void Init(IGCore.MVCS.AContext ctx)
    {
        base.Init(ctx);
        context.AddData("PlayerId", string.Empty);
        context.AddData("IsAccountLinked", false);

        var playerModel = new TitleScreenPlayerModel(ctx);
        model = new TitleScreenModel(context, playerModel);
        controller = new TitleScreenController(this, view, model, context);

        playerModel.Init();
        model.Init();
        controller.Init();
        (analyticsService as IAnalyticsService).Init();

        AuthService.EventOnSignedIn += OnSignedIn;
        AuthService.EventOnSignInFailed += OnSignInFailed;
        AuthService.EventOnSignOut += OnSignOut;
        AuthService.EventOnSessionExpired += OnSessionExpired;

        view.OnViewEnable += OnViewEnable;
    }

    public override void Dispose()
    {
        base.Dispose();

        AuthService.EventOnSignedIn -= OnSignedIn;
        AuthService.EventOnSignInFailed -= OnSignInFailed;
        AuthService.EventOnSignOut -= OnSignOut;
        AuthService.EventOnSessionExpired -= OnSessionExpired;

        view.OnViewEnable -= OnViewEnable;
    }

    void OnViewEnable()
    {
        AuthService.SignIn();
    }

    void OnSignedIn(string playerId)
    {
        context.AddData("PlayerId", playerId);
        context.AddData("IsAccountLinked", AuthService.IsAccountLinkedWithIdentity("unity"));
    }
    void OnSignInFailed(string msg)
    {
    }
    void OnSignOut() { }
    void OnSessionExpired() { }

    public void SwitchUnit(string nextUnit)
    {
        UnitSwitcher.SwitchUnit(nextUnit);
    }
}