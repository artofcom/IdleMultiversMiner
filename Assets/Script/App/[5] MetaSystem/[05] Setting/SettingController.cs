using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using IGCore.MVCS;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class SettingController : AController
{
    SettingDialogView View;

    public SettingController(AUnit unit, AView view, AModel model, AContext context) : base(unit, view, model, context)
    { }

    public override void Init() 
    { 
        View = (view as SettingDialogView);
        object queryRet = context.RequestQuery("AppPlayerModel", "IsBGMOn");
        bool bEnabled = queryRet != null ? (bool)queryRet : true;
        View.EnableBGM(bEnabled);

        queryRet = context.RequestQuery("AppPlayerModel", "IsSoundFXOn");
        bEnabled = queryRet != null ? (bool)queryRet : true;
        View.EnableSoundFX(bEnabled);
    }
    public override void Resume(int awayTimeInSec) { }
    public override void Pump() { }
    public override void WriteData() { }

    protected override void OnViewEnable()  
    { 
        SettingDialogView.EventOnBtnBGMClicked += EventOptionDlgOnBtnBGMClicked;
        SettingDialogView.EventOnBtnSoundFXClicked += EventOptionDlgOnBtnSoundFXClicked;
        SettingDialogView.EventOnLinkAccountClicked += EventOptionDlgOnBtnLinkAccountClicked;
        SettingDialogView.EventOnUnlinkAccountClicked += EventOptionDlgOnBtnUnlinkAccountClicked;
        SettingDialogView.EventOnSignOutClicked += EventOnSignOutClicked;
        SettingDialogView.EventOnDeleteAccountClicked += EventOnDeleteAccountClicked;

        View.AuthService.EventOnLinkAccount += EventOnLinkAccount;

        RefreshView();
    }
    protected override void OnViewDisable() 
    {
        SettingDialogView.EventOnBtnBGMClicked -= EventOptionDlgOnBtnBGMClicked;
        SettingDialogView.EventOnBtnSoundFXClicked -= EventOptionDlgOnBtnSoundFXClicked;
        SettingDialogView.EventOnLinkAccountClicked -= EventOptionDlgOnBtnLinkAccountClicked;
        SettingDialogView.EventOnUnlinkAccountClicked -= EventOptionDlgOnBtnUnlinkAccountClicked;
        SettingDialogView.EventOnSignOutClicked -= EventOnSignOutClicked;
        SettingDialogView.EventOnDeleteAccountClicked -= EventOnDeleteAccountClicked;

        View.AuthService.EventOnLinkAccount -= EventOnLinkAccount;
    }



    void RefreshView()
    {
        var presentInfo = new SettingDialogView.PresentInfo(
                (bool)context.RequestQuery("AppPlayerModel", "IsSoundFXOn"), 
                (bool)context.RequestQuery("AppPlayerModel", "IsBGMOn"), 
                isOffline:false, 
                (bool)context.GetData("IsAccountLinked"),
                (string)context.GetData("PlayerId"));

        View.Refresh(presentInfo);
    }

    void EventOptionDlgOnBtnBGMClicked(bool isOn)
    {
        Debug.Log("BGM has been clicked..." + isOn);
        context.RequestQuery("AppPlayerModel", "SetBGM", isOn);
        View.EnableBGM(isOn);
    }
    void EventOptionDlgOnBtnSoundFXClicked(bool isOn)
    {
        Debug.Log("SoundFX has been clicked..." + isOn);
        context.RequestQuery("AppPlayerModel", "SetSoundFX", isOn);
        View.EnableSoundFX(isOn);
    }
    async void EventOptionDlgOnBtnLinkAccountClicked()
    {
        await View.AuthService.LinkAccountWithPlatform();
    }
    async void EventOptionDlgOnBtnUnlinkAccountClicked()
    {
        if(await View.AuthService.UnlinkAccountWithPlatform())
        {
            await Task.Delay(100);

            context.UpdateData("IsAccountLinked", View.AuthService.IsAccountLinkedWithIdentity("unity"));
            RefreshView();
        }
    }
    void EventOnSignOutClicked() 
    { 
        (unit as SettingUnit).ShouldSignOut = true;
        unit.Detach();
    }

    void EventOnDeleteAccountClicked() 
    { 
        //(unit as SettingUnit).ShouldDeleteAccount = true;
        //unit.Detach();

        var presentInfo = new MessageDialog.PresentInfo( 
                message :  "About to delete Account, continue?", 
                title : "Warnning", type : MessageDialog.Type.YES_NO, 
                callbackYes : () => 
                {
                    View.AuthService.SignOut();

                    (unit as SettingUnit).ShouldDeleteAccount = true;
                    unit.Detach();

                }, "YES", "NO");

        context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.LOBBY_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
            "MessageDialog",  
            presentInfo,
            new Action<APopupDialog>( (popupDlg) => 
            { 
                Debug.Log("Message Dialog has been closed.");
            } ) );  
    }

    void EventOnLinkAccount(bool successed)
    {
        if(successed)
        {
            context.UpdateData("IsAccountLinked", View.AuthService.IsAccountLinkedWithIdentity("unity"));
            RefreshView();
        }
    }
}
