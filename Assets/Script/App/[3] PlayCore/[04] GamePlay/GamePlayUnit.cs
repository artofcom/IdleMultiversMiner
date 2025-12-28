using UnityEngine;
using IGCore.MVCS;

namespace App.GamePlay.IdleMiner.GamePlay
{
    public class GamePlayUnit : AUnit
    {
        APlayerModel playerModel;

        public override void Init(IGCore.MVCS.AContext ctx)
        {
            base.Init(ctx);

            playerModel = new GamePlayPlayerModel(context, (context as IdleMinerContext).GameCoreGatewayService);
            model = new GamePlayModel(context, playerModel);
            controller = new GamePlayController(this, view, model, context);

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
