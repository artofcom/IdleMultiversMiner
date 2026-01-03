using IGCore.MVCS;

public class GameCardsUnit : AUnit
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Init(AContext context)
    {
        base.Init(context);

        var playerModel = new GameCardsPlayerModel(context, ((IdleMinerContext)context).MetaGatewayServiceList);
        model = new GameCardsModel(context, playerModel);
        controller = new GameCardsController(this, view, model, context);

        
        playerModel.Init();
        model.Init();
        controller.Init();
    }
}
