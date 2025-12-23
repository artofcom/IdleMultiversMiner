using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class ShopPopupDialog : APopupDialog
    {
        // const string ------------------------------------
        //
        static public readonly int SUPPORTING_ITEM_COUNT = 3;

        static public Action<int> EventOnBuyClicked;

        // Serialize Fields -------------------------------
        //
        [SerializeField] ShopItemComp ItemLv12;
        [SerializeField] ShopItemComp ItemLv34;
        [SerializeField] ShopItemComp ItemLv5;
        


        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        public class PresentInfo : APresentor
        {
            public PresentInfo(ShopItemComp.PresentInfo _itemLv12,
                ShopItemComp.PresentInfo _itemLv34,
                ShopItemComp.PresentInfo _itemLv5)
            {
                ItemLv12 = _itemLv12;
                ItemLv34 = _itemLv34;
                ItemLv5 = _itemLv5;
            }

            public ShopItemComp.PresentInfo ItemLv12 { get; private set; }
            public ShopItemComp.PresentInfo ItemLv34 { get; private set; }
            public ShopItemComp.PresentInfo ItemLv5 { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
           // Assert.IsNotNull(ItemLv12);
            //Assert.IsNotNull(ItemLv34);
           // Assert.IsNotNull(ItemLv5);


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

         //   ItemLv12.Refresh(presentInfo.ItemLv12);
         //   ItemLv34.Refresh(presentInfo.ItemLv34);
         //   ItemLv5.Refresh(presentInfo.ItemLv5);
        }

        public void OnCloseBtnClicked()
        {
            OnClose();
        }

        public void OnBtnBuyClicked(int amount)
        {
            EventOnBuyClicked?.Invoke(amount);
        }
    }
}
