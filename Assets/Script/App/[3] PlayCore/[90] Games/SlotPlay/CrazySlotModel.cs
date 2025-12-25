using UnityEngine;

public class CrazySlotModel : IGCore.MVCS.AModel
{
    public CrazySlotModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

    public override void Init(object data = null) {}
}
