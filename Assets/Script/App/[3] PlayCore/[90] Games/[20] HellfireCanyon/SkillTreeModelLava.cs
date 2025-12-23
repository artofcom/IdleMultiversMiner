using App.GamePlay.IdleMiner.Common;
using App.GamePlay.IdleMiner.SkillTree;
using IGCore.MVCS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SkillTreeModelLava : SkillTreeModel
{
    public SkillTreeModelLava(AContext ctx, APlayerModel playerData) : base(ctx, playerData) 
    { }


    protected override void LoadData(string gamePath)
    {
        Debug.Log("[InitSeq]:[SkillTreeModel] InitSkillTreeData...");
        skillCategories.Clear();

        string[] skillFiles = new string[]{"Skill_Mining" , "Skill_CompCraft", "Skill_ItemCraft" , "Skill_Goal" };

        //string[] skillFiles = new string[]{"Skill_Mining", "Skill_ItemCraft" };//, "Skill_Goal" };

        for(int cat = 0; cat < skillFiles.Length; cat++) 
        {
            string filePath = gamePath + $"/Data/{skillFiles[cat]}";
            var jsonData = Resources.Load<TextAsset>(filePath);
            Assert.IsTrue(jsonData != null);
            SkillTreeCategoryInfo categoryInfo = JsonUtility.FromJson<SkillTreeCategoryInfo>(jsonData.text);
            Assert.IsTrue(categoryInfo != null);

            skillCategories.Add(categoryInfo);
        }
    }
}
