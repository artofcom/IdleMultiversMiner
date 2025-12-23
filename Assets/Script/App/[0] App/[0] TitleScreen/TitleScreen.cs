using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IGCore.MVCS;
using IGCore.SubSystem.Analytics;


public class TitleScreen : AUnit
{
    [ImplementsInterface(typeof(IAnalyticsService))]
    [SerializeField] ScriptableObject analyticsService;

    

    // DictorMain.Start() -> AUnitSwitcher.Init() -> TitleScreen.Init()
    public override void Init(IGCore.MVCS.AContext ctx)
    {
        base.Init(ctx);

        var playerModel = new TitleScreenPlayerModel(ctx);
        model = new TitleScreenModel(context, playerModel);
        controller = new TitleScreenController(view, model, context);

        
        playerModel.Init();
        model.Init();
        controller.Init();
        (analyticsService as IAnalyticsService).Init();
    }
}