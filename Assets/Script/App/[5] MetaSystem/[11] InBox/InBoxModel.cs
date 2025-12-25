using IGCore.MVCS;
using UnityEngine;

public class InBoxModel : AModel
{
    public InBoxModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData)  { }

    public override void Init(object data = null) { }   
}
