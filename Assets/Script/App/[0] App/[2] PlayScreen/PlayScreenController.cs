using App.GamePlay.Common;
using App.GamePlay.IdleMiner;
using IGCore.MVCS;
using UnityEngine;

public class PlayScreenController : IGCore.MVCS.AController
{
    const float WAIT_TIME_SEC = 1.5f;

    //public Action<string> OnClose;    // Next Module Key.

    IdleMinerUnit IdleMinerUnitCache;
    IdleMinerContext IMContext => (IdleMinerContext)context;

    public PlayScreenController(AUnit unit, AView view, AModel model, AContext ctx)
        : base(unit, view, model, ctx)
    { 
        
    }

    public override void Init() {}

    protected override void OnViewEnable()
    {
        Debug.Log("============================= GamePlay Enter ");

        // view.StartCoroutine( coOnViewEnabled() );
        context.AddRequestDelegate("PlayScreen", "SwitchUnit", switchUnit);
    }

    protected override void OnViewDisable() 
    {
        context.RemoveRequestDelegate("PlayScreen", "SwitchUnit");
    }

    public override void Resume(int awayTimeInSec) { }
    
    public override void Pump() { }
    
    public override void WriteData() { }






    void EventOnBtnOptionClicked() 
    {
        (unit as PlayScreen).SwitchUnit("TitleScreen");
    }

    object switchUnit(params object[] data) // int amount, bool isOffset)
    {
        if(data.Length < 1)
            return null;

        string nextUnitId = (string)data[0];
        (unit as PlayScreen).SwitchUnit(nextUnitId);
        return null;
    }

    /*
    PlayScreenView _view;
    GameContext _context;

    AView _gamePlayView;
    IController _gamePlayController;

    //  Events ----------------------------------------
    //
    EventsGroup Events = new EventsGroup();

    public PlayScreenController(PlayScreenView view, GameContext context)
    {
        _view = view;
        _context = context;

        Events.RegisterEvent("PlayScreenView_OnEnable", PlayScreenView_OnEnable);
        Events.RegisterEvent("PlayScreenView_OnDisable", PlayScreenView_OnDisable);
    }

    void PlayScreenView_OnEnable(object data)
    {
        _view.Init(_context.GamePrefab, _context);

        // _context.GameType ???
        //_gamePlayController = new WordTileSearchController(_view.GamePlayView, _context);
        // else 
        _gamePlayController = new IdleMinerController(_view.GamePlayView, _context);


        //Init(_view.ReelComponent);
    }

    void PlayScreenView_OnDisable(object data)
    {
    }

    //void Init(AReelComponent reelComponent)
    //{
        // read data.
        //mGame = new BaseGame();
        //mGame.Init(_context.GameControlData);

        // based on the data.
        //ReelComponent = reelComponent;
        //FeatureComponent = new EmptyFeatureComponent();
        //EvaluatorComponent = new DefaultEvaluatorComponent();
        //
    //}*/
}
