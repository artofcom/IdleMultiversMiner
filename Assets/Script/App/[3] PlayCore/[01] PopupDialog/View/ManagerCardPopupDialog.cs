using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class ManagerCardPopupDialog : APopupDialog
    {
        // const string ------------------------------------
        //


        // Serialize Fields -------------------------------
        //
        [SerializeField] ManagerItemComp MngItem;
        

        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        public class PresentInfo : APresentor
        {
            public PresentInfo(ManagerItemComp.PresentInfo _item)
            {
                Item = _item;
            }

            public ManagerItemComp.PresentInfo Item { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(MngItem);

            //RecruitManagerItemComp.EventOnBtnClicked += RecruitManagerItemComp_OnBtnClicked;
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

            MngItem.Refresh(presentInfo.Item);
        }

        public void OnCloseBtnClicked()
        {
            OnClose();
        }
    }
}
