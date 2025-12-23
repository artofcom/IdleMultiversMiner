using App.GamePlay.IdleMiner.Common.Model;
using UnityEngine;
using App.GamePlay.IdleMiner.Common;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class SkillItemComp : IGCore.MVCS.AView
    {
        [SerializeField] List<SkillItemComp> children;

        [SerializeField] Image iconSkill;
        [SerializeField] Image lineV, lineH;
        [SerializeField] Image iconFrame;
        [SerializeField] [ReadOnly] string skill_id;

        [Header(" ")]
        [SerializeField] Sprite frameFinished;
        [SerializeField] Sprite frameWorking;
        [SerializeField] Sprite frameLearnable;
        [SerializeField] Sprite frameUnreachable;
        
        [SerializeField] Color defaultLineColor;
        [SerializeField] Color unreachableLineColor;
        
        public List<SkillItemComp> Children => children;
        public string SkillId => skill_id;

        public static System.Action<string> EventOnIconClicked;
        public Sprite IconSprite => iconSkill.sprite;

    #if UNITY_EDITOR
        [Header("--- Editor Area ---")]
        [Header(" mining_zone_buff [ZoneId:Duration:Rate:CoolTime]")]
        [Header(" unlock_feature [feature_name]")]
        [Header(" game_reset [reward_currency:count]")]

        [Header(" ")]
        [SerializeField] protected SkillInfo skillData;
        public SkillInfo SkillData => skillData;
        public void SetSkillId(string skillId)  {   skill_id = skillId; }
        public Sprite SkillSprite => iconSkill?.sprite;
    #endif

        public class Presentor : APresentor
        {
            public Presentor(string skill_id, SKILL_STATUS status, bool isLearnable, Sprite skillIcon)
            {
                this.Skill_id = skill_id;
                this.status = status;
                this.IsLearnable = isLearnable;
                this.skillIcon = skillIcon;
            }

            public string Skill_id { get; private set; }
            public SKILL_STATUS status { get; protected set; }
            public Sprite skillIcon { get; protected set; }
            public bool IsLearnable { get; protected set; }

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            bool isEmptyNode = gameObject.name.ToLower().Contains("empty");
            Assert.IsTrue(isEmptyNode || (!isEmptyNode && !string.IsNullOrEmpty(skill_id)), "Run Build Data From the Edtior first." );
        }

        public override void Refresh(APresentor presentor)
        {
            Presentor info = presentor as Presentor;
            if (info == null)
                return;
            
            Assert.IsTrue(this.skill_id == info.Skill_id);

            iconSkill.sprite = info.skillIcon!=null ? info.skillIcon : iconSkill.sprite;
            
            Sprite spriteFrame = frameFinished;
            Color lineColor = defaultLineColor;
            Color iconColor = Color.white;
            switch(info.status)
            {
            case SKILL_STATUS.LEARNED:
                iconColor = Color.gray;
                break;
            case SKILL_STATUS.LEARNING:
                spriteFrame = info.IsLearnable ? frameLearnable : frameWorking;
                break;
            case SKILL_STATUS.UNREACHABLE:
                spriteFrame = frameUnreachable;
                lineColor = unreachableLineColor;
                break;
            default:
                Assert.IsTrue(false, "Unknown Skill Status : " + info.status);
                break;
            }
            
            iconSkill.color = iconColor;
            iconFrame.sprite = spriteFrame;
            if(lineV.gameObject.activeSelf)     lineV.color = lineColor;
            if(lineH.gameObject.activeSelf)     lineH.color = lineColor;
        }

        public void OnIconClicked()
        {
            Debug.Log(skill_id + " icon clicked.");
            EventOnIconClicked?.Invoke(this.skill_id);
        }
    }
}
