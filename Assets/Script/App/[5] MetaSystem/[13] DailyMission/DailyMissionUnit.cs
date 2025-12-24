using IGCore.MVCS;
using Unity.Services.Analytics;
using UnityEngine;

public class DailyMissionUnit : AUnit
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Init(AContext context)
    {
        base.Init(context);

        var playerModel = new DailyMissionPlayerModel(context, (context as IdleMinerContext).MetaDataGatewayService);
        model = new DailyMissionModel(context, playerModel);
        controller = new DailyMissionController(view, model, context);

        
        playerModel.Init();
        model.Init();
        controller.Init();
    }

    public void OnBtnCloseClicked()
    {
        this.Detach();
    }
}
