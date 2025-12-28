using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using Core.Util;
using IGCore.MVCS;
using IGCore.PlatformService.Cloud;
using System;
using System.Collections;
using UnityEngine;

public class TitleScreenController : AController
{
    const float WAIT_TIME_SEC = 1.5f;

    IdleMinerContext IMContext => (IdleMinerContext)context;

    TitleScreen titleScreen => unit as TitleScreen;

    public TitleScreenController(AUnit unit, AView view, AModel model, AContext ctx)
        : base(unit, view, model, ctx)
    {}

    public override void Init()
    {
        titleScreen.AuthService.EventOnSignedIn += OnSignedIn;
        titleScreen.AuthService.EventOnSignInFailed += OnSignInFailed;
    }

    protected override void OnViewEnable()
    {
        Debug.Log("============================= Title Enter ");

        //AsyncInitService();

        bool isLoginRequired = (bool)context.GetData("IsTitleViewLoginRequired", false);
        
        DelayedAction.TriggerActionWithDelay(IMContext.CoRunner, WAIT_TIME_SEC, () =>
        {
            if(!isLoginRequired)
                titleScreen.AuthService.SignInAsync();
            else
            {
                context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayUnitPopupDialog", (errMsg, ret) => { },
                    "LoginDialog",
                    new Action<APopupDialog>((popupDlg) =>
                    {
                        Debug.Log("Login Dialog has been closed.");
                    }));    
            }
        });
    }
    
    protected override void OnViewDisable() 
    { 
        context.UpdateData("IsTitleViewLoginRequired", false);
    }

    public override void Resume(int awayTimeInSec) { }
    
    public override void Pump() { }
    
    public override void WriteData() { }

    public override void Dispose()
    {
        base.Dispose();

        titleScreen.AuthService.EventOnSignedIn -= OnSignedIn;
        titleScreen.AuthService.EventOnSignInFailed -= OnSignInFailed;
    }
    
    void OnSignedIn(string playerId) 
    {
        Debug.Log($"<color=green>Login has been successed. PlayerId : [{playerId}]</color>");
        context.AddData("PlayerId", playerId);
        context.AddData("IsAccountLinked", titleScreen.AuthService.IsAccountLinkedWithPlayer("unity"));
        titleScreen.SwitchUnit("LobbyScreen");
    }
    void OnSignInFailed(string errorMessage)
    {
        Debug.Log($"<color=red>{errorMessage}</color>");
        titleScreen.SwitchUnit("LobbyScreen");
    }
}