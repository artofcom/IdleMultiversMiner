using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Core.Events;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using App.GamePlay.IdleMiner.SkillTree;
using App.GamePlay.IdleMiner.Common;

namespace App.GamePlay.IdleMiner.PopupDialog
{
    public class SkillTreePopupDialog : APopupDialog
    {
        
        // const string ------------------------------------
        //

        // Serialize Fields -------------------------------
        //
        [SerializeField] TMP_Text TextSkillName;
        [SerializeField] Image IconSkill;
        [SerializeField] TMP_Text TextDesc;

        [SerializeField] GameObject InProgressRoot;
        [SerializeField] GameObject ProjectDoneRoot;
        [SerializeField] GameObject UnreachableRoot;


        // In Progress Data.
        [SerializeField] List<ResourceStatComp> ReqItems;

        [SerializeField] Button BtnResearch;
        //[SerializeField] TMP_Text TextBtn;

        [SerializeField] GameObject BtnForceResearch;

        // Events ----------------------------------------
     //   //
       // static public UnityEvent<SkillPath.eType, string> EventOnBtnRearchClicked = new UnityEvent<SkillPath.eType, string>();

        public Action<string> EventOnBtnRearchClicked;
        public Action<string> EventOnBtnForceRearchClicked;

        // Members----------------------------------------
        //
        public static string sID { get; private set; }  // dlg id per dlg-class.
      //  SkillPath.eType pathType;
        string skillId;
        bool IsStarted = false;

        public string SkillId => skillId;
        public bool IsRearchClicked { get; private set; } = false;
        public class PresentInfo : APresentor
        {
            public PresentInfo(SKILL_STATUS status, string _skillId, string _skillName, Sprite _icon, string _desc, bool isLearnable, List<ResourceStatComp.PresentInfo> _info)
            {
                this.status = status;

                SkillId = _skillId;
                SkillName = _skillName;
                SpriteSkill = _icon;
                Desc = _desc;
                this.IsLearnable = isLearnable;

                RscStatInfos = _info;
                Assert.IsTrue(_info!=null && _info.Count <= SkillInfo.MAX_REQIREMENTS);
            }

            public PresentInfo(SKILL_STATUS status, string _skillId, string _skillName, Sprite _icon, string _desc)
            {
                this.status = status;
                SkillId = _skillId;
                SkillName = _skillName;
                SpriteSkill = _icon;
                Desc = _desc;
            }
         
            public SKILL_STATUS status { get; private set; }
            public string SkillId { get; private set; }
            public Sprite SpriteSkill { get; private set; }
            public string SkillName { get; private set; }
            public string Desc { get; private set; }
            public bool IsLearnable { get; private set; }

            public List<ResourceStatComp.PresentInfo> RscStatInfos { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (IsStarted) return;

            Assert.IsNotNull(TextSkillName);
            Assert.IsNotNull(IconSkill);
            Assert.IsNotNull(TextDesc);

            Assert.IsNotNull(InProgressRoot);
            Assert.IsNotNull(ProjectDoneRoot);

            Assert.IsNotNull(ReqItems);
          //  Assert.IsTrue(ReqItems.Count == SkillInfo.MAX_REQIREMENTS);

            Assert.IsNotNull(BtnResearch);
           // Assert.IsNotNull(TextBtn);

            /*if (BtnForceResearch != null)
            {
                BtnForceResearch.SetActive(
#if UNITY_EDITOR
                true);
#else
                false);
#endif
            }*/

            IsStarted = true;
        }

        protected override void OnEnable()
        {
            if (!IsStarted) Start();

            IsRearchClicked = false;

            //Events.RegisterEvent(RecipeSingleItemComp.EVENT_SLOT_CLICKED, SingleRecipeItemComp_OnBtnSlotClicked);
            //Events.RegisterEvent(RecipeSingleItemComp.EVENT_LOCKED_CLICKED, SingleRecipeItemComp_OnBtnLockedSlotClicked);
        }
        protected override void OnDisable()
        {
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



            skillId = presentInfo.SkillId;

            InProgressRoot.SetActive(presentInfo.status == SKILL_STATUS.LEARNING);
            ProjectDoneRoot.SetActive(presentInfo.status == SKILL_STATUS.LEARNED);
            UnreachableRoot.SetActive(presentInfo.status == SKILL_STATUS.UNREACHABLE);

            TextSkillName.text = presentInfo.SkillName;
            IconSkill.sprite = presentInfo.SpriteSkill;
            TextDesc.text = presentInfo.Desc;

            if(presentInfo.status == SKILL_STATUS.LEARNING)
            {
              //  TextBtn.text = presentInfo.IsWorkingSkill ? "RESEARCH" : "UNAVAILABLE";
                BtnResearch.interactable = presentInfo.IsLearnable;

                for(int q = 0; q < ReqItems.Count; ++q)
                {
                    if (q < presentInfo.RscStatInfos.Count)
                    {
                        ReqItems[q].gameObject.SetActive(true);
                        ReqItems[q].Refresh(presentInfo.RscStatInfos[q]);
                    }
                    else
                        ReqItems[q].gameObject.SetActive(false);
                }
            }
        }


        public void OnCloseBtnClicked()
        {
            OnClose();
        }

        public void OnBtnResearchClicked()
        {
            IsRearchClicked = true;

            EventOnBtnRearchClicked.Invoke(SkillId);
            OnClose();
        }

        public void OnBtnForceResearchClicked()
        {
#if UNITY_EDITOR
            EventOnBtnForceRearchClicked.Invoke(SkillId);
            OnClose();
#endif
        }
        
    }
}
