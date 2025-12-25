using IGCore.MVCS;
using UnityEngine;

public class GameCardsModel : AModel
{
    public GameCardsModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData)  { }

    public override void Init(object data = null) { }   
}
