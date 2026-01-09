using IGCore.MVCS;
using UnityEngine;

public class DailyMissionUnit : AUnit
{
    [SerializeField] DailyMissionConfig dailyMissionConfig;
    [SerializeField] SpriteConfig commonSpriteConfig;

    DailyMissionPlayerModel playerModel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Init(AContext context)
    {
        Dispose();

        base.Init(context);

        playerModel = new DailyMissionPlayerModel(context, (context as IdleMinerContext).MetaGatewayServiceList);
        model = new DailyMissionModel(context, playerModel);
        controller = new DailyMissionController(this, view, model, context);

        (controller as DailyMissionController).CommonSpriteConfigCache = commonSpriteConfig;
        
        playerModel.Init();
        model.Init(dailyMissionConfig);
        controller.Init();
    }

    public void OnBtnCloseClicked()
    {
        this.Detach();
    }    

    public override void Dispose()
    {
        playerModel?.Dispose();
        base.Dispose();
    }
}
