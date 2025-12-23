using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using App.GamePlay.IdleMiner;
using System;
using UnityEngine.Assertions;
using Core.Util;

public class PlayScreenController : IGCore.MVCS.AController
{
    const float WAIT_TIME_SEC = 1.5f;

    //public Action<string> OnClose;    // Next Module Key.

    IdleMinerUnit IdleMinerUnitCache;
    IdleMinerContext IMContext => (IdleMinerContext)context;

    public PlayScreenController(IGCore.MVCS.AView view, IGCore.MVCS.AModel model, IGCore.MVCS.AContext ctx)
        : base(view, model, ctx)
    { 
        
    }

    public override void Init() {}

    protected override void OnViewEnable()
    {
        Debug.Log("============================= GamePlay Enter ");

        // view.StartCoroutine( coOnViewEnabled() );
    }

    /*IEnumerator coOnViewEnabled()
    {
        while(true)
        {
            IdleMinerUnitCache = (IdleMinerUnit) context.GetData("gamePlayModule");
            if(IdleMinerUnitCache != null)
                break;

            yield return null;
        }

       // ((IdleMinerView)IdleMinerUnitCache.View).EventOnBtnOptionClicked += EventOnBtnOptionClicked; 

        DelayedAction.TriggerActionWithDelay(IMContext.CoRunner, WAIT_TIME_SEC, () =>
        {
            // OnEventClose?.Invoke("TitleScreen");
        });
    }*/
    
    protected override void OnViewDisable() { }

    public override void Resume(int awayTimeInSec) { }
    
    public override void Pump() { }
    
    public override void WriteData() { }






    void EventOnBtnOptionClicked() 
    {
        OnEventClose?.Invoke("TitleScreen");
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
