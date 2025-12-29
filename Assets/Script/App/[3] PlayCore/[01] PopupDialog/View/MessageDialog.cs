using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;
using System; 

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class MessageDialog : APopupDialog
    {
        public enum Type { CONFIRM, YES_NO_TO_BETTER, YES_NO_TO_WORSE };

        // Serialize Fields -------------------------------
        //
        [SerializeField] TMP_Text txtTitle;
        [SerializeField] TMP_Text txtMessage;
        [SerializeField] GameObject btnGroupOne;
        [SerializeField] GameObject btnGroupTwoBetter;
        [SerializeField] GameObject btnGroupTwoWorse;

        [SerializeField] TMP_Text txt01BtnConfirm;
        [SerializeField] TMP_Text txt02BetterBtnOk;
        [SerializeField] TMP_Text txt02BetterBtnCancel;
        [SerializeField] TMP_Text txt02WorseBtnOk;
        [SerializeField] TMP_Text txt02WorseBtnCancel;

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
            Assert.IsNotNull(btnGroupTwoBetter);
            Assert.IsNotNull(btnGroupTwoWorse);
            
            Assert.IsNotNull(txt01BtnConfirm);
            Assert.IsNotNull(txt02BetterBtnOk);
            Assert.IsNotNull(txt02BetterBtnCancel);
            Assert.IsNotNull(txt02WorseBtnOk);
            Assert.IsNotNull(txt02WorseBtnCancel);
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
            btnGroupTwoBetter.SetActive(presentInfo.Type == Type.YES_NO_TO_BETTER);
            btnGroupTwoWorse.SetActive(presentInfo.Type == Type.YES_NO_TO_WORSE);

            txt01BtnConfirm.text = presentInfo.BtnMsgOk;
            txt02BetterBtnOk.text = presentInfo.BtnMsgOk;
            txt02BetterBtnCancel.text = presentInfo.BtnMsgNo;
            txt02WorseBtnOk.text = presentInfo.BtnMsgOk;
            txt02WorseBtnCancel.text = presentInfo.BtnMsgNo;

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
