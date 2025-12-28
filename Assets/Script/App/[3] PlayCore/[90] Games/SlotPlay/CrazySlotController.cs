using UnityEngine;
using IGCore.MVCS;

public class CrazySlotController : AController
{
    public CrazySlotController(AUnit unit, AView view, AModel model, AContext ctx)
        : base(unit, view, model, ctx)
    { }

    public override void Init() {}

    protected override void OnViewEnable()
    {
        Debug.Log("============================= CrazySlot Play Enter ");
    }

    protected override void OnViewDisable() { }

    public override void Resume(int awayTimeInSec) { }
    public override void Pump() { }
    public override void WriteData() { }
}
