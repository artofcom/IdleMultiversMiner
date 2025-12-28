using UnityEngine;
using App.GamePlay.IdleMiner.SkillTree;
using IGCore.MVCS;

public class SkillTreeUnitLostDesert : SkillTreeUnit
{
    public override void Init(AContext ctx)
    {
        this.context = ctx;

        playerModel =  new SkillTreePlayerModel(context, (ctx as IdleMinerContext).GameCoreGatewayService);
        model = new SkillTreeModelLostDesert(context, playerModel);
        controller = new SkillTreeController(this, view, model, context);

        playerModel.Init();
        model.Init();
        controller.Init();  
    }

}
