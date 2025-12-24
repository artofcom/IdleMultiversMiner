using IGCore.MVCS;

public class DailyMissionController : AController
{
    public DailyMissionController(AView view, AModel model, AContext context) : base(view, model, context)
    { }

    public override void Init() { }
    public override void Resume(int awayTimeInSec) { }
    public override void Pump() { }
    public override void WriteData() { }

    protected override void OnViewEnable()  
    { 
        RefreshView();
    }
    protected override void OnViewDisable() { }



    void RefreshView()
    {
        view.Refresh(new DailyMissionView.Presentor("Test00"));
    }
}
