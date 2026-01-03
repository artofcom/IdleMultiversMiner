using UnityEngine;
using IGCore.MVCS;
using App.GamePlay.IdleMiner.GamePlay;

namespace App.GamePlay.IdleMiner.Games.IdleHellBoy
{
    public class HBGamePlayUnit : AUnit
    {
        public override void Init(IGCore.MVCS.AContext ctx)
        {
            base.Init(ctx);

            context.UpdateData("planet_data_path", "Bundles/G033_IdleMiner/Data/PlanetData");
            context.UpdateData("bossplanet_data_path", "Bundles/G033_IdleMiner/Data/PlanetBossData");

            var playerModel = new GamePlayPlayerModel(context, (context as IdleMinerContext).GameGatewayServiceList );
            var model = new GamePlayModel(context, playerModel );
            controller = new HBGamePlayController(this, view, model, context);

            playerModel.Init();
            model.Init();
            controller.Init();  
        }

        public override void Attach()
        {
            base.Attach();
        }
    }
}
