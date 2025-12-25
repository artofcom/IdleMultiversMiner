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
    public class TasksPopupDialog : APopupDialog
    {
        // const string ------------------------------------
        //

        // Serialize Fields -------------------------------
        //
        [SerializeField] TMP_Text TextSkillName;
        [SerializeField] Image IconSkill;
        

        // In Progress Data.
        
        // Events ----------------------------------------
        //
        //static public UnityEvent<SkillPath.eType, string> EventOnBtnRearchClicked = new UnityEvent<SkillPath.eType, string>();


        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
        
        public class PresentInfo : APresentor
        {
            public PresentInfo()
            {
                
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
         //   Assert.IsNotNull(TextSkillName);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            //Events.RegisterEvent(RecipeSingleItemComp.EVENT_SLOT_CLICKED, SingleRecipeItemComp_OnBtnSlotClicked);
            //Events.RegisterEvent(RecipeSingleItemComp.EVENT_LOCKED_CLICKED, SingleRecipeItemComp_OnBtnLockedSlotClicked);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            //Events.UnRegisterEvent(RecipeSingleItemComp.EVENT_SLOT_CLICKED, SingleRecipeItemComp_OnBtnSlotClicked);
            //Events.UnRegisterEvent(RecipeSingleItemComp.EVENT_LOCKED_CLICKED, SingleRecipeItemComp_OnBtnLockedSlotClicked);
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
        }


        public void OnCloseBtnClicked()
        {
            OnClose();
        }
    }
}
