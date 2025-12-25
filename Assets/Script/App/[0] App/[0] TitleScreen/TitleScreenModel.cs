using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenModel : IGCore.MVCS.AModel
{
    public TitleScreenModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) { }

    public override void Init(object data = null) {}
}

