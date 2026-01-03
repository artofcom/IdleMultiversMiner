
using System.Collections.Generic;
using System;
using IGCore.MVCS;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class MiningStatPlayerModel : MultiGatewayWritablePlayerModel
    {
        public MiningStatPlayerModel(AContext ctx, List<IDataGatewayService> gatewayService) : base(ctx, gatewayService) 
        {
            // InitPlayerData();
        }

        public void WriteData() { }

        public override void Init()
        {
            base.Init();

            IsInitialized = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            IsInitialized = false;
        }

        public override List<Tuple<string, string>> GetSaveDataWithKeys()
        {
            return null;
        }
    }
}
