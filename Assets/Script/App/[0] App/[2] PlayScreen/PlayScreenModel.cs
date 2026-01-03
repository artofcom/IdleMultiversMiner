using IGCore.MVCS;

public class PlayScreenModel : NoPlayerDataModel
{
    public PlayScreenModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData) { }

    public override void Init(object data = null) {}
}
