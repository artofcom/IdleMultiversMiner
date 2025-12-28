using App.GamePlay.IdleMiner;
using IGCore.MVCS;
using UnityEngine;


namespace App.GamePlay.IdleMiner.GamePlay
{
    public class ManagerUnit : IGCore.MVCS.AUnit
    {
        public override void Init(IGCore.MVCS.AContext ctx)
        {
            base.Init(ctx);

            model = new ManagerModel(context, null);
            controller = new ManagerController(this, view, model, context);

            model.Init();
            controller.Init();  
        }

        public override void Attach()
        {
            base.Attach();
        }
    }
}