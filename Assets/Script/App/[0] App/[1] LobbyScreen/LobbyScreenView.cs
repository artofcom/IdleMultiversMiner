using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner;
using IGCore.MVCS;


public class LobbyScreenView : AView
{
    static public Action<string> EventOnBtnStart;
    static public Action EventOnBtnOptionDialogClicked;
    static public Action EventOnBtnShopDialogClicked;
    static public Action EventOnBtnDailyMissionClicked;

    [SerializeField] List<GameObject> views;
    [SerializeField] TopUIComp topHUDView;
    [SerializeField] Transform BGMListRoot;
    [SerializeField] Transform SoundFXListRoot;
    
    public TopUIComp TopHUDView => topHUDView;

    public class Presentor : APresentor
    {
        public Presentor(GameCardsPortalComp.Presentor gameCardsPresentor)
        {
            GameCardsPresentor = gameCardsPresentor;
        }   

        public GameCardsPortalComp.Presentor GameCardsPresentor { get; private set; }
    }

    // Start is called before the first frame update
    private void Awake()
    {
        Assert.IsTrue(views!=null && views.Count>0);
    }
    void Start()
    {
          for(int q = 0; q < views.Count; ++q)
            views[q].gameObject.SetActive(false);

        views[1].gameObject.SetActive(true);
        (views[1].GetComponent<GameCardsPortalComp>()).EventGameCardClicked += OnBtnGameStart;
    }


    public override void Refresh(APresentor presentData)
    {
        Presentor presentor = presentData as Presentor;
        if(presentor == null)
            return;

        (views[1].GetComponent<GameCardsPortalComp>()).Refresh(presentor.GameCardsPresentor);
    }

    public AView GetGameCardView(string gameKey)
    {
        return (views[1].GetComponent<GameCardsPortalComp>()).GetGameCardView(gameKey);
    }

    void OnBtnGameStart(string gameKey)
    {
        EventOnBtnStart?.Invoke(gameKey);
    }

    public void OnBtnParkClicked()
    {
        for(int q = 0; q < views.Count; ++q)
            views[q].gameObject.SetActive(false);

        views[0].gameObject.SetActive(true);
    }
    public void OnBtnGamePortalClicked()
    {
        for(int q = 0; q < views.Count; ++q)
            views[q].gameObject.SetActive(false);

        views[1].gameObject.SetActive(true);
    }
    public void OnBtnOptionClicked()
    {
        EventOnBtnOptionDialogClicked?.Invoke();
    }

    public void OnBtnShopClicked()
    {
        EventOnBtnShopDialogClicked?.Invoke();
    }

    public void OnBtnDailyMissionClicked()
    {
        EventOnBtnDailyMissionClicked?.Invoke();
    }

    public void EnableSoundFX(bool enable)
    {
        if(SoundFXListRoot == null) return;

        for(int q = 0; q < SoundFXListRoot.childCount; ++q)
        {
            var audio = SoundFXListRoot.GetChild(q).GetComponent<AudioSource>();
            if(audio == null) continue;

            audio.volume = enable ? 0.9f : .0f;
        }
    }

    public void EnableBGM(bool enable)
    {
        if(BGMListRoot == null) return;

        for(int q = 0; q < BGMListRoot.childCount; ++q)
        {
            var audio = BGMListRoot.GetChild(q).GetComponent<AudioSource>();
            if(audio == null) continue;

            audio.volume = enable ? 0.9f : .0f;
        }
    }

    /*
     * 
    public const string EVENT_BTN_OPTION_CLICKED = "LobbyScreenView_OnBtnOptionClicked";
    public const string EVENT_BTN_PLAY_CLICKED = "LobbyScreenView_OnBtnPlayClicked";
    public const string EVENT_BTN_BACK_CLICKED = "LobbyScreenView_OnBtnBackClicked";

    public void OnBtnPlayClicked()
    {
        EventSystem.DispatchEvent(EVENT_BTN_PLAY_CLICKED);
    }

    public void OnBtnDailyChallengeClicked()
    {
        Debug.Log("DC Clicked.");
    }

    // Top Hud
    public void OnBtnBackClicked()
    {
        EventSystem.DispatchEvent(EVENT_BTN_BACK_CLICKED);
    }

    public void OnBtnOptionClicked()
    {
        EventSystem.DispatchEvent(EVENT_BTN_OPTION_CLICKED);
    }

    public void OnBtnBuyClicked()
    {
        Debug.Log("Buy Clicked.");
    }

    public void OnBtnXPClicked()
    {
        Debug.Log("XP Clicked.");
    }

    public void OnBtnChipClicked()
    {
        Debug.Log("Chip Clicked.");
    }*/
}
