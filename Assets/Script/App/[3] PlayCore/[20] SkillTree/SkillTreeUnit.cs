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

            playerModel =  new SkillTreePlayerModel(context, (ctx as IdleMinerContext).GameCoreGatewayService);
            model = new SkillTreeModel(context, playerModel);
            controller = new SkillTreeController(view, model, context);

            playerModel.Init();
            model.Init();
            controller.Init();  
        }

        public override void Dispose()
        {
            base.Dispose();
            playerModel.Dispose();
        }
    }
}