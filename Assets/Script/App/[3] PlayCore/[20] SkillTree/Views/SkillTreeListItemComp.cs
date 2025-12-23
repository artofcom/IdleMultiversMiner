using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class SkillTreeListItemComp : IGCore.MVCS.AView
    {
        [SerializeField] Image Icon;
        [SerializeField] TMP_Text TxtName;
        [SerializeField] TMP_Text TxtClass;
        [SerializeField] Image BGImage;
        [SerializeField] Image BGSelected;

        [Header("Select Sprite From Assets.")]
        [SerializeField] Sprite sprLearningBG;
        [SerializeField] Sprite sprLearnedBG;
        [SerializeField] Sprite sprUnreachableBG;
        

        public static Action<uint> EventOnBtnClicked;

        uint id;

        public class PresentInfo : APresentor
        {
            public enum STATUS
            {
                LEARNING, LEARNED, UNREACHABLE, SELECTED,
            };


            public PresentInfo(uint id, STATUS status, Sprite sprIcon, string className, string name, bool isSelected)
            {
                this.Id = id;
                this.status = status;
                this.sprIcon = sprIcon;
                this.className = className;
                this.name = name;
                this.isSelected = isSelected;
            }   

            public uint Id { get; private set; }
            public STATUS status { get; private set;}
            public Sprite sprIcon { get; private set;}
            public string className { get; private set;}
            public string name { get; private set;}
            public bool isSelected { get; private set;}
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            Assert.IsNotNull(Icon);
            Assert.IsNotNull(TxtName);
            Assert.IsNotNull(TxtClass); 
            Assert.IsNotNull(BGImage);
            Assert.IsNotNull(BGSelected);

            Assert.IsNotNull(sprLearningBG);
            Assert.IsNotNull(sprLearnedBG);
            Assert.IsNotNull(sprUnreachableBG);
        }


        public override void Refresh(APresentor presentor)
        {
            PresentInfo info = presentor as PresentInfo;
            if(info == null)    return;

            id = info.Id;

            Sprite sprBG = null;
            switch(info.status)
            {
            case PresentInfo.STATUS.LEARNING:   sprBG = sprLearningBG;      break;
            case PresentInfo.STATUS.LEARNED:    sprBG = sprLearnedBG;       break;
            
            default:
            case PresentInfo.STATUS.UNREACHABLE:sprBG = sprUnreachableBG;   break;
            }

            Icon.sprite = info.sprIcon;
            BGImage.sprite = sprBG;
            TxtName.text = info.name;
            TxtClass.text = info.className;

            BGSelected.gameObject.SetActive(info.isSelected);
        }


        public void OnBtnClicked()
        {
            EventOnBtnClicked?.Invoke(this.id);
        }
    }
}
