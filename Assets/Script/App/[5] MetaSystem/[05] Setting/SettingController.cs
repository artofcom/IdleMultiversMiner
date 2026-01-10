using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using IGCore.MVCS;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class SettingController : AController
{
    SettingDialogView View;
    SettingUnit settingUnit;

    public SettingController(AUnit unit, AView view, AModel model, AContext context) : base(unit, view, model, context)
    { }

    public override void Init() 
    { 
        base.Init();

        settingUnit = unit as SettingUnit;
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

    public override void Dispose()
    {
        base.Dispose();

        settingUnit = null;
        View = null;
    }

    protected override void OnViewEnable()  
    { 
        SettingDialogView.EventOnBtnBGMClicked += EventOptionDlgOnBtnBGMClicked;
        SettingDialogView.EventOnBtnSoundFXClicked += EventOptionDlgOnBtnSoundFXClicked;
        SettingDialogView.EventOnLinkAccountClicked += EventOptionDlgOnBtnLinkAccountClicked;
        SettingDialogView.EventOnUnlinkAccountClicked += EventOptionDlgOnBtnUnlinkAccountClicked;
        SettingDialogView.EventOnSignOutClicked += EventOnSignOutClicked;
        SettingDialogView.EventOnDeleteAccountClicked += EventOnDeleteAccountClicked;
        SettingDialogView.EventOnAccountManagementClicked += EventOnAccountManagementClicked;
        SettingDialogView.EventOnCloseClicked += EventOnCloseClicked;

        settingUnit.AuthService.EventOnLinkAccount += EventOnLinkAccount;

        RefreshView();

        Debug.Log("[Setting] OnViewEnabled");
    }
    protected override void OnViewDisable() 
    {
        (context as IdleMinerContext).SaveMetaDataInstantly();

        SettingDialogView.EventOnBtnBGMClicked -= EventOptionDlgOnBtnBGMClicked;
        SettingDialogView.EventOnBtnSoundFXClicked -= EventOptionDlgOnBtnSoundFXClicked;
        SettingDialogView.EventOnLinkAccountClicked -= EventOptionDlgOnBtnLinkAccountClicked;
        SettingDialogView.EventOnUnlinkAccountClicked -= EventOptionDlgOnBtnUnlinkAccountClicked;
        SettingDialogView.EventOnSignOutClicked -= EventOnSignOutClicked;
        SettingDialogView.EventOnDeleteAccountClicked -= EventOnDeleteAccountClicked;
        SettingDialogView.EventOnAccountManagementClicked -= EventOnAccountManagementClicked;
        SettingDialogView.EventOnCloseClicked -= EventOnCloseClicked;

        settingUnit.AuthService.EventOnLinkAccount -= EventOnLinkAccount;
        Debug.Log("[Setting] OnViewDisable");
    }



    void RefreshView()
    {
        string playerId = (string)context.GetData("PlayerId");
        playerId = playerId.Contains("GUEST") ? "GUEST_PLAYER" : playerId;

        var presentInfo = new SettingDialogView.PresentInfo(
                (bool)context.RequestQuery("AppPlayerModel", "IsSoundFXOn"), 
                (bool)context.RequestQuery("AppPlayerModel", "IsBGMOn"), 
                isSignedIn : settingUnit.AuthService.IsSignedIn(),
                (bool)context.GetData("IsAccountLinked"),
                playerId,  "Version " + Application.version);

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
        await settingUnit.AuthService.LinkAccountWithPlayer();
    }
    async void EventOptionDlgOnBtnUnlinkAccountClicked()
    {
        if(await settingUnit.AuthService.UnlinkAccountWithPlayer())
        {
            await Task.Delay(100);

            context.UpdateData("IsAccountLinked", settingUnit.AuthService.IsAccountLinkedWithPlayer("unity"));
            RefreshView();
        }
    }
    void EventOnSignOutClicked() 
    { 
        // (unit as SettingUnit).ShouldSignOut = true;

        context.UpdateData("IsTitleViewLoginRequired", true);
        settingUnit.AuthService.SignOut();
                
        var presentData = new ToastMessageDialog.PresentInfo( message :  "Player has signed out.", duration:1.5f, ToastMessageDialog.Type.INFO);
        context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
            "ToastMessageDialog", presentData,
            new Action<APopupDialog>( (popupDlg) => 
            { 
                Debug.Log("ToastMessage Dialog has been closed.");
                unit.Detach();

            } ) ); 
    }

    void EventOnDeleteAccountClicked() 
    { 
        //(unit as SettingUnit).ShouldDeleteAccount = true;
        //unit.Detach();

        var presentInfo = new MessageDialog.PresentInfo( 
                message :  "If you proceed, all your progress will be permanently deleted. You will not be able to recover this data later.\r\n\r\nAre you sure you want to delete everything?", 
                title : "Delete Guest Account?", type : MessageDialog.Type.YES_NO_TO_WORSE, 
                callbackYes : () => 
                {
                    // (unit as SettingUnit).ShouldDeleteAccount = true;
                    
                    context.UpdateData("IsTitleViewLoginRequired", true);
                    settingUnit.AuthService.SignOut();
                    unit.Detach();

                }, "Delete Forever", "Cancel");


        context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
            "MessageDialog",  
            presentInfo,
            new Action<APopupDialog>( (popupDlg) => 
            { 
                Debug.Log("Message Dialog has been closed.");
            } ) );  
    }

    void EventOnLinkAccount(bool successed, string errMessage)
    {
        if(successed)
        {
            context.UpdateData("IsAccountLinked", settingUnit.AuthService.IsAccountLinkedWithPlayer("unity"));
            RefreshView();
        }
        else
        {
            var presentInfo = new MessageDialog.PresentInfo( 
                message :  errMessage, 
                title : "Linking Account has been failed!", type : MessageDialog.Type.CONFIRM);

            context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
                "MessageDialog",  
                presentInfo,
                new Action<APopupDialog>( (popupDlg) => 
                { 
                    Debug.Log("Message Dialog has been closed.");
                } ) );  
        }
    }

    void EventOnAccountManagementClicked()
    {
        Application.OpenURL( settingUnit.AuthService.GetManagementURL() );
    }
    void EventOnCloseClicked()
    {
        unit.Detach();
    }
}
