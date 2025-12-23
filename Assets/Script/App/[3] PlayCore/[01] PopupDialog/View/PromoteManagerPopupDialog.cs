using UnityEngine;
using UnityEngine.Assertions;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class PromoteManagerPopupDialog : APopupDialog
    {
        // const string ------------------------------------
        //

        // Events ------------------------------------
        //
        public static Action EventOnPromoteBtnClicked;


        // Serialize Fields -------------------------------
        //
        [SerializeField] ManagerList3ItemComp ListItemCache;
        [SerializeField] GameObject ContentRoot;

        [SerializeField] TMP_Text TxtDesc;
        [SerializeField] ManagerItemComp MainManager;
        [SerializeField] Button BtnPromote;


        // Members----------------------------------------
        //
        List<ManagerList3ItemComp> List3Items = new List<ManagerList3ItemComp>();

        public static string sID { get; private set; }  // dlg id per dlg-class.
        public class PresentInfo : APresentor
        {
            public PresentInfo(string _desc, bool _isPromotable,
                ManagerItemComp.PresentInfo _mainManager,
                List<ManagerList3ItemComp.PresentInfo> _list3ItemPresentInfo)
            {
                Desc = _desc;
                IsPromotable = _isPromotable;
                MainManagerPresentor = _mainManager;
                ManagerList3ItemPresentor = _list3ItemPresentInfo;
            }

            public string Desc { get; private set; }
            public bool IsPromotable { get; private set; }
            public ManagerItemComp.PresentInfo MainManagerPresentor { get; private set; }
            public List<ManagerList3ItemComp.PresentInfo> ManagerList3ItemPresentor { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(ListItemCache);
            Assert.IsNotNull(ContentRoot);
            Assert.IsNotNull(TxtDesc);
            Assert.IsNotNull(MainManager);
            Assert.IsNotNull(BtnPromote);

            ListItemCache.gameObject.SetActive(false);

            //RecruitManagerItemComp.EventOnBtnClicked += RecruitManagerItemComp_OnBtnClicked;
        }

        public override void InitDialog(string id)
        {
            base.InitDialog(id);
            sID = id;
        }

        public override void Refresh(APresentor _presentor)
        {
            if (_presentor == null)
                return;

            var presentor = (PresentInfo)_presentor;
            if (presentor == null)
                return;

            for (int q = 0; q < List3Items.Count; ++q)
                Destroy(List3Items[q].gameObject);
            List3Items.Clear();

            TxtDesc.text = presentor.Desc;

            MainManager.Refresh(presentor.MainManagerPresentor);
            BtnPromote.interactable = presentor.IsPromotable;

            // Manager List.
            for (int q = 0; q < presentor.ManagerList3ItemPresentor.Count; ++q)
            {
                var newObj = Instantiate(ListItemCache, ContentRoot.transform);
                var itemComp = newObj.GetComponent<ManagerList3ItemComp>();
                Assert.IsNotNull(itemComp);

                itemComp.Refresh(presentor.ManagerList3ItemPresentor[q]);
                itemComp.gameObject.SetActive(true);
                List3Items.Add(itemComp);
            }
        }

        public void OnCloseBtnClicked()
        {
            OnClose();
        }

        public void OnBtnPromoteClicked()
        {
            EventOnPromoteBtnClicked?.Invoke();
        }
    }
}
