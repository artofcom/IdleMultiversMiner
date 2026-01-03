using UnityEngine;
using App.GamePlay.IdleMiner.SkillTree;
using IGCore.MVCS;

public class SkillTreeUnitLava : SkillTreeUnit
{
    public override void Init(AContext ctx)
    {
        this.context = ctx;

        playerModel =  new SkillTreePlayerModel(context, (ctx as IdleMinerContext).GameGatewayServiceList);
        model = new SkillTreeModelLava(context, playerModel);
        controller = new SkillTreeController(this, view, model, context);

        playerModel.Init();
        model.Init();
        controller.Init();  
    }

}
