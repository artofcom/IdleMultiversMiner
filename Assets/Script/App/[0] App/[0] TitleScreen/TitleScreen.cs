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

    [ImplementsInterface(typeof(ICloudService))]
    [SerializeField] MonoBehaviour cloudService;

    [ImplementsInterface(typeof(IUnitSwitcher))]
    [SerializeField] MonoBehaviour unitSwitcher;

    public IAuthService AuthService => authService as IAuthService;
    public ICloudService CloudService => cloudService as ICloudService;
    IUnitSwitcher UnitSwitcher => unitSwitcher as IUnitSwitcher;

    // DictorMain.Start() -> AUnitSwitcher.Init() -> TitleScreen.Init()
    public override void Init(AContext ctx)
    {
        base.Init(ctx);
        context.AddData("PlayerId", string.Empty);
        context.AddData("IsAccountLinked", false);


        model = new TitleScreenModel(context, null);
        controller = new TitleScreenController(this, view, model, context);

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
    {}

    void OnSignedIn(string playerId) {}
    void OnSignInFailed(string msg) {}
    void OnSignOut() { }
    void OnSessionExpired() { }

    public void SwitchUnit(string nextUnit)
    {
        UnitSwitcher.SwitchUnit(nextUnit);
    }
}