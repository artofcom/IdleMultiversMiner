using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScreenModel : IGCore.MVCS.AModel
{
    public LobbyScreenModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

    public override void Init() {}
}
