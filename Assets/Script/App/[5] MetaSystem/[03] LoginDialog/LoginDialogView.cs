using App.GamePlay.IdleMiner.PopupDialog;
using UnityEngine;
using System;

public class LoginDialogView : APopupDialog
{
    public Action EventOnLoginClicked;
    public Action EventOnAnonymLoginClicked;


    public class PresentInfo : APresentor
    {
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public override void Refresh(APresentor presentor)
    {    
    }

    public void OnBtnLoginClicked()
    {
        EventOnLoginClicked?.Invoke();
    }

    public void OnBtnAnonymLoginClicked()
    {
        EventOnAnonymLoginClicked?.Invoke();
    }
}
