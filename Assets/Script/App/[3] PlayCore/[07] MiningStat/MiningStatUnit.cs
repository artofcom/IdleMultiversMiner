using UnityEngine;
using IGCore.MVCS;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class MiningStatUnit : AUnit
    {
        APlayerModel playerModel;

        public override void Init(IGCore.MVCS.AContext ctx)
        {
            base.Init(ctx);

            ctx.AddRequestDelegate("MiningStat", "Attach", attach);

            playerModel = new MiningStatPlayerModel(context, (ctx as IdleMinerContext).GameGatewayServiceList);
            model = new MiningStatModel(context, playerModel);
            controller = new MiningStatController(this, view, model, context);

            playerModel.Init();
            model.Init();
            controller.Init();  
        }

        object attach(params object[] data)
        {
            if(data.Length < 1) return null;

            int zoneId = (int)data[0];
            int planetId = (int)data[1];
            context.UpdateData("CurrentZoneId", zoneId);
            context.UpdateData("CurrentPlanetId", planetId);

            this.Attach();
            return null;
        }

        public override void Dispose()
        {
            base.Dispose();
            playerModel?.Dispose();
        }
    }
}