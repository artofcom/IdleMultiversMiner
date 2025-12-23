using IGCore.MVCS;
using UnityEngine;

public class AppModel : AModel
{
    public AppModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData)  { }

    public override void Init() { }       
}
