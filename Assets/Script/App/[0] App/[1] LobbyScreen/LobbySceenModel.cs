using IGCore.MVCS;

public class LobbyScreenModel : NoPlayerDataModel
{
    public LobbyScreenModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData) { }

    public override void Init(object data = null) {}
}
