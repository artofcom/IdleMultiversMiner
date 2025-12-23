using UnityEngine;
using IGCore.MVCS;
using System;
using App.GamePlay.IdleMiner.Common.Model;
using Core.Util;


namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class PopupDialogController : AController
    {
        string dialogKey;
        PopupDialogView popupDialogView => (PopupDialogView)view;

        public PopupDialogController(string dialogKey, IGCore.MVCS.AView view, IGCore.MVCS.AModel model, IGCore.MVCS.AContext ctx)
            : base(view, model, ctx)
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

        internal APopupDialog DisplayPopupDialog(string id, IGCore.MVCS.AView.APresentor presentor, Action<APopupDialog> OnCloseCallBack)
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
                popupDialogView.Dialogs[idx].Display(presentor, OnCloseCallBack);
                return popupDialogView.Dialogs[idx];
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
        }

        object displayPopupDialog(params object[] data)
        {
            if(data.Length < 3)
                return null;

            string dlgId = (string)data[0];
            IGCore.MVCS.AView.APresentor presentor = (IGCore.MVCS.AView.APresentor)data[1];
            Action<APopupDialog> onCloseCallback = (Action<APopupDialog>)data[2];

            return DisplayPopupDialog(dlgId, presentor, onCloseCallback);
        }

        #endregion
    }
}