using App.GamePlay.IdleMiner;
using UnityEngine;

public class MeteorUnit :  IGCore.MVCS.AUnit
{
    public override void Init(IGCore.MVCS.AContext ctx)
    {
        base.Init(ctx);

        controller = new MeteorController(this, view, new MeteorModel(context, null), context);
    }

    public override void Attach()
    {
        base.Attach();
    }
}
