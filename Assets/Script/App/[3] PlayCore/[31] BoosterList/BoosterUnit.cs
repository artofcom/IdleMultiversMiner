using App.GamePlay.IdleMiner;
using UnityEngine;

public class BoosterUnit : IGCore.MVCS.AUnit
{
    public override void Init(IGCore.MVCS.AContext ctx)
    {
        base.Init(ctx);

        controller = new BoosterController(view, new BoosterModel(context, null), context);
    }

    public override void Attach()
    {
        base.Attach();
    }

}
