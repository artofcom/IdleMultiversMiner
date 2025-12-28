using IGCore.MVCS;
using IGCore.PlatformService.Cloud;
using UnityEngine;

public class LoginDialogUnit : AUnit
{
    [ImplementsInterface(typeof(IAuthService))]
    [SerializeField] MonoBehaviour authService;

    public IAuthService AuthService => authService as IAuthService;

    public override void Init(AContext context)
    {
        base.Init(context);

        model = new LoginDialogModel(context, null);
        controller = new LoginDialogController(this, view, model, context);

        model.Init();
        controller.Init();
    }
}
