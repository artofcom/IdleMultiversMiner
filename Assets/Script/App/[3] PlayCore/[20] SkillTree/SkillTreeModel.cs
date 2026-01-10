
using App.GamePlay.IdleMiner.Common;
using App.GamePlay.IdleMiner.Common.PlayerModel;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using Core.Util;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class SkillTreeModel : AModel
    {
        EventsGroup Events = new EventsGroup();

        public List<SkillTreeCategoryInfo> skillCategories { get; private set; } = new List<SkillTreeCategoryInfo>();

        public SkillTreePlayerModel PlayerData => playerData as SkillTreePlayerModel;

        public SkillTreeModel(AContext ctx, APlayerModel playerData) : base(ctx, playerData) 
        { }
           
        public override void Init(object data = null)
        {
            base.Init(data);    

            IdleMinerContext IMCtx = (IdleMinerContext)context;
            Assert.IsNotNull(IMCtx);
            _InitModel();

            Events.RegisterEvent(EventID.SKILL_RESET_GAME_INIT, ResetGamePlay_InitGame);
        }

        protected virtual void _InitModel()
        {
            IdleMinerContext IMCtx = (IdleMinerContext)context;
            Assert.IsNotNull(IMCtx);

            string gamePath = (string)IMCtx.GetData("gamePath");
            this.LoadData(gamePath);

            SanatizeData();
            
            InitSkillStatusBuffer();

            RegisterRequestables();

            _isInitialized = true;
        }

        void ResetGamePlay_InitGame(object data)
        {
            PlayerData.ResetGamePlay_InitGame(caller:this);

            SanatizeData();
            InitSkillStatusBuffer();
        }


        protected virtual void LoadData(string gamePath)
        {
            Assert.IsTrue(false, "[SkillTree] Should Load Skill Category Data for each game Properly! " + this.GetType().Name);
        }

        void SanatizeData()
        {
            for(int cat = 0; cat < skillCategories.Count; cat++) 
                sanatizeWorkingSkill(skillCategories[cat].Id);
                
            Debug.Log("[InitSeq]:[SkillTree] Validate Skils..");
        }


        public override void Dispose()
        {
            base.Dispose();

            // de-init model.
            if(skillCategories != null)
            {
                for(int q = 0; q < skillCategories.Count; q++)
                    skillCategories[q].Dispose();
                skillCategories?.Clear();
            }

            UnregisterRequestables();

            Events.UnRegisterEvent(EventID.SKILL_RESET_GAME_INIT, ResetGamePlay_InitGame);

            _isInitialized = false;
        }

        void InitSkillStatusBuffer()
        {
            for(int q = 0; q < skillCategories.Count; ++q)
            {
                SkillTreeCategoryInfo categoryInfo = skillCategories[q];
                List<string> workingSkills = PlayerData.GetWorkingSkillId(categoryInfo.Id);
                categoryInfo.RebuildStatusBuffer( workingSkills );

                string ws = workingSkills.Count>0 ? workingSkills[0] : "FIN";
                Debug.Log($"[InitSeq]:[SkillTree] Status Buffer Created...{categoryInfo.Id} / WS [{ws}]");
            }
        }


        public SkillInfo GetSkillInfo(string skillId)
        {
            // O(N : skill category count : usually 3 ~ 5)
            for(int q = 0; q < skillCategories.Count; q++)
            {
                SkillTreeCategoryInfo categoryInfo = skillCategories[q];
                SkillInfo ret = categoryInfo.GetSkillInfo(skillId); // O(1)
                if(ret != null) return ret;
            }

            return null;
        }

        public SKILL_STATUS GetSkillState(string skillId) 
        {
            for(int q = 0; q < skillCategories.Count; q++)
            {
                SkillTreeCategoryInfo categoryInfo = skillCategories[q];
                if(categoryInfo.GetSkillInfo(skillId) != null)
                    return categoryInfo.GetSkillState(skillId);
            }

            return SKILL_STATUS.UNKNOWN;
        }



        SkillTreeCategoryInfo GetSkillCategoryInfo(string categoryId)
        {
            categoryId = categoryId.ToLower();
            
            for(int q = 0; q < skillCategories.Count; q++)
            {
                if(skillCategories[q].Id.ToLower() == categoryId)
                    return skillCategories[q];
            }
            return null;
        }

        public string GetCategoryIdFromSkillId(string skillId)
        {
            for(int q = 0; q < skillCategories.Count; q++)
            {
                SkillTreeCategoryInfo categoryInfo = skillCategories[q];
                SkillInfo ret = categoryInfo.GetSkillInfo(skillId);
                if(ret != null) return categoryInfo.Id;
            }
            return string.Empty;
        }

        public bool IsSkillLearnAffordable(string skill_id, bool isDebugMode=false, bool printLog=false)
        {
            SkillInfo selectedInfo = GetSkillInfo(skill_id);
            if(selectedInfo == null)
                return false;
            
            bool ret = true;
            for (int q = 0; q < selectedInfo.UnlockCost?.Count; q++)
            {
                ResourceCollectInfo srcCollected = null;
                context.RequestQuery("Resource", "PlayerData.GetResourceCollectInfo", (errMsg, ret) =>
                {
                    if(!string.IsNullOrEmpty(errMsg))
                        Debug.Log($"<color=yellow>Request Query has been failed...[{errMsg}]</color>");
                    srcCollected = ret as ResourceCollectInfo;
                }, selectedInfo.UnlockCost[q].ResourceId);

                if(srcCollected == null)
                    continue;

                BigInteger reqCount = isDebugMode ? 1 : selectedInfo.UnlockCost[q].GetCount();

                if(printLog)
                {
                    string strCount = srcCollected==null ? "0" : srcCollected.BICount.ToString();
                    Debug.Log($"[SIM][Status] [SkillTree-LearnAffordable] Skill_id[{skill_id}], Rsc:[{selectedInfo.UnlockCost[q].ResourceId}], {strCount}/{selectedInfo.UnlockCost[q].GetCount()}");
                }

                // Un-affordable !!!
                if(srcCollected==null || srcCollected.BICount < reqCount)
                {
                    ret = false;        // let the loop finish.
                    //if(printLog)
                        //string strCount = srcCollected==null ? "0" : srcCollected.BICount.ToString();
                    
                }
            }
            return ret;
        }

        public bool LEARN_Skill(string categoryId, string skill_id, bool payCost = true, bool isPartOfInitProcess = false)
        {
            skill_id = skill_id.ToLower();

            // Search Skill-Info Data.
            if(string.IsNullOrEmpty(categoryId))
            {
                categoryId = GetCategoryIdFromSkillId(skill_id);
                if(string.IsNullOrEmpty (categoryId))
                {
                    Assert.IsTrue(false, "Couldn't find category id..." + skill_id);
                    return false;
                }
            }
            SkillInfo selectedInfo = GetSkillInfo(skill_id);
            Assert.IsNotNull(selectedInfo);
            
            // Affordability Check.
            if (payCost)
            {
                if(false == IsSkillLearnAffordable(skill_id, isDebugMode:false))
                {
                    Debug.Log("Learning Skill has been failed due to the shortage of the resource count.");
                    return false;
                }

                // Consume Cost.
                for(int q = 0; q < selectedInfo.UnlockCost.Count; q++)
                {
                    BigInteger reqCount = selectedInfo.UnlockCost[q].GetCount();
                    context.RequestQuery("Resource", "PlayerData.UpdateResourceX1000", (errMsg, ret) => {}, selectedInfo.UnlockCost[q].ResourceId, -1000 * reqCount, true);
                }
            }
            
            // Debug.Log("Learning Skills - " + skill_id);
            EventSystem.DispatchEvent(EventID.SKILL_LEARNED, new Tuple<string, string, string, bool>(skill_id, selectedInfo.AbilityID, selectedInfo.AbilityParam, isPartOfInitProcess));

            if(!isPartOfInitProcess)
            {
                PlayerData.SetWorkingSkill(categoryId, skill_id, selectedInfo.Children);
                SkillTreeCategoryInfo cateInfo = GetSkillCategoryInfo(categoryId);
                Assert.IsNotNull(cateInfo);
                cateInfo.RebuildStatusBuffer( PlayerData.GetWorkingSkillId(categoryId) );

                if(context.IsSimulationMode())
                {
                    if(selectedInfo.Children == null ||  selectedInfo.Children.Count == 0)
                        Debug.Log($"<color=green>[SIM][SkillTree] Finished learning all Skills in the [{categoryId}] catetory !!! lastSkill [{skill_id}] </color>");
                }
            }
                            
            return true;
        }

        // Player Model wrapper.
        public List<string> GetWorkingSkillId(string category)
        {
            return PlayerData.GetWorkingSkillId(category);
        }

       
        void sanatizeWorkingSkill(string category)
        {    
            SkillTreeCategoryInfo skill_category = GetSkillCategoryInfo(category);
                
            Assert.IsTrue(skill_category != null, $"[{category}] data should not be null!");
            Assert.IsTrue(!string.IsNullOrEmpty(skill_category.RootId), "Invalid Skill Info...!" );
            
            List<string> workingSkill_id = PlayerData.GetWorkingSkillId(category);
            if(workingSkill_id==null || workingSkill_id.Count==0)
                PlayerData.SetWorkingSkill(category, string.Empty, new List<string>{ skill_category.RootId });

            // Make sure resolving all opened skills so far. 
            workingSkill_id = PlayerData.GetWorkingSkillId(category);
            Assert.IsTrue(workingSkill_id!=null && workingSkill_id.Count>0, "Working Skill Id can NOT be empty !!!");

            InitLearnedSkill(skill_category, workingSkill_id);
        }

        void InitLearnedSkill(SkillTreeCategoryInfo categoryInfo, List<string> working_skill_id)
        {
            // DFS.
            _initLearnedSkill(categoryInfo, categoryInfo.RootId.ToLower(), working_skill_id);
        }

        void _initLearnedSkill(SkillTreeCategoryInfo categoryInfo, string skill_id, List<string> working_skill_id)
        {
            SkillInfo skillInfo = categoryInfo.GetSkillInfo(skill_id);
            Assert.IsNotNull(skillInfo, $"Couldn't find skill_id : {categoryInfo.Id}, {skill_id}");

            bool thisIsWorkingSkill = false;
            for(int q = 0; q < working_skill_id.Count; ++q)
            {
                if(working_skill_id[q] == skill_id)
                {
                    thisIsWorkingSkill = true;
                    break;
                }
            }
            if(thisIsWorkingSkill)      return;     // No need to recover any further.

            
            // Learn Skill.
            //
            LEARN_Skill(categoryInfo.Id, skill_id, payCost:false, isPartOfInitProcess:true);
            //
            //

            for(int q = 0; q < skillInfo.Children.Count; ++q)
            {
                _initLearnedSkill(categoryInfo, skillInfo.Children[q].ToLower(), working_skill_id);
            }
        } 

        public void WriteSimLogWhenResume(float duration)
        {
            // 
            PlayerData.WriteSimLogWhenResume(duration);
        }



        #region Simulator Related.

#if UNITY_EDITOR

        // Get Cost-Resource Ids for the skill node.
        Dictionary<string, int> SIM_GetRequiredResources(string skill_id)
        {
            if(string.IsNullOrEmpty(skill_id))
                return SIM_GetRequiredResourcesForAllWorkingSkills();
            
            var skillInfo = GetSkillInfo(skill_id);
            if(skillInfo == null)   
                return SIM_GetRequiredResourcesForAllWorkingSkills();

            var costs = new Dictionary<string, int>();
            for(int q = 0; q < skillInfo.UnlockCost.Count; ++q)
            {
                if(false == costs.ContainsKey(skillInfo.UnlockCost[q].ResourceId.ToLower()))
                    costs.Add( skillInfo.UnlockCost[q].ResourceId.ToLower(), 1);
                else 
                    ++costs[ skillInfo.UnlockCost[q].ResourceId.ToLower()];
            }

            return costs;
        }

        // Get the whole cost-resource ids for the all workign skill nodes. 
        Dictionary<string, int> SIM_GetRequiredResourcesForAllWorkingSkills()
        {
            var costs = new Dictionary<string, int>();
            for(int c = 0; c < skillCategories.Count; c++)
            {
                var skillCategory = skillCategories[c];
                List<string> working_skill_id = GetWorkingSkillId(skillCategory.Id);
                if(working_skill_id == null)
                    continue;

                for(int w = 0; w < working_skill_id.Count; ++w)
                {
                    var skill_info = skillCategory.GetSkillInfo(working_skill_id[w]);
                    if(skill_info == null)
                    {
                        Debug.LogWarning("[SIM][SkillTree] Couldn't find the working skil info.." + working_skill_id[w]);
                        continue;
                    }
                    string strRscList = string.Empty;
                    for(int q = 0; q < skill_info.UnlockCost.Count; ++q)
                    {
                        string srcId = skill_info.UnlockCost[q].ResourceId.ToLower();
                        BigInteger requiredAmount = skill_info.UnlockCost[q].GetCount();

                        if(false == costs.ContainsKey(srcId))  
                            costs.Add( srcId, (int)requiredAmount);
                        else 
                            costs[ srcId ] += (int)requiredAmount;

                        strRscList += srcId + ", ";
                    }
                    Debug.Log($"[SIM][SkillTree] Collecting ReqRscs... category [{skillCategory.Id}] node [{skill_info.Id}], reqRscId [{strRscList}]");
                }
            }
            return costs;
        }
     
        List<SkillInfo> SIM_CollectAllWorkingNodes()
        {
            List<SkillInfo> learnableSkills = new List<SkillInfo>();    
            List<SkillCategoryProcInfo> categoryList = PlayerData.GetCategoryProcInfo();
            for(int q = 0; q < categoryList.Count; ++q)
            {   
                for(int k = 0; k < categoryList[q].LearningSkillIdList.Count; ++k)
                {
                    string skillId = categoryList[q].LearningSkillIdList[k];
                    SkillInfo skillInfo = GetSkillInfo(skillId);
                    if(skillInfo != null)
                        learnableSkills.Add(skillInfo);
                    else
                    {
                        // This means the path of the category has been finished.
                        Debug.Log($"[SkillTree] - WARNING: SkillInfo not found for: {skillId}, Maybe Finished ? ");
                    }
                }
            }
            return learnableSkills;
        }

#endif

        #endregion




        void RegisterRequestables()
        {
#if UNITY_EDITOR
            context.AddRequestDelegate("SkillTree", "SIM_GetRequiredResources", sim_getRequiredResources);
            context.AddRequestDelegate("SkillTree", "SIM_CollectAllWorkingNodes", sim_collectAllWorkingNodes);
#endif
        }

        void UnregisterRequestables() 
        {
#if UNITY_EDITOR
            context.RemoveRequestDelegate("SkillTree", "SIM_GetRequiredResources");
            context.RemoveRequestDelegate("SkillTree", "SIM_CollectAllWorkingNodes");
#endif
        }

#if UNITY_EDITOR

        object sim_getRequiredResources(params object[] data)
        {
            if(data==null || data.Length<1)
                return null;

            return SIM_GetRequiredResources((string)data[0]);
        }
        /*
        object collectReqResourcesFromSkillTree(params object[] data) 
        {
            CollectReqResourcesFromSkillTree();
            return null;
        }*/
        object sim_collectAllWorkingNodes(params object[] data)
        {
            return SIM_CollectAllWorkingNodes();
        }
#endif
    }
}

