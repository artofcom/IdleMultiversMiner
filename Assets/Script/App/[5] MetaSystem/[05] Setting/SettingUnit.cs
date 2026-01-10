using IGCore.MVCS;
using IGCore.PlatformService.Cloud;
using UnityEngine;

public class SettingUnit : AUnit
{
    APlayerModel playerModel;

    [ImplementsInterface(typeof(IAuthService))]
    [SerializeField] MonoBehaviour authService;

    public IAuthService AuthService => authService as IAuthService;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Init(AContext context)
    {
        Dispose();

        base.Init(context);

        playerModel = new SettingPlayerModel(context, (context as IdleMinerContext).MetaGatewayServiceList);
        model = new SettingModel(context, playerModel);
        controller = new SettingController(this, view, model, context);
        
        playerModel.Init();
        model.Init();
        controller.Init();
    }

    public override void Attach()
    {
        base.Attach();
    }

    public override void Dispose() 
    { 
        base.Dispose();
        playerModel?.Dispose();
    }
}
