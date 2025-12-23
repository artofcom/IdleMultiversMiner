using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using IGCore.MVCS;
using UnityEngine;

public class TitleScreenPlayerModel : WritablePlayerModel 
{   
    public TitleScreenPlayerModel(AContext ctx) : base(ctx)  { }

    public override void Init()
    {   
        IsInitialized = true;
    }

    public override void Dispose()
    {
        base.Dispose();

        IsInitialized= false;
    }
}
