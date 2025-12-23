using App.GamePlay.IdleMiner.Common;
using App.GamePlay.IdleMiner.Common.Model;
using Core.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class SkillItemBundleComp : IGCore.MVCS.AView
    {
        // Hold Roots of trees of categories.
        [SerializeField] List<SkillItemCategoryComp> categoryComp;

        public List<SkillItemCategoryComp> CategoryComp => categoryComp;

        
        public class PresentInfo : APresentor
        {
            public PresentInfo(Dictionary<string, SkillItemCategoryComp.PresentInfo> dictSkillCategoryPresentsInfo)
            {
                this.DictSkillCategoryPresentInfo = dictSkillCategoryPresentsInfo;
            }
            public Dictionary<string, SkillItemCategoryComp.PresentInfo> DictSkillCategoryPresentInfo { get; private set; }
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }
        
        override public void Refresh(APresentor presentor)
        {
            PresentInfo info = presentor as PresentInfo;
            if(info == null)    return;

            for(int q = 0; q < categoryComp.Count; ++q)
            {
                SkillItemCategoryComp categoryInfo = categoryComp[q];

                Assert.IsTrue( info.DictSkillCategoryPresentInfo.ContainsKey(categoryInfo.CategoryId.ToLower()), $"{categoryInfo.CategoryId} is missing!" );
                
                categoryInfo.Refresh( info.DictSkillCategoryPresentInfo[ categoryInfo.CategoryId.ToLower() ] );
            }
        }

        public Sprite GetSkillIcon(string category_id,  string skill_id)
        {
            if(!string.IsNullOrEmpty(category_id))
            {
                category_id = category_id.ToLower();
                for(int q = 0; q < CategoryComp.Count; ++q)
                {
                    if(categoryComp[q].CategoryId.ToLower() == category_id)
                        return categoryComp[q].GetSkillIcon(skill_id);
                }
            }
            else
            {
                for(int q = 0; q < CategoryComp.Count; ++q)
                {
                    Sprite ret = categoryComp[q].GetSkillIcon(skill_id);
                    if(ret != null)     return ret;
                }
            }

            return null;
        }


#if UNITY_EDITOR
        public void ExportSkillData()
        {
            if(categoryComp==null || categoryComp.Count == 0) return;

            List<SkillTreeCategoryInfo> categoryInfos = new List<SkillTreeCategoryInfo>();

            for(int q = 0; q < categoryComp.Count; ++q)
            {
                SkillTreeCategoryInfo categoryInfo = new SkillTreeCategoryInfo();
                categoryInfo.SetId( categoryComp[q].CategoryId );
                categoryInfo.SetRootId( CategoryComp[q].RootNode.SkillData.Id );

                // Build Data Pool. 
                List<SkillInfo> skillInfoList = new List<SkillInfo>();
                for(int ch = 0; ch < categoryComp[q].transform.childCount; ++ch)
                {
                    Transform trChild = categoryComp[q].transform.GetChild(ch);

                    if(!trChild.gameObject.activeSelf)
                        continue;

                    SkillItemComp skillItemComp = trChild.GetComponent<SkillItemComp>();
                    if(skillItemComp == null || skillItemComp.SkillData==null || string.IsNullOrEmpty(skillItemComp.SkillData.Id))
                        continue;

                    Assert.IsNotNull(skillItemComp.SkillData.UnlockCost);

                    SkillInfo editorData = skillItemComp.SkillData;
                    SkillInfo skillInfo = new SkillInfo(editorData.Id, editorData.Name, editorData.AbilityID, editorData.AbilityParam, editorData.Description, editorData.UnlockCost, editorData.Children);
                    skillInfoList.Add(skillInfo);
                }
                categoryInfo.SetSkillInfoPool(skillInfoList);

                categoryInfos.Add(categoryInfo);
            }



            // Finally Export category to each json file.
            const string DATA_SUB_PATH = "/EditorData/";
            for(int q = 0; q < categoryInfos.Count; ++q) 
            {
                string content = JsonUtility.ToJson(categoryInfos[q], prettyPrint:true);
                TextFileIO.WriteTextFile(Application.dataPath +  DATA_SUB_PATH + $"Skill_{categoryInfos[q].Id}.json", content);
            }
        }
#endif
    }
}
