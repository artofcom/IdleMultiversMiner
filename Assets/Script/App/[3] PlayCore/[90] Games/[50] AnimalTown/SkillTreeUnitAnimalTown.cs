using UnityEngine;
using App.GamePlay.IdleMiner.SkillTree;
using IGCore.MVCS;

public class SkillTreeUnitAnimalTown : SkillTreeUnit
{
    public override void Init(AContext ctx)
    {
        base.Init(ctx);
        this.context = ctx;

        playerModel =  new SkillTreePlayerModel(context, (ctx as IdleMinerContext).GameGatewayServiceList);
        model = new SkillTreeModelAnimalTown(context, playerModel);
        controller = new SkillTreeController(this, view, model, context);

        playerModel.Init();
        model.Init();
        controller.Init();
    }

}
