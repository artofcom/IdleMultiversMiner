using UnityEngine;

public class CrazySlotController : IGCore.MVCS.AController
{
    public CrazySlotController(IGCore.MVCS.AView view, IGCore.MVCS.AModel model, IGCore.MVCS.AContext ctx)
        : base(view, model, ctx)
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
