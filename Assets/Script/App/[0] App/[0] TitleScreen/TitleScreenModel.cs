using IGCore.MVCS;

public class TitleScreenModel : NoPlayerDataModel
{
    public TitleScreenModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData) { }

    public override void Init(object data = null) {}
}

