using IGCore.MVCS;
using Unity.Services.Analytics;
using UnityEngine;

public class DailyTaskUnit : AUnit
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Init(AContext context)
    {
        base.Init(context);

        var playerModel = new DailyTaskPlayerModel(context, (context as IdleMinerContext).MetaDataGatewayService);
        model = new DailyTaskModel(context, playerModel);
        controller = new DailyTaskController(view, model, context);

        
        playerModel.Init();
        model.Init();
        controller.Init();
    }
}
