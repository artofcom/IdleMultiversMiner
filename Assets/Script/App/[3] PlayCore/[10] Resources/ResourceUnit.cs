using IGCore.MVCS;
using UnityEngine;


namespace App.GamePlay.IdleMiner.Resouces
{
    public class ResourceUnit : IGCore.MVCS.AUnit
    {
        APlayerModel playerModel;

        public override void Init(IGCore.MVCS.AContext ctx)
        {
            base.Init(ctx);

            playerModel = new ResourcePlayerModel(context, (context as IdleMinerContext).GameCoreGatewayService);
            model = new ResourceModel(context, playerModel);
            controller = new ResourceController(view, model, context);

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