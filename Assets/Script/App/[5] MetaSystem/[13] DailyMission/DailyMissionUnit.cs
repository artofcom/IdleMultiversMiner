using IGCore.MVCS;
using UnityEngine;

public class DailyMissionUnit : AUnit
{
    [SerializeField] DailyMissionConfig dailyMissionConfig;
    [SerializeField] SpriteConfig commonSpriteConfig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Init(AContext context)
    {
        base.Init(context);

        var playerModel = new DailyMissionPlayerModel(context, (context as IdleMinerContext).MetaDataGatewayService);
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
}
