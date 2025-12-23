using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;

public class PlayScreenView : IGCore.MVCS.AView
{
    public class Presentor : APresentor
    {
        public Presentor(string loadingMsg)
        {
            this.loadingMsg = loadingMsg;
        }   

        public string loadingMsg { get; private set; } 
    }


    public override void Refresh(APresentor presentData)
    {
        Presentor presentor = presentData as Presentor;
        if(presentor == null)
            return;

        ///txtLoading.text = presentor.loadingMsg;
    }


    /*
    [SerializeField] Transform GamePlayRoot;
    [SerializeField] Camera UICamera;

    [SerializeField] GameObject GameMainPrefab;

    public AView GamePlayView { get; private set; }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Init(GameObject gamePrefab, GameContext context)
    {
        GameObject objGamePlay = GameMainPrefab; 
            //Instantiate(gamePrefab, GamePlayRoot);
        GamePlayView = objGamePlay.GetComponent<AView>();
        context.UICamera = Camera.main;// UICamera;

        //SlotMain = objSlot.GetComponent<SlotMainComponent>();
        //SlotMain.Init(context);
    }

    private void OnEnable()
    {
        EventSystem.DispatchEvent("PlayScreenView_OnEnable");
    }

    private void OnDisable()
    {
        EventSystem.DispatchEvent("PlayScreenView_OnDisable");
    } */   
}
