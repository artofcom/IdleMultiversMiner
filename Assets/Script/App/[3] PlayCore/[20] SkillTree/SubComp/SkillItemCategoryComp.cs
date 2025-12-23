using App.GamePlay.IdleMiner.GamePlay;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class SkillItemCategoryComp : IGCore.MVCS.AView
    {
        [SerializeField] string categoryId;
        [SerializeField] SkillItemComp root;
        [SerializeField] int defaultUnlockCount = 10;

        [Header("[(1) Resource Section ]")]
        [SerializeField] ResourceDataSetting materialSet;
        [SerializeField] ResourceDataSetting componentSet;
        [SerializeField] ResourceDataSetting itemSet;

        [SerializeField] PlanetControllerComp planetController;

        public string CategoryId => categoryId;
        public SkillItemComp RootNode => root;
        public ResourceDataSetting MaterialSet => materialSet;
        public ResourceDataSetting ComponentSet => componentSet;
        public ResourceDataSetting ItemSet => itemSet;
        public PlanetControllerComp PlanetController => planetController;

        public class PresentInfo : APresentor
        {
            public PresentInfo(Dictionary<string, SkillItemComp.Presentor> skillCompPresentsInfo)
            {
                this.DictSkillCompPresentInfo = skillCompPresentsInfo;
            }
            public Dictionary<string, SkillItemComp.Presentor> DictSkillCompPresentInfo { get; private set; }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(categoryId));
            Assert.IsNotNull(root);
        }

        public override void Refresh(APresentor presentor)
        {
            PresentInfo info = presentor as PresentInfo;
            if(info == null)    return;

            Queue<SkillItemComp> queueItemComp = new Queue<SkillItemComp>();
            queueItemComp.Enqueue(root);
            while(queueItemComp.Count > 0) 
            {
                SkillItemComp item = queueItemComp.Dequeue();
                if(item == null) continue;

                if(info.DictSkillCompPresentInfo.ContainsKey(item.SkillId.ToLower()))
                    item.Refresh(info.DictSkillCompPresentInfo[item.SkillId.ToLower()]);
                else 
                    Debug.LogWarning("Could not find skill id in the dictionary ! " + item.SkillId);
                        
                for(int q = 0; q < item.Children.Count; q++) 
                    queueItemComp.Enqueue(item.Children[q]);
            }
        }

        public Sprite GetSkillIcon(string skillId)
        {
            skillId = skillId.ToLower();

            Queue<SkillItemComp> queue = new Queue<SkillItemComp>();
            queue.Enqueue(root);

            while(queue.Count > 0) 
            {
                SkillItemComp itemComp = queue.Dequeue();
                if(itemComp.SkillId.ToLower() == skillId.ToLower())
                    return itemComp.IconSprite;

                for(int q = 0; q < itemComp.Children.Count; q++) 
                    queue.Enqueue(itemComp.Children[q]);
            }
            return null;
        }

#if UNITY_EDITOR
        public void SetToUnlockCost(string rscId, SkillItemComp skillItem)
        {
            if(skillItem == null)
            {
                Assert.IsNotNull(skillItem, "Skill Item should not be null!");
                return;
            }
            rscId = rscId.ToLower();
            if(skillItem.SkillData.UnlockCost == null)
                skillItem.SkillData.SetUnlockCostList(new System.Collections.Generic.List<ResourceRequirement>());

            for(int q = 0; q < skillItem.SkillData.UnlockCost.Count; ++q)
            {
                if(skillItem.SkillData.UnlockCost[q].ResourceId.ToLower() == rscId)
                    return;
            }
            skillItem.SkillData.UnlockCost.Add(new ResourceRequirement(rscId, defaultUnlockCount));
        }
        public void ClearUnlockCost(string rscId, SkillItemComp skillItem)
        {
            if(skillItem == null)
            {
                Assert.IsNotNull(skillItem, "Skill Item should not be null!");
                return;
            }

            rscId = rscId.ToLower();
            if(skillItem.SkillData.UnlockCost == null)
                return;

            for(int q = 0; q < skillItem.SkillData.UnlockCost.Count; ++q)
            {
                if(skillItem.SkillData.UnlockCost[q].ResourceId.ToLower() == rscId)
                {
                    skillItem.SkillData.UnlockCost.RemoveAt(q);
                    break;
                }
            }
        }
#endif
    }
}