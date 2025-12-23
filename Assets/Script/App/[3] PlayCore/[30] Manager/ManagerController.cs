using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


namespace App.GamePlay.IdleMiner
{
    //  SkillTree Controlling.-------------------------------------
    //
    public class ManagerController : IGCore.MVCS.AController// AMinerModule
    {
        ManagerPanelController mngPanelController;


        //public ManagerController(IdleMinerController _controller) : base(_controller) { }
        
         public ManagerController(IGCore.MVCS.AView view, IGCore.MVCS.AModel model, IGCore.MVCS.AContext ctx)
            : base(view, model, ctx)
        { }


        public override void Init() {}

        protected override void OnViewEnable() { }
        protected override void OnViewDisable() { }

        public override void Resume(int awayTimeInSec) { }
        public override void Pump() { }
        public override void WriteData() { }

        // Recruit Dialog handler----------------------------------------
        //
        //
        void RecruitManagerItemComp_OnBtnClicked(string productId)
        {
           /* RecruitProductInfo product = Model.GetRecruitProductInfo(productId);
            Assert.IsNotNull(product);

            OwnedManagerInfo newManager = Model.RecruitManager(product.Cost, product.MinLevel, product.MaxLevel);
            if (newManager != null)
                mngPanelController?.DisplayManagerCard(newManager.Id, () => mngPanelController.RefreshView());
           */
        }

        void IncreaseManagerSlotDialog_OnBtnPurchaseClicked()
        {
            mngPanelController?.IncreaseSlotDlg.OnClose();
        }








        /*

        protected override void InitModule()
        {
            mngPanelController = new ManagerPanelController(controller);

            RecruitManagerItemComp.EventOnBtnClicked += RecruitManagerItemComp_OnBtnClicked;
            IncreaseManagerSlotDialog.EventOnBtnPurchaseClicked += IncreaseManagerSlotDialog_OnBtnPurchaseClicked;
        }*/
    }
}