using App.GamePlay.IdleMiner.PopupDialog;
using IGCore.MVCS;
using UnityEngine;

public class SettingController : AController
{
    public SettingController(AView view, AModel model, AContext context) : base(view, model, context)
    { }

    public override void Init() 
    { 
        object queryRet = context.RequestQuery("AppPlayerModel", "IsBGMOn");
        bool bEnabled = queryRet != null ? (bool)queryRet : true;
        (view as SettingDialogView).EnableBGM(bEnabled);

        queryRet = context.RequestQuery("AppPlayerModel", "IsSoundFXOn");
        bEnabled = queryRet != null ? (bool)queryRet : true;
        (view as SettingDialogView).EnableSoundFX(bEnabled);
    }
    public override void Resume(int awayTimeInSec) { }
    public override void Pump() { }
    public override void WriteData() { }

    protected override void OnViewEnable()  
    { 
        SettingDialogView.EventOnBtnBGMClicked += EventOptionDlgOnBtnBGMClicked;
        SettingDialogView.EventOnBtnSoundFXClicked += EventOptionDlgOnBtnSoundFXClicked;
    }
    protected override void OnViewDisable() 
    {
        SettingDialogView.EventOnBtnBGMClicked -= EventOptionDlgOnBtnBGMClicked;
        SettingDialogView.EventOnBtnSoundFXClicked -= EventOptionDlgOnBtnSoundFXClicked;
    }


    void EventOptionDlgOnBtnBGMClicked(bool isOn)
    {
        Debug.Log("BGM has been clicked..." + isOn);
        context.RequestQuery("AppPlayerModel", "SetBGM", isOn);
        (view as SettingDialogView).EnableBGM(isOn);
    }
    void EventOptionDlgOnBtnSoundFXClicked(bool isOn)
    {
        Debug.Log("SoundFX has been clicked..." + isOn);
        context.RequestQuery("AppPlayerModel", "SetSoundFX", isOn);
        (view as SettingDialogView).EnableSoundFX(isOn);
    }
}
