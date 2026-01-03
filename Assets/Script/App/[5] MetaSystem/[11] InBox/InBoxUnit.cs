using IGCore.MVCS;

public class InBoxUnit : AUnit
{
    APlayerModel playerModel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Init(AContext context)
    {
        base.Init(context);

        playerModel = new InBoxPlayerModel(context, (context as IdleMinerContext).MetaGatewayServiceList);
        model = new InBoxModel(context, playerModel);
        controller = new InBoxController(this, view, model, context);

        
        playerModel.Init();
        model.Init();
        controller.Init();
    }

    public override void Dispose() 
    { 
        base.Dispose();
        playerModel.Dispose();
    }
}
