using IGCore.MVCS;
using UnityEngine;

public class SettingModel : AModel
{
    public SettingModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData)  { }

    public override void Init(object data = null) { }   
}
