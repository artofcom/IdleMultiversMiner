using UnityEngine;
using IGCore.MVCS;
using App.GamePlay.IdleMiner.GamePlay;

namespace App.GamePlay.IdleMiner.Games.IdleFinishing
{
    public class IFGamePlayUnit : AUnit
    {
        public override void Init(IGCore.MVCS.AContext ctx)
        {
            base.Init(ctx);

            context.UpdateData("planet_data_path", "Bundles/G033_IdleMiner/Data/PlanetData");
            context.UpdateData("bossplanet_data_path", "Bundles/G033_IdleMiner/Data/PlanetBossData");

            var playerModel = new GamePlayPlayerModel(context, (context as IdleMinerContext).GameGatewayServiceList);
            var model = new GamePlayModel(context, playerModel);
            controller = new IFGamePlayController(this, view, model, context);

            playerModel.Init();
            model.Init();
            controller.Init();  
        }
    }
}
