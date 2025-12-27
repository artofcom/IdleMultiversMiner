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

    IAuthService AuthService => authService as IAuthService;

    // DictorMain.Start() -> AUnitSwitcher.Init() -> TitleScreen.Init()
    public override void Init(IGCore.MVCS.AContext ctx)
    {
        base.Init(ctx);
        context.AddData("PlayerId", string.Empty);

        var playerModel = new TitleScreenPlayerModel(ctx);
        model = new TitleScreenModel(context, playerModel);
        controller = new TitleScreenController(view, model, context);

        playerModel.Init();
        model.Init();
        controller.Init();
        (analyticsService as IAnalyticsService).Init();

        AuthService.EventOnSignedIn += OnSignedIn;
        AuthService.EventOnSignInFailed += OnSignInFailed;
        AuthService.EventOnSignOut += OnSignOut;
        AuthService.EventOnSessionExpired += OnSessionExpired;
    }

    public override void Dispose()
    {
        base.Dispose();

        AuthService.EventOnSignedIn -= OnSignedIn;
        AuthService.EventOnSignInFailed -= OnSignInFailed;
        AuthService.EventOnSignOut -= OnSignOut;
        AuthService.EventOnSessionExpired -= OnSessionExpired;
    }

    void OnSignedIn(string playerId)
    {
        context.AddData("PlayerId", playerId);
    }
    void OnSignInFailed(string msg)
    {
    }
    void OnSignOut() { }
    void OnSessionExpired() { }
}