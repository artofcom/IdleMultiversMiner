using Core.Util;
using IGCore.MVCS;
using System.Collections;
using UnityEngine;

public class TitleScreenController : AController
{
    const float WAIT_TIME_SEC = 1.5f;

    IdleMinerContext IMContext => (IdleMinerContext)context;

    public TitleScreenController(AUnit unit, AView view, AModel model, AContext ctx)
        : base(unit, view, model, ctx)
    {}

    public override void Init()
    {}

    protected override void OnViewEnable()
    {
        Debug.Log("============================= Title Enter ");

        //AsyncInitService();

        DelayedAction.TriggerActionWithDelay(IMContext.CoRunner, WAIT_TIME_SEC, () =>
        {
            (unit as TitleScreen).SwitchUnit("LobbyScreen");
        });
    }
    
    protected override void OnViewDisable() { }

    public override void Resume(int awayTimeInSec) { }
    
    public override void Pump() { }
    
    public override void WriteData() { }
    
}