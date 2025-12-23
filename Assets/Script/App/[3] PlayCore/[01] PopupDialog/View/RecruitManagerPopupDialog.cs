using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class RecruitManagerPopupDialog : APopupDialog
    {
        // const string ------------------------------------
        //
        static public readonly int SUPPORTING_ITEM_COUNT = 4;


        // Serialize Fields -------------------------------
        //
        [SerializeField] RecruitManagerItemComp ItemLv1;
        [SerializeField] RecruitManagerItemComp ItemLv2;
        [SerializeField] RecruitManagerItemComp ItemLv3;
        [SerializeField] RecruitManagerItemComp ItemLv4;
        


        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        public class PresentInfo : APresentor
        {
            public PresentInfo(RecruitManagerItemComp.PresentInfo _itemLv1,
                RecruitManagerItemComp.PresentInfo _itemLv2,
                RecruitManagerItemComp.PresentInfo _itemLv3, 
                RecruitManagerItemComp.PresentInfo _itemLv4)
            {
                ItemLv1 = _itemLv1;
                ItemLv2 = _itemLv2;
                ItemLv3 = _itemLv3;
                ItemLv4 = _itemLv4;
            }

            public RecruitManagerItemComp.PresentInfo ItemLv1 { get; private set; }
            public RecruitManagerItemComp.PresentInfo ItemLv2 { get; private set; }
            public RecruitManagerItemComp.PresentInfo ItemLv3 { get; private set; }
            public RecruitManagerItemComp.PresentInfo ItemLv4 { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(ItemLv1);
            Assert.IsNotNull(ItemLv2);
            Assert.IsNotNull(ItemLv3);
            Assert.IsNotNull(ItemLv4);


            RecruitManagerItemComp.EventOnBtnClicked += RecruitManagerItemComp_OnBtnClicked;
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

            ItemLv1.Refresh(presentInfo.ItemLv1);
            ItemLv2.Refresh(presentInfo.ItemLv2);
            ItemLv3.Refresh(presentInfo.ItemLv3);
            ItemLv4.Refresh(presentInfo.ItemLv4);
        }

        void RecruitManagerItemComp_OnBtnClicked(string productId)
        {
            OnClose();
        }

        public void OnCloseBtnClicked()
        {
            OnClose();
        }
    }
}
