using App.GamePlay.IdleMiner.Common.PlayerModel;
using IGCore.MVCS;
using System;
using System.Collections.Generic;

public class DailyMissionPlayerModel : GatewayWritablePlayerModel
{
    public DailyMissionPlayerModel(AContext ctx, IDataGatewayService gatewayService) : base(ctx, gatewayService) { }


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
        // Assert.IsNotNull(unlockedZoneGroup);
        List<Tuple<string, string>> listDataSet = new List<Tuple<string, string>>();
        //listDataSet.Add(new Tuple<string, string>(DateKey_ZoneInfo, JsonUtility.ToJson(unlockedZoneGroup)));
        return listDataSet;
    }
}
