using IGCore.MVCS;
using UnityEngine;

public class ShopController : AController
{
    public ShopController(AUnit unit, AView view, AModel model, AContext context) : base(unit, view, model, context)
    { }

    public override void Init() {   base.Init(); }
    public override void Resume(int awayTimeInSec) { }
    public override void Pump() { }
    public override void WriteData() { }

    protected override void OnViewEnable()  { }
    protected override void OnViewDisable() { }
}
