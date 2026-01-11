using IGCore.MVCS;
using UnityEngine;

namespace App.GamePlay.IdleMiner.Craft
{
    public class CraftUnit : AUnit
    {
        APlayerModel playerModel;

        public override void Init(AContext ctx)
        {
            //ctx.UpdateData("comp_craft_data_path", "Bundles/G033_IdleMiner/EditorData/Craft_comp");
            //ctx.UpdateData("item_craft_data_path", "Bundles/G033_IdleMiner/EditorData/Craft_Item");

            base.Init(ctx);

            playerModel = new CraftPlayerModel(context, (ctx as IdleMinerContext).GameGatewayServiceList);
            model = new CraftModel(context, playerModel);
            controller = new CraftController(this, view, model, context);

            playerModel.Init();
            model.Init();
            controller.Init();  
        }

        public override void Dispose()
        {
            base.Dispose();
            playerModel?.Dispose();
        }
    }
}