using IGCore.MVCS;

public class ShopUnit : AUnit
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Init(AContext context)
    {
        base.Init(context);

        model = new ShopModel(context, null);
        controller = new ShopController(this, view, model, context);

        
        model.Init();
        controller.Init();
    }

    public override void Dispose() 
    { 
        base.Dispose();
    }
}
