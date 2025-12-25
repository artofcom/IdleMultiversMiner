using IGCore.MVCS;
using UnityEngine;

public class ShopModel : AModel
{
    public ShopModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData)  { }

    public override void Init(object data = null) { }   
}
