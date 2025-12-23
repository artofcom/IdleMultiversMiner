using IGCore.MVCS;
using UnityEngine;

public class GameCardsController : AController
{
    public GameCardsController(AView view, AModel model, AContext context) : base(view, model, context)
    { }

    public override void Init() { }
    public override void Resume(int awayTimeInSec) { }
    public override void Pump() { }
    public override void WriteData() { }

    protected override void OnViewEnable()  { }
    protected override void OnViewDisable() { }
}
