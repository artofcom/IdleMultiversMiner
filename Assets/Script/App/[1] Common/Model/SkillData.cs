
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.Common
{
    public enum SKILL_STATUS { LEARNING, LEARNED, UNREACHABLE, UNKNOWN };
    /*public enum SKILL_ABILITY { 
        UNLOCK_ZONE, 
        UNLOCK_FEATURE, 
        CRAFT_BUFF,
        MULTI_CRAFT_BUFF,
        CRAFT_SKIP_COST,
        MINING_ZONE_BUFF,
        GAME_RESET,
        //
        // Add ability ids as needed.
    };*/

    public interface ISkillLeaner
    {
        void CreateSkillBehaviors();
        void LearnSkill(string skill_id, string ability_id, string ability_param);
    }

    public interface ISkillBehavior
    {
        void Learn(AController ctrler, string ability_param);
    }


    // Baseline of design. => Flexibility
    // - Should add/remove skill-info object at any updates. 
    // - Should switch/replace object at any updates.
    // - Should have multiple source/output objects
    // 
    [Serializable]
    public class SkillInfo
    {
        public bool IsValid() { return string.IsNullOrEmpty(id)==false; }

        public const int MAX_REQIREMENTS = 3;
        public const string EON = "EndOfNode";

        [SerializeField] string name;
        [SerializeField] string id;
        [SerializeField] string abilityId;      // this id is recognizable from code (SkillTreeController) for now.
        [SerializeField] string abilityParam;   // - usually numbers : if param has '." then should be float, others should be int.
        [SerializeField] string description;

        [SerializeField] List<ResourceRequirement> unlockCost = null;

        [SerializeField] List<string> children;

        // Accessor.
        public string Id => id;
        public string Name => name;
        public string AbilityID => abilityId;
        public string AbilityParam => abilityParam;
        public string Description => description;
        public List<ResourceRequirement> UnlockCost => unlockCost;
        public List<string> Children => children;

#if UNITY_EDITOR
        public SkillInfo(string id, string name, string ability_id, string ability_param, string desc, List<ResourceRequirement> reqResources, List<string> children)
        {
            this.id = id;
            this.name = name;           this.abilityId = ability_id;    this.abilityParam = ability_param;
            this.description = desc;    this.unlockCost = reqResources;
            this.children = children;
        }
        public void SetId(string id) { this.id = id; }
        public void SetUnlockCostList(List<ResourceRequirement> reqResources) { unlockCost = reqResources; }
#endif


        // Runtime Value.
        // public Sprite Icon { get; set; } = null;
    }

/*
    [Serializable]
    public class SkillCategory
    {
        [SerializeField] string categoryId;
        [SerializeField] List<SkillInfo> skillInfo; // All Skills in the Path.

        /*public SkillPath(List<SkillInfo> skillInfo)
        {
            this.skillInfo = skillInfo;
            for (int q = 0; q < skillInfo.Count; q++)
                skillInfo[q].Index = q;
        }

        // Accessor.
        public string CategoryId => categoryId;
        public List<SkillInfo> SkillInfos => skillInfo;

        public void Convert()
        {
            /*skillInfo.Sort((a, b) =>
            {
                if(a.Id > b.Id)         return 1;
                else if(a.Id < b.Id)    return -1;
                return 0;
            });

            categoryId = categoryId.ToLower();

#if UNITY_EDITOR
            Debug.Log("Skill Category : " + categoryId);
            for(int q = 0; q < skillInfo.Count; q++) 
                Debug.Log($"Skill Data : {skillInfo[q].Id}-{skillInfo[q].Name}");
#endif
        }

        public SkillInfo GetSkillInfo(uint skillId)
        {
            Assert.IsNotNull(skillInfo);

            for(int q = 0; q < skillInfo.Count; ++q)
            {
              //  if(skillInfo[q].Id == skillId)
              //      return skillInfo[q];
            }
            
            return null;
        }

#if UNITY_EDITOR
        public void SetCategoryId(string categoryId)
        {
            this.categoryId = categoryId;
        }
        public void AddSkillInfo(SkillInfo info)
        {
            if(skillInfo == null)
                skillInfo = new List<SkillInfo>();

            skillInfo.Add(info);
        }
#endif

    }*/

    
    [Serializable]
    public class SkillTreeCategoryInfo
    {
        [SerializeField] string id;                     // category id.
        [SerializeField] string rootId;                 // root skill info id.
        [SerializeField] List<SkillInfo> skillInfoPool = new List<SkillInfo>();

        public string RootId => rootId;
        public string Id => id;

#if UNITY_EDITOR
        public void SetId(string id) { this.id = id; }
        public void SetRootId(string rootId) { this.rootId = rootId; }
        public void SetSkillInfoPool(List<SkillInfo> skillInfos) { this.skillInfoPool = skillInfos; }
        public List<SkillInfo> GetSkillInfoPool() { return skillInfoPool; }
#endif

        Dictionary<string, SkillInfo> dictSkillInfo = new Dictionary<string, SkillInfo>();
        Dictionary<string, SKILL_STATUS> dictStatusCache = new Dictionary<string, SKILL_STATUS>();

        public Dictionary<string, SkillInfo> DictSkillInfo
        {
            get
            {
                if(dictSkillInfo.Count == 0)
                {
                    for(int q = 0; q < skillInfoPool.Count; q++)
                        dictSkillInfo.Add(skillInfoPool[q].Id.ToLower(), skillInfoPool[q]);
                }
                return dictSkillInfo;
            }
        }

        public SKILL_STATUS GetSkillState(string skill_id)
        {
            skill_id = skill_id.ToLower();
            if(dictStatusCache.Count == 0)
            {
                Debug.LogWarning($"Status-dictionary is not ready...{id} / {skill_id}");
                return SKILL_STATUS.UNKNOWN;
            }
            if(dictStatusCache.ContainsKey(skill_id))
                return dictStatusCache[skill_id];

            return SKILL_STATUS.UNKNOWN;
        }
        public SkillInfo GetSkillInfo(string skill_id) 
        {
            skill_id = skill_id.ToLower();
            if(!DictSkillInfo.ContainsKey(skill_id))
                return null;

            return DictSkillInfo[skill_id];
        }
        
        public void RebuildStatusBuffer(List<string> workingskill_id)
        {
            dictStatusCache.Clear();

            var allPaths = new List<List<string>>();
            collectPaths(this.rootId, new List<string>(), allPaths);

            foreach(var path in allPaths) 
            {
                bool foundTarget = false;
                for(int q = 0; q < workingskill_id.Count; q++)
                {
                    string target_skill_id = workingskill_id[q].ToLower();

                    int index = path.FindIndex(node_skill_id => string.Equals(node_skill_id, target_skill_id, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        // 경로상 target 전까지 0
                        for (int i = 0; i < index; i++)
                            dictStatusCache[ path[i] ] = SKILL_STATUS.LEARNED;

                        // target 노드 1
                        dictStatusCache[ path[index] ] = SKILL_STATUS.LEARNING;

                        // target 아래 노드들 2
                        for (int i = index + 1; i < path.Count; i++)
                            dictStatusCache[ path[i] ] = SKILL_STATUS.UNREACHABLE;

                        foundTarget = true;
                        break;
                    }
                }

                if(!foundTarget)
                {
                    foreach (var skill_id in path)
                        dictStatusCache[ skill_id ] = SKILL_STATUS.LEARNED;
                }
            }
        }

        void collectPaths(string cur_skill_id, List<string> path, List<List<string>> allPaths)
        {
            cur_skill_id = cur_skill_id.ToLower();

            SkillInfo skillInfo = GetSkillInfo(cur_skill_id);
            Assert.IsNotNull(skillInfo, "Couldn't find skill_id : " + cur_skill_id);
            if(skillInfo == null)   return;

            path.Add(cur_skill_id);

            if(skillInfo.Children==null || skillInfo.Children.Count==0)
            {
                // Reaching leaf → saveing path
                allPaths.Add(new List<string>(path));
            }
            else
            {
                foreach (var child_id in skillInfo.Children)
                    collectPaths(child_id, path, allPaths);
            }

            path.RemoveAt(path.Count - 1);
        }

        public void Dispose()
        {
            skillInfoPool?.Clear();
        }
    }
}
