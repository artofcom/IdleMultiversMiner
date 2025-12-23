using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Numerics;
using TMPro;
using Core.Utils;
using Core.Events;
using UnityEngine.UI;
using System;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public sealed class SkillBtnComp : ARefreshable
    {
        //
        [Header("[ ]")]
        [SerializeField] Image ImgSkill;
        [SerializeField] TMP_Text TxtName;
        [SerializeField] Image ImgFrame;
        [SerializeField] Image ImageEffect;


        //[Header("[ Data Section ]")]
        //[SerializeField] List<SkillTreePathComp> skillTreePaths;


        // Events.
    //    static public Action<SkillPath.eType, string> EventOnSkillIconClicked;

        // Accessor.
        public string SkillId => gameObject.name;
        //public List<SkillTreePathComp> SkillTreePaths => skillTreePaths;
        public Sprite Icon => ImgSkill.sprite;

        // Member.
   //     SkillPath.eType type;

        public class PresentInfo : IPresentor
        {
            public PresentInfo(string category, string skill_id, string _skillName, Sprite _skillIcon, Sprite _frmIcon, bool _isEffectFrmOn)           
            {
                this.category = category;
                SkillId = skill_id;
                SkillName = _skillName;  
                SkillIcon = _skillIcon;
                FrameIcon = _frmIcon;
                IsEffectAtive = _isEffectFrmOn;
            }

            public string category { get; private set; }
            public string SkillId { get; private set; }
            public Sprite SkillIcon { get; private set; }
            public Sprite FrameIcon { get; private set; }
            public string SkillName { get; private set; }
            public bool IsEffectAtive { get; private set; }
        }


        // Start is called before the first frame update
        void Start()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(SkillId));
            Assert.IsNotNull(ImgSkill);
            Assert.IsNotNull(ImgFrame);
            Assert.IsNotNull(TxtName);
        }


        public override void Refresh(IPresentor presentInfo)
        {
            if (presentInfo == null)
                return;

            var presentor = (PresentInfo)presentInfo;
            if (presentor == null)
                return;

         //   this.type = presentor.Type;

            TxtName.text = presentor.SkillName;
            // ImgSkill.sprite = presentor.SkillIcon;
            ImgFrame.sprite = presentor.FrameIcon;
            if (ImageEffect != null)
                ImageEffect.gameObject.SetActive(presentor.IsEffectAtive);
        }


        // Event Recvers.
        //
        public void OnBtnSkillClicked()
        {
         //   EventOnSkillIconClicked.Invoke(this.type, this.SkillId);
        }
    }
}
