using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;
using System; 

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class MessageDialog : APopupDialog
    {
        public enum Type { CONFIRM, YES_NO, };

        // Serialize Fields -------------------------------
        //
        [SerializeField] TMP_Text txtTitle;
        [SerializeField] TMP_Text txtMessage;
        [SerializeField] GameObject btnGroupOne;
        [SerializeField] GameObject btnGroupTwo;

        [SerializeField] TMP_Text txt01BtnConfirm;
        [SerializeField] TMP_Text txt02BtnOk;
        [SerializeField] TMP_Text txt02BtnCancel;

        Action callbackOnOk;

        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        
        public class PresentInfo : APresentor
        {            
            public PresentInfo(string message, string title = "Notice", Type type = Type.CONFIRM, Action callbackYes = null, 
                string okBtnMsg = "OK", string cancelBtnMsg = "CANCEL")
            {
                Message = message;
                Title = title;
                Type = type;
                BtnMsgOk = okBtnMsg;
                BtnMsgNo = cancelBtnMsg;
                YesCallback = callbackYes;
            }

            public string Message { get; private set; }
            public string Title { get; private set; }
            public string BtnMsgOk { get; private set; }
            public string BtnMsgNo { get; private set; }
            public Type Type { get; private set; }
            public Action YesCallback { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(txtTitle);
            Assert.IsNotNull(txtMessage);
            Assert.IsNotNull(btnGroupOne);
            Assert.IsNotNull(btnGroupTwo);
            
            Assert.IsNotNull(txt01BtnConfirm);
            Assert.IsNotNull(txt02BtnOk);
            Assert.IsNotNull(txt02BtnCancel);
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

            txtTitle.text = presentInfo.Title;
            txtMessage.text = presentInfo.Message;

            btnGroupOne.SetActive(presentInfo.Type == Type.CONFIRM);
            btnGroupTwo.SetActive(presentInfo.Type == Type.YES_NO);

            txt01BtnConfirm.text = presentInfo.BtnMsgOk;
            txt02BtnOk.text = presentInfo.BtnMsgNo;
            txt02BtnCancel.text = presentInfo.BtnMsgNo;

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
