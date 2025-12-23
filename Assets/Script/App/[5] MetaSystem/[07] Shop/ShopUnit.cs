using IGCore.MVCS;
using Unity.Services.Analytics;
using UnityEngine;

public class ShopUnit : AUnit
{
    APlayerModel playerModel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Init(AContext context)
    {
        base.Init(context);

        playerModel = new ShopPlayerModel(context, (context as IdleMinerContext).MetaDataGatewayService);
        model = new ShopModel(context, playerModel);
        controller = new ShopController(view, model, context);

        
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
