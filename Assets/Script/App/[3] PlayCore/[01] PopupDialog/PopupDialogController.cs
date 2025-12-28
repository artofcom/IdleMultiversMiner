using UnityEngine;
using IGCore.MVCS;
using System;
using Core.Util;


namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class PopupDialogController : AController
    {
        string dialogKey;
        PopupDialogView popupDialogView => (PopupDialogView)view;

        public PopupDialogController(string dialogKey, AUnit unit, AView view, AModel model, AContext ctx)
            : base(unit, view, model, ctx)
        { 
            // init.

            this.dialogKey = dialogKey;
            RegisterRequestables();
        }

        public override void Init() {}

        protected override void OnViewEnable() { }
        protected override void OnViewDisable() { }

        public override void Resume(int awayTimeInSec) { }
        public override void Pump() { }
        public override void WriteData() { }

        public override void Dispose()
        {
            base.Dispose();
            context.RemoveRequestDelegate(dialogKey, "DisplayPopupDialog");
        }

        #region ===> Interfaces

        APopupDialog DisplayPopupDialog(string id, AView.APresentor presentor, Action<APopupDialog> OnCloseCallBack)
        {
            int idx = -1;
            for(int q = 0; q < popupDialogView.Dialogs.Count; ++q)
            {
                if(popupDialogView.Dialogs[q].name.ToLower() == id.ToLower())
                {
                    idx = q;
                    break;
                }
            }

            if (idx >= 0)
            {
                popupDialogView.Dialogs[idx].OnCloseCallback += OnCloseCallBack;
                popupDialogView.Dialogs[idx].Display(presentor);
                return popupDialogView.Dialogs[idx];
            }
            return null;
        }

        AUnit DisplayPopupDialog(string id, Action<APopupDialog> OnCloseCallBack)
        {
            for(int q = 0; q < popupDialogView.DialogUnits.Count; ++q) 
            {
                if(string.Compare(popupDialogView.DialogUnits[q].name, id, ignoreCase:true) == 0)
                {
                    if(!popupDialogView.DialogUnits[q].IsAttached)
                    {
                        (popupDialogView.DialogUnits[q].View as APopupDialog).OnCloseCallback += OnCloseCallBack;
                        popupDialogView.DialogUnits[q].Attach();
                    }

                    return popupDialogView.DialogUnits[q];
                }
            }
            return null;
        }

        public bool IsVisible(string id)
        {
            for (int q = 0; q < popupDialogView.Dialogs.Count; ++q)
            {
                if (popupDialogView.Dialogs[q].name.ToLower() == id.ToLower())
                    return popupDialogView.Dialogs[q].gameObject.activeSelf;
            }
            return false;
        }

        #endregion



        #region ===> Requestables

        void RegisterRequestables()
        {
            context.AddRequestDelegate(dialogKey, "DisplayPopupDialog", displayPopupDialog);
            context.AddRequestDelegate(dialogKey, "DisplayUnitPopupDialog", displayUnitPopupDialog);
        }

        object displayPopupDialog(params object[] data)
        {
            if(data.Length < 3)
                return null;

            string dlgId = (string)data[0];
            AView.APresentor presentor = (AView.APresentor)data[1];
            Action<APopupDialog> onCloseCallback = (Action<APopupDialog>)data[2];

            return DisplayPopupDialog(dlgId, presentor, onCloseCallback);
        }
        object displayUnitPopupDialog(params object[] data)
        {
            if(data.Length < 1)
                return null;

            string dlgId = (string)data[0];
            Action<APopupDialog> onCloseCallback = (Action<APopupDialog>)data[1];

            return DisplayPopupDialog(dlgId, onCloseCallback);
        }

        #endregion
    }
}