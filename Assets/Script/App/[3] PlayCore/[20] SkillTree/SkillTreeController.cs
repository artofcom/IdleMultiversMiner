//using System.Collections;
using App.GamePlay.IdleMiner.Common;
using App.GamePlay.IdleMiner.Common.Model;
using App.GamePlay.IdleMiner.Common.PlayerModel;
using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using Core.Events;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner;
using IGCore.MVCS;

namespace App.GamePlay.IdleMiner.SkillTree
{
    //  SkillTree Controlling.-------------------------------------
    //
    public class SkillTreeController : IGCore.MVCS.AController // AMinerModule
    {
        SkillTreePopupDialog mSkillTreePopupDlgCache = null;

        EventsGroup Events = new EventsGroup();

        SkillTreeView View => view as SkillTreeView;
        SkillTreeModel STModel => model as SkillTreeModel;
        IdleMinerContext IMContext => (IdleMinerContext)context;


        const int PUMP_REFRESH_COUNT = 5;
        int cntPump = 0;

        public SkillTreeController(AUnit unit, AView view, AModel model, AContext ctx)
            : base(unit, view, model, ctx)
        { }


        //==========================================================================
        //
        // Events.
        //
        //
        public override void Init()
        {
            if(View != null)
                View.EventOnSkillItemClicked += OnSkillBtnClicked;
            
            Events.RegisterEvent(EventID.RESOURCE_UPDATED, SkillTreeView_OnResourceUpdated);
            Events.RegisterEvent(EventID.GAME_RESET_REFRESH, OnEventRefreshView);
        }

        public override void Dispose()
        {
            if(View != null)
            View.EventOnSkillItemClicked -= OnSkillBtnClicked;

            Events.UnRegisterEvent(EventID.RESOURCE_UPDATED, SkillTreeView_OnResourceUpdated);
            Events.UnRegisterEvent(EventID.GAME_RESET_REFRESH, OnEventRefreshView);
        }        

        public override void Resume(int awayTimeInSec) 
        {
            if(context.IsSimulationMode())
            {
                STModel.WriteSimLogWhenResume(awayTimeInSec);

                SIM_LearnSkillIfPossible();
            }
        }

        public override void Pump()
        {
            if(0 == cntPump)
            {
                if(View.gameObject.activeSelf)
                    RefreshView();
                
                UpdateAffordability();
            }

            ++cntPump;
            cntPump = cntPump>=PUMP_REFRESH_COUNT ? 0 : cntPump;
        }

        public override void WriteData()
        {
            STModel.PlayerData.WriteData();
        }

        protected override void OnViewEnable() 
        {
            View.Notificator?.DisableNotification();
            RefreshView();
        }
        protected override void OnViewDisable() { }




        void RefreshView()
        {
            Assert.IsNotNull(View);          

            if(context.IsSimulationMode() || !View.gameObject.activeSelf)
                return;

            var dictCategoryPresentInfo = new Dictionary<string, SkillItemCategoryComp.PresentInfo>();

            for(int q = 0; q < STModel.skillCategories.Count; ++q)
            {
                SkillTreeCategoryInfo categoryInfo = STModel.skillCategories[q];

                var dictSkillCompPresentor = new Dictionary<string, SkillItemComp.Presentor>();
                foreach(var key in categoryInfo.DictSkillInfo.Keys)
                {
                    // Build Skill Item Presentor.
                    SkillInfo skillInfo = categoryInfo.DictSkillInfo[key];
                    SKILL_STATUS status = categoryInfo.GetSkillState(key);
                    bool isAffordable = status==SKILL_STATUS.LEARNING ? STModel.IsSkillLearnAffordable(skillInfo.Id) : false;
                    dictSkillCompPresentor.Add(key, new SkillItemComp.Presentor(skillInfo.Id, categoryInfo.GetSkillState(key), isAffordable, View.GetSkillIcon(string.Empty, skillInfo.Id)));
                }
                
                // Build Category Presentor.
                dictCategoryPresentInfo.Add( categoryInfo.Id.ToLower(), new SkillItemCategoryComp.PresentInfo(dictSkillCompPresentor));
            }

            // Build View Presentor.
            SkillItemBundleComp.PresentInfo bundleCompPresentInfo = new SkillItemBundleComp.PresentInfo(dictCategoryPresentInfo);
            View.Refresh(new SkillTreeView.PresentInfo( bundleCompPresentInfo ));
        }

        //
        //
        // LEARN / TRIGGER SKILL !!!
        //
        //
        void OnBtnLearnSkillClicked(string skill_id)
        {
            LEARN_SKILL(skill_id, payCost:true);
        }

        bool LEARN_SKILL(string skill_id, bool payCost) 
        {
            bool ret = STModel.LEARN_Skill(string.Empty, skill_id, payCost);
            if (ret)
            {
                if(false == context.IsSimulationMode())
                {
                    RefreshView();

                    var presentData = new ToastMessageDialog.PresentInfo( message :  $"Great! You learned {skill_id} skill.", duration:3.0f, ToastMessageDialog.Type.INFO);
                    context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
                        "ToastMessageDialog", presentData,
                        new Action<APopupDialog>( (popupDlg) => 
                        { 
                            Debug.Log("ToastMessage Dialog has been closed.");
                        } ) ); 
                }
            }
            
            string retString = ret ? "Learned Successfully!" : "Learning has been Failed!!!";
            string strColor = ret ? "green" : "red";
            Debug.Log($"<color={strColor}>[SIM][Action] [SkillTree] Skill {skill_id} {retString}</color>");

            return ret;
        }

        void OnBtnForceLearnSkillClicked(string skill_id)
        {
            LEARN_SKILL(skill_id, payCost:false);
        }

        // skill selected.
        void OnSkillBtnClicked(string skill_id)
        { 
            SkillTreePopupDialog.PresentInfo presentInfo = BuildSkillTreePopupPresentInfo(skill_id);
            context.RequestQuery(unitName:(string)context.GetData(KeySets.CTX_KEYS.GAME_DLG_KEY), endPoint:"DisplayPopupDialog", 
                // query result callback.
                finishCallback:(errMsg, ret) => 
                {
                    mSkillTreePopupDlgCache = (SkillTreePopupDialog)ret;
                    mSkillTreePopupDlgCache.EventOnBtnRearchClicked += OnBtnLearnSkillClicked;
                    mSkillTreePopupDlgCache.EventOnBtnForceRearchClicked += OnBtnForceLearnSkillClicked;
                },
                
                // Func-Parameter Section.
                "SkillItemStatusDialog",    // Dialog Id
                presentInfo,                // SkillTreePopupDialog.PresentInfo
                                            // Dialog close call-Back.
                new Action<APopupDialog>( (popupDlg) => 
                { 
                    Debug.Log("SkillItem Dialog has been closed.");
                    mSkillTreePopupDlgCache.EventOnBtnRearchClicked -= OnBtnLearnSkillClicked;
                    mSkillTreePopupDlgCache.EventOnBtnForceRearchClicked -= OnBtnForceLearnSkillClicked;
                    mSkillTreePopupDlgCache = null;
                } ) );    
        
        }

        
        SkillTreePopupDialog.PresentInfo BuildSkillTreePopupPresentInfo(string skill_id)
        {
            SkillTreePopupDialog.PresentInfo presentInfo = null;

            SkillInfo skillInfo = STModel.GetSkillInfo(skill_id);
            SKILL_STATUS skillState = STModel.GetSkillState(skill_id);
            Assert.IsNotNull(skillInfo, "Couldn't find skil info : " + skill_id);

            Sprite skill_icon = View.GetSkillIcon(string.Empty, skill_id);

            // Rearched.
            if (skillState == SKILL_STATUS.LEARNED || skillState == SKILL_STATUS.UNREACHABLE)
                presentInfo = new SkillTreePopupDialog.PresentInfo(skillState, skill_id, _skillName: skillInfo.Name, skill_icon, skillInfo.Description);

            // Need to collect resources. 
            else
            {
                bool isLearnable = true;
                List<ResourceStatComp.PresentInfo> listCompPresent = new List<ResourceStatComp.PresentInfo>();
                for (int q = 0; q < SkillInfo.MAX_REQIREMENTS; ++q)
                {
                    ResourceStatComp.PresentInfo statPresentInfo = null;
                    if (_buildResourceStatPresentInfo(ref statPresentInfo, skillInfo, q))
                    {
                        listCompPresent.Add(statPresentInfo);
                        isLearnable &= (statPresentInfo.CurCount>=statPresentInfo.TargetCount);
                    }

                   // else if (_buildPlanetStatPresentInfo(ref statPresentInfo, skillInfo, q))
                   //     listCompPresent.Add(statPresentInfo);
                }

                presentInfo = new SkillTreePopupDialog.PresentInfo(skillState, skill_id, skillInfo.Name, skill_icon, skillInfo.Description, isLearnable, listCompPresent);
            }

            return presentInfo;
        }

        void SkillTreeView_OnResourceUpdated(object data)
        {
#if UNITY_EDITOR
            if(context.IsSimulationMode())
                return;
#endif
            if(View.gameObject.activeSelf)
                RefreshView();
        }

        void OnEventRefreshView(object data)
        {
            View.Notificator.Reset();

            if(View.gameObject.activeSelf)
                RefreshView();
        }

        void UpdateAffordability()
        {
            // Update Notificator.
            List<SkillInfo> skill_infos = GetAffordableSkills();
            for(int i = 0; i < skill_infos.Count; ++i)             
                View.Notificator.EnableNotification(skill_infos[i].Id);                
            
            Debug.Log("[SkillTree] Checking Skill Tree's Affordability....");
        }

        void SIM_LearnSkillIfPossible()
        {
            Assert.IsTrue(context.IsSimulationMode());

            List<SkillInfo> learnableSkills = GetAffordableSkills(printLog:true);
            for(int q = 0; q < learnableSkills.Count; q++)
            {
                // LEARN_SKILL(learnableSkills[q].Id, payCost:true);
                bool learned = LEARN_SKILL(learnableSkills[q].Id, payCost:true);
                if(learned)
                    Debug.Log($"<color=green>[SIM][SkillTree] Successfully learned skill: {learnableSkills[q].Id}</color>");
            }

            // Logger.
            /*List<SkillCategoryProcInfo> procInfoList = STModel.PlayerData.GetCategoryProcInfo();
            for(int q = 0; q < procInfoList.Count; ++q)
            {
                Debug.Log($"<color=yellow>[SIM][SkillTree] Category: {procInfoList[q].CategoryId}, Learning Skills: {procInfoList[q].LearningSkillIdList.Count}</color>");
                for(int k = 0; k < procInfoList[q].LearningSkillIdList.Count; ++k)
                {
                    string skillId = procInfoList[q].LearningSkillIdList[k];
                    bool isAffordable = STModel.IsSkillLearnAffordable(skillId, isDebugMode:false, printLog:false);
                    Debug.Log($"<color=yellow>[SIM][SkillTree]   - {skillId}: Affordable={isAffordable}</color>");
                }
            }*/
            //
        }

        List<SkillInfo> GetAffordableSkills(bool printLog=false)
        {
#if UNITY_EDITOR
            Debug.Log("<color=blue>[SIM][Action] SkillTree Checking Affordability...</color>");
#endif
            // LearningSkillIdList 상태 확인
            List<SkillCategoryProcInfo> procInfoList = STModel.PlayerData.GetCategoryProcInfo();
            
            //int totalLearningSkills = 0;
            //for(int q = 0; q < procInfoList.Count; ++q)
            //    totalLearningSkills += procInfoList[q].LearningSkillIdList.Count;
            
            List<SkillInfo> learnable_skills = new List<SkillInfo>();
            for(int q = 0; q < STModel.skillCategories.Count; ++q)
            {
                SkillTreeCategoryInfo categoryInfo = STModel.skillCategories[q];

                var dictSkillCompPresentor = new Dictionary<string, SkillItemComp.Presentor>();
                foreach(var key in categoryInfo.DictSkillInfo.Keys)
                {
                    // Build Skill Item Presentor.
                    SkillInfo skillInfo = categoryInfo.DictSkillInfo[key];
                    SKILL_STATUS status = categoryInfo.GetSkillState(key);
                    
                    bool isAffordable = status==SKILL_STATUS.LEARNING ? STModel.IsSkillLearnAffordable(skillInfo.Id, isDebugMode:false, printLog) : false;
                    if(isAffordable)
                    {
                        learnable_skills.Add(skillInfo);
                        if(printLog)
                            Debug.Log($"<color=green>[SIM][SkillTree] Affordable skill found: {skillInfo.Id}</color>");
                    }
                    else if(status == SKILL_STATUS.LEARNING)
                    {
                        if(printLog)
                        {
                            Debug.Log($"<color=orange>[SIM][SkillTree] Skill not affordable yet: {skillInfo.Id}</color>");
                            STModel.IsSkillLearnAffordable(skillInfo.Id, isDebugMode:false, printLog:true);
                        }
                    }
                }
            }
    
            if(printLog)
                Debug.Log($"<color=blue>[SIM][SkillTree] Total affordable skills: {learnable_skills.Count}</color>");
    
            return learnable_skills;
        }

        bool _buildResourceStatPresentInfo(ref ResourceStatComp.PresentInfo presentInfo, SkillInfo skillInfo, int idx)
        {
            if (skillInfo!=null && skillInfo.UnlockCost != null && skillInfo.UnlockCost.Count > 0 && idx < skillInfo.UnlockCost.Count)
            {
                ResourceInfo rscInfo = null;
                context.RequestQuery("Resource", endPoint:"GetResourceInfo", (errorMsg, ret) => 
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errorMsg));
                    rscInfo = (ResourceInfo)ret;

                }, skillInfo.UnlockCost[idx].ResourceId);
                Assert.IsNotNull(rscInfo, "Couldn't find Rsc id : " + skillInfo.UnlockCost[idx].ResourceId);


                ResourceCollectInfo srcCollected = null;
                context.RequestQuery("Resource", "PlayerData.GetResourceCollectInfo", (errMsg, ret) =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                    srcCollected = (ResourceCollectInfo)ret;
                }, rscInfo.Id);

                BigInteger collectedCount = srcCollected != null ? srcCollected.BICount : BigInteger.Zero;

                ulong reqCount = (ulong)skillInfo.UnlockCost[idx].GetCount();
                Sprite iconSprite = IMContext.GetSprite(rscInfo.GetSpriteGroupId(), spriteKey:rscInfo.Id);
                presentInfo = new ResourceStatComp.PresentInfo(iconSprite, rscInfo.Id, rscInfo.GetClassKey(), reqCount, collectedCount);

                return true;
            }
            return false;
        }

    }
}
