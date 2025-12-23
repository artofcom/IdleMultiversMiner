using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using System; 

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class GameResetDialog : APopupDialog
    {
        public enum Type { CONFIRM, YES_NO, };

        // Serialize Fields -------------------------------
        //
        [SerializeField] TMP_Text txtMessage;
        
        Action callbackOnOk;
        
        public class PresentInfo : APresentor
        {            
            public PresentInfo(string message, Action callbackYes)
            {
                Message = message;
                YesCallback = callbackYes;
            }

            public string Message { get; private set; }
            public Action YesCallback { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(txtMessage);
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var presentInfo = (PresentInfo)presentor;
            if (presentInfo == null)
                return;

            txtMessage.text = presentInfo.Message;

            callbackOnOk = presentInfo.YesCallback;
        }

        public void OnCloseBtnClicked()
        {
            OnClose();
        }

        public void OnBtnConfirmClicked()
        {
            callbackOnOk?.Invoke();
            OnClose();
        }
    }
}
