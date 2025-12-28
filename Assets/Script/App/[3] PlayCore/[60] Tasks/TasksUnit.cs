using App.GamePlay.IdleMiner;
using UnityEngine;

public class TasksUnit : IGCore.MVCS.AUnit
{
    public override void Init(IGCore.MVCS.AContext ctx)
    {
        base.Init(ctx);

        controller = new TasksController(this, view, new TaskModel(context, null), context);
    }

    public override void Attach()
    {
        UnityEngine.Assertions.Assert.IsNotNull(context);

        base.Attach();
    }

}
