using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;
using System; 

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class IncreaseManagerSlotDialog : APopupDialog
    {
        // const string / Events ------------------------------------
        //
        public static Action EventOnBtnPurchaseClicked;


        // Serialize Fields -------------------------------
        //
        [SerializeField] TMP_Text txtPrice;
        [SerializeField] Button btnPurchase;



        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        public bool IsPurchasedClicked { get; private set; }

        public class PresentInfo : APresentor
        {
            public PresentInfo(string _price, bool _isBtnEnabled)
            {
                Price = _price;
                IsBtnEnabled = _isBtnEnabled;
            }

            public string Price { get; private set; }
            public bool IsBtnEnabled { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(txtPrice);
            Assert.IsNotNull(btnPurchase);
        }

        public override void InitDialog(string id)
        {
            base.InitDialog(id);
            sID = id;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            IsPurchasedClicked = false;
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var presentInfo = (PresentInfo)presentor;
            if (presentInfo == null)
                return;

            btnPurchase.interactable = presentInfo.IsBtnEnabled;
            txtPrice.text = presentInfo.Price;
        }

        public void OnCloseBtnClicked()
        {
            IsPurchasedClicked = false;
            OnClose();
        }

        public void OnBtnPurchaseClicked()
        {
            IsPurchasedClicked = true;
            EventOnBtnPurchaseClicked?.Invoke();
        }
    }
}
