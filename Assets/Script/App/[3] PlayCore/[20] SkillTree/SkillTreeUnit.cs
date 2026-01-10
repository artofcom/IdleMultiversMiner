using IGCore.MVCS;
using UnityEngine;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class SkillTreeUnit : IGCore.MVCS.AUnit
    {
        protected APlayerModel playerModel;

        public override void Init(IGCore.MVCS.AContext ctx)
        {
            base.Init(ctx);

            // Should Implement from child.
            //
            //
        }

        public override void Dispose()
        {
            base.Dispose();
            playerModel.Dispose();
        }
    }
}