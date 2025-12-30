using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using System;
using Core.Util;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class ToastMessageDialog : APopupDialog
    {
        public enum Type { WARNING, INFO, };

        // Serialize Fields -------------------------------
        //
        [SerializeField] TMP_Text txtMessage;
        [SerializeField] Color32 colorInfo, colorWarning;
        
        Action callbackOnOk;

        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        
        public class PresentInfo : APresentor
        {            
            public PresentInfo(string message, float duration = 3.0f, Type msgType = Type.WARNING)
            {
                Message = message;
                this.duration = duration;
                type = msgType;
            }

            public string Message { get; private set; }
            public float duration { get; private set; }
            public Type type { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(txtMessage);
        }

        public override void InitDialog(string id)
        {
            base.InitDialog(id);
            sID = id;
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var presentInfo = (PresentInfo)presentor;
            if (presentInfo == null)
                return;

            txtMessage.color = presentInfo.type == Type.INFO ? colorInfo : colorWarning;
            txtMessage.text = presentInfo.Message;

            DelayedAction.TriggerActionWithDelay(this, presentInfo.duration, () =>
            {
                OnClose();
            });
        }
    }
}
