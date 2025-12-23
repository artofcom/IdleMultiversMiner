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
    public class DailyAttendItem : APopupDialog
    {
        // Serialize Fields -------------------------------
        //
        [SerializeField] TMP_Text TextDay;
        [SerializeField] TMP_Text TextContent;
        [SerializeField] Image itemIcon;
        [SerializeField] GameObject objectNoti;
        [SerializeField] GameObject objectClaimed;
        [SerializeField] Button buttonIcon;

        public enum State { CLAIMABLE, CLAIMED, UNREACHABLE, };

        // In Progress Data.
        int itemId;
        State eState = State.UNREACHABLE;

        // Events ----------------------------------------
        //
        static public Action<int> EventOnItemClicked;


        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        
        public class PresentInfo : APresentor
        {
            public PresentInfo(int id, string dayString, string strTitle, State state)
            {
                this.itemId = id;
                this.strTitle = strTitle;
                this.strDay = dayString;
                this.eState = state;
            }

            public int itemId { get; private set; }
            public string strDay { get; private set; }
            public string strTitle { get; private set; }
            public State eState { get; private set; }
        }

        // Start is called before the first frame update
        void Awake()
        {
            Assert.IsNotNull(TextDay);
            Assert.IsNotNull(TextContent);
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

            itemId = presentInfo.itemId;

            TextDay.text = presentInfo.strDay;
            TextContent.text = presentInfo.strTitle;

            eState = presentInfo.eState;

            buttonIcon.interactable = eState == State.CLAIMABLE;
            itemIcon.color = eState==State.CLAIMABLE ? Color.white : Color.gray;

            objectNoti.SetActive(eState == State.CLAIMABLE);
            objectClaimed.SetActive(eState == State.CLAIMED);
        }


        public void OnBtnItemClicked()
        {
            EventOnItemClicked?.Invoke(this.itemId);
        }
    }
}
