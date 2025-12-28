using IGCore.MVCS;
using Unity.VisualScripting;
using UnityEngine;

public class SettingUnit : AUnit
{
    APlayerModel playerModel;

    public bool ShouldSignOut { get ;set; } = false;
    public bool ShouldDeleteAccount { get ;set; } = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Init(AContext context)
    {
        base.Init(context);

        playerModel = new SettingPlayerModel(context, (context as IdleMinerContext).MetaDataGatewayService);
        model = new SettingModel(context, playerModel);
        controller = new SettingController(this, view, model, context);

        
        playerModel.Init();
        model.Init();
        controller.Init();
    }

    public override void Attach()
    {
        base.Attach();

        ShouldSignOut = false;
        ShouldDeleteAccount = false;
    }

    public override void Dispose() 
    { 
        base.Dispose();
        playerModel.Dispose();
    }
}
