using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using UnityEngine.UI;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class RecruitManagerItemComp : MonoBehaviour
    {
        [SerializeField] TMP_Text TxtName;
        [SerializeField] TMP_Text TxtDesc;
        [SerializeField] TMP_Text TxtRecruitBtn;
        [SerializeField] Button BtnRecruit;

        // Event.
        public static System.Action<string> EventOnBtnClicked = null;

        public string CompId { get; private set; }

        public class PresentInfo
        {
            public PresentInfo( string _productId, string _name, string _desc, string _btnDesc, bool _enableBtn)
            {
                ProductId = _productId;
                Name = _name;   Desc = _desc;   BtnDesc = _btnDesc;
                BtnEnabled = _enableBtn;
            }

            public string ProductId { get; private set; }
            public string Name { get; private set; }
            public string Desc { get; private set; }
            public string BtnDesc { get; private set; }
            public bool BtnEnabled { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(TxtName);
            Assert.IsNotNull(TxtDesc);
            Assert.IsNotNull(TxtRecruitBtn);
            Assert.IsNotNull(BtnRecruit);
        }

        public void Refresh(PresentInfo presentor)
        {
            Assert.IsNotNull(presentor);

            CompId = presentor.ProductId;
            TxtName.text = presentor.Name;
            TxtDesc.text = presentor.Desc;
            TxtRecruitBtn.text = presentor.BtnDesc;
            BtnRecruit.interactable = presentor.BtnEnabled;
        }

        public void OnClicked()
        {
            EventOnBtnClicked?.Invoke(CompId);
        }
    }
}
