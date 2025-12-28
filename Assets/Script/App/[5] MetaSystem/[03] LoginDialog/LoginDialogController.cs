using IGCore.MVCS;

public class LoginDialogModel : AModel
{
    public LoginDialogModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData)  { }
    public override void Init(object data = null) { }   
}


public class LoginDialogController : AController
{
    LoginDialogView View;
    LoginDialogUnit Unit;

    public LoginDialogController(AUnit unit, AView view, AModel model, AContext context) : base(unit, view, model, context)
    { }

    public override void Init() 
    { 
        Unit = (unit as LoginDialogUnit);
        View = (view as LoginDialogView);
    }
    public override void Resume(int awayTimeInSec) { }
    public override void Pump() { }
    public override void WriteData() { }

    protected override void OnViewEnable()  
    { 
        View.EventOnLoginClicked += OnBtnLoginClicked;
        View.EventOnAnonymLoginClicked += OnBtnAnonymLoginClicked;

        Unit.AuthService.EventOnSignedIn += OnSignedIn;
        Unit.AuthService.EventOnSignInFailed += OnSignInFailed;

        //RefreshView();
    }
    protected override void OnViewDisable() 
    {
        View.EventOnLoginClicked -= OnBtnLoginClicked;
        View.EventOnAnonymLoginClicked -= OnBtnAnonymLoginClicked;

        Unit.AuthService.EventOnSignedIn -= OnSignedIn;
        Unit.AuthService.EventOnSignInFailed -= OnSignInFailed;
    }

    void OnBtnLoginClicked() 
    {
        Unit.AuthService.PlayerSignInAsync();
    }
    void OnBtnAnonymLoginClicked() 
    { 
        Unit.AuthService.SignInAsync();
    }

    void OnSignedIn(string playerId)
    {
        Unit.Detach();
    }
    void OnSignInFailed(string errMsg)
    {
        Unit.Detach();
    }
}
