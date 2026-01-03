
using App.GamePlay.IdleMiner.Common;
using Core.Events;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Types;

namespace App.GamePlay.IdleMiner.SkillTree
{
    [Serializable]
    public class SkillCategoryProcInfo
    {
        [SerializeField] string categoryId;
        [SerializeField] List<string> learningSkillIdList;

        public SkillCategoryProcInfo(string category)
        {
            this.categoryId = category;
        }

        public string CategoryId => categoryId;
        public List<string> LearningSkillIdList => learningSkillIdList;
        //public bool IsLearned => isLearned;

        public void Update(string oldLearningIdToRemove, List<string> newlearningSkillId)
        {
            if(learningSkillIdList == null)
                learningSkillIdList = new List<string>();
            
            oldLearningIdToRemove = oldLearningIdToRemove.ToLower();

            // Copy Data to Set.
            HashSet<string> setLearningSkillId = new HashSet<string>();
            for(int q = 0; q < learningSkillIdList.Count; q++)
            {
                string skill_id = learningSkillIdList[q].ToLower();
                if(!setLearningSkillId.Contains(skill_id))
                    setLearningSkillId.Add(skill_id);
            }

            // Remove Old Skill Node.
            if(setLearningSkillId.Contains(oldLearningIdToRemove))
                setLearningSkillId.Remove(oldLearningIdToRemove);

            // Add New Skill Ids.
            if(newlearningSkillId==null || newlearningSkillId.Count==0)
                setLearningSkillId.Add(SkillInfo.EON);  // End Of Node.
            
            else
            {
                for (int q = 0; q < newlearningSkillId.Count; ++q)
                {
                    string skill_id = newlearningSkillId[q].ToLower();
                    if (!setLearningSkillId.Contains(skill_id))
                        setLearningSkillId.Add(skill_id);
                }
            }

            learningSkillIdList.Clear();
            foreach(string learning_skill_id  in setLearningSkillId)
                learningSkillIdList.Add(learning_skill_id);
        }
    }

    [Serializable]
    public class SkillTreeProcInfo
    {
        [SerializeField] List<SkillCategoryProcInfo> categoryProcInfo = new List<SkillCategoryProcInfo>(); // count == SkillPath.eType.MAX
        
        // Accessor.
        public List<SkillCategoryProcInfo> CategoryProcInfo      {   get => categoryProcInfo;   set => categoryProcInfo = value;   }
        
    }

    public class SkillTreePlayerModel : MultiGatewayWritablePlayerModel
    {        
        public static readonly string EVENT_ON_SKILLMINING_BUFF_UPDATE = "OnSkillMiningBuffUpdated"; 

        // Serialize Fields.
        SkillTreeProcInfo skillTreeProcInfo = new SkillTreeProcInfo();
       // SkillAbilityInfo skillAbilityInfo = new SkillAbilityInfo();

        // Accessor.
        public SkillTreeProcInfo SkillTreeProcInfo => skillTreeProcInfo;
      //  public SkillAbilityInfo SkillAbilityInfo => skillAbilityInfo;


        EventsGroup Events = new EventsGroup();

        public SkillTreePlayerModel(AContext ctx, List<IDataGatewayService> gatewayService) : base(ctx, gatewayService)  { }

        static string DataKey_SkillTree => $"{nameof(SkillTreePlayerModel)}_SkillTreeData";

        public override void Init()
        {
            base.Init();

            LoadSkillTreeData();

            IsInitialized = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            IsInitialized = false;
        }

        public void WriteData()
        {
         //   SaveSkillTreeData();
        }        

        //public void SaveSkillTreeData()
        //{
            //if(skillTreeProcInfo != null)
            //    WriteFileInternal(SkillTreePlayDataFile, skillTreeProcInfo);

          //  if(skillAbilityInfo != null)
          //      WriteFileInternal($"{mAccount}_SkillAbilityData", skillAbilityInfo);
        //}
        void LoadSkillTreeData()
        {
            if(context.IsSimulationMode())
                return;
            
            int idxGatewayService = (context as IdleMinerContext).ValidGatewayServiceIndex;
            FetchData(idxGatewayService, DataKey_SkillTree, out skillTreeProcInfo, fallback:new SkillTreeProcInfo());

//            ReadFileInternal<SkillAbilityInfo>($"{mAccount}_SkillAbilityData", ref skillAbilityInfo);
        }


        //==========================================================================
        //
        // SkillTree Processor.
        //
        //
        SkillCategoryProcInfo GetCategoryProcInfo(string category)
        {
            for(int q = 0; q < skillTreeProcInfo.CategoryProcInfo.Count; ++q)
            {
                if(0 == string.Compare(skillTreeProcInfo.CategoryProcInfo[q].CategoryId, category, StringComparison.OrdinalIgnoreCase))
                    return skillTreeProcInfo.CategoryProcInfo[q];
            }
            return null;
        }
        public List<SkillCategoryProcInfo> GetCategoryProcInfo()
        {
            return skillTreeProcInfo.CategoryProcInfo;
        }
        public List<string> GetWorkingSkillId(string category)
        {
            var categoryProcInfo = GetCategoryProcInfo(category);
            if(categoryProcInfo != null)
                return categoryProcInfo.LearningSkillIdList;
            
            return null;
        }
        public bool SetWorkingSkill(string category, string oldWorkingSkill, List<string> workingSkills, bool isLearned=false)
        {
            category = category.ToLower();
            var categoryProcInfo = GetCategoryProcInfo(category);
            if(categoryProcInfo == null)
            {
                categoryProcInfo = new SkillCategoryProcInfo(category);
                skillTreeProcInfo.CategoryProcInfo.Add(categoryProcInfo);
            }

            categoryProcInfo.Update(oldWorkingSkill, workingSkills);//, isLearned);
            return true;
        }


        public void WriteSimLogWhenResume(float duration)
        {
            Assert.IsTrue(context.IsSimulationMode());
            
            // Model._InitModel should have intialized stuff already.
            Assert.IsTrue(skillTreeProcInfo!=null && skillTreeProcInfo.CategoryProcInfo!=null && skillTreeProcInfo.CategoryProcInfo.Count>0);

            for(int q = 0; q < skillTreeProcInfo.CategoryProcInfo.Count; ++q)
            {
                var categoryProc = skillTreeProcInfo.CategoryProcInfo[q];
                for(int z = 0; z < categoryProc.LearningSkillIdList.Count; ++z) 
                {
                    Debug.Log($"[SIM][Status] [WorkingSkill] CategoryId:[{categoryProc.CategoryId}], SkillId:[{categoryProc.LearningSkillIdList[z]}]");
                }
            }
        }

        public void ResetGamePlay_InitGame(AModel caller)
        {
            Assert.IsNotNull(caller as SkillTreeModel);

            LoadSkillTreeData();
        }

        #region IWritableModel

        public override List<Tuple<string, string>> GetSaveDataWithKeys()
        {
            List<Tuple<string, string>> listDataSet = new List<Tuple<string, string>>();
            
            Assert.IsNotNull(skillTreeProcInfo);
            listDataSet.Add(new Tuple<string, string>(DataKey_SkillTree, JsonUtility.ToJson(skillTreeProcInfo)));
            
            return listDataSet;
        }
        
        #endregion

#if UNITY_EDITOR
        //==========================================================================
        //
        // Editor - Reset Data Prefab
        //
        [UnityEditor.MenuItem("PlasticGames/Clear PlayerData/SkillTree")]
        public static void EditorClearSkillTreeData()
        {
            // ClearSkillTreeData();
        }
#endif
    }
}
