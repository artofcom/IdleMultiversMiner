using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayScreenModel : IGCore.MVCS.AModel
{
    public PlayScreenModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

    public override void Init() {}
}
