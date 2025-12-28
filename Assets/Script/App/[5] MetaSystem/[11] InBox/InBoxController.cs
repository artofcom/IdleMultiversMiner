using IGCore.MVCS;
using UnityEngine;

public class InBoxController : AController
{
    public InBoxController(AUnit unit, AView view, AModel model, AContext context) : base(unit, view, model, context)
    { }

    public override void Init() { }
    public override void Resume(int awayTimeInSec) { }
    public override void Pump() { }
    public override void WriteData() { }

    protected override void OnViewEnable()  { }
    protected override void OnViewDisable() { }
}
