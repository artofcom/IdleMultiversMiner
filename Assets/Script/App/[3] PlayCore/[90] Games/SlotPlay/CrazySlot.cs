using UnityEngine;
using IGCore.MVCS;

namespace App.GamePlay.Demo
{
    public class CrazySlot : AUnit
    {
        public override void Init(IGCore.MVCS.AContext ctx)
        {
            base.Init(ctx);
        }

        public override void Attach()
        {
            controller = new CrazySlotController(this, view, new CrazySlotModel(context, null), context);

            base.Attach();
        }
    }
}
