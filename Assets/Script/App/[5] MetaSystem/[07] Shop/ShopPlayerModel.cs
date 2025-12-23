using App.GamePlay.IdleMiner.Common.PlayerModel;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopPlayerModel : GatewayWritablePlayerModel
{
    public ShopPlayerModel(AContext ctx, IDataGatewayService gatewayService) : base(ctx, gatewayService) { }


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
