using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Core.Events;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using App.GamePlay.IdleMiner.PopupDialog;

namespace App.GamePlay.IdleMiner
{
    public class DailyAttendPopupDialog : APopupDialog
    {
        // const string ------------------------------------
        //

        // Serialize Fields -------------------------------
        //
        [SerializeField] List<DailyAttendItem> AttendRewardItems;
        [SerializeField] TMP_Text txtDesc;
        [SerializeField] Button btnClaim;
        [SerializeField] TMP_Text txtMsgClaimBtn;

        // In Progress Data.
        
        // Events ----------------------------------------
        //
        static public Action<int> EventOnBtnClaimClicked;


        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        
        public class PresentInfo : APresentor
        {
            public PresentInfo(List<DailyAttendItem.PresentInfo> listPresentData, bool claimable, string desc)
            {
                this.desc = desc;
                this.Claimable = claimable;
                this.listPresentData = listPresentData;
            }

            public List<DailyAttendItem.PresentInfo> listPresentData { get; private set; }
            public bool Claimable { get; private set; }
            public string desc { get; private set; }
        }

        // Start is called before the first frame update
        void Awake()
        {
            Assert.IsNotNull(AttendRewardItems);
            Assert.IsNotNull(txtDesc);
            Assert.IsNotNull(btnClaim); 
            Assert.IsNotNull(txtMsgClaimBtn);

            DailyAttendItem.EventOnItemClicked += DailyAttendItem_OnItemClicked;
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


            Assert.IsTrue(presentInfo.listPresentData.Count == AttendRewardItems.Count);

            this.txtDesc.text = presentInfo.desc;

            btnClaim.interactable = presentInfo.Claimable;
            txtMsgClaimBtn.text = presentInfo.Claimable ? "ATTEND" : "CLAIMED";

            for(int q = 0; q < AttendRewardItems.Count; q++) 
            {
                // AttendRewardItems[q].Refresh(presentInfo.listPresentData[q]);
            }
        }

        public void OnCloseBtnClicked()
        {
            OnClose();
        }

        public void OnBtnAttendClicked()
        {
            EventOnBtnClaimClicked?.Invoke(-1);
        }

        void DailyAttendItem_OnItemClicked(int idxDay)
        {
            EventOnBtnClaimClicked?.Invoke(idxDay);
        }
    }
}
