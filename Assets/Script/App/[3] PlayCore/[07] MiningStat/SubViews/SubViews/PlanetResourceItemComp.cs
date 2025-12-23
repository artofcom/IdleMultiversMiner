using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Core.Events;
using Core.Utils;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class PlanetResourceItemComp : IGCore.MVCS.AView
    {
        [SerializeField] Image ImageIcon;
        [SerializeField] Image ImageBG;
        [SerializeField] TMP_Text Name;
        [SerializeField] TMP_Text YieldRate;
        [SerializeField] TMP_Text MiningRate;
        [SerializeField] TMP_Text MinedCount;
        [SerializeField] TMP_Text CollectRate;
        [SerializeField] TMP_Text CollectedCount;
        [SerializeField] GameObject objClassText;

        [SerializeField] Color InitFrameColor = Color.white;
        [SerializeField] Color InitIconColor = Color.white;

        public class PresentInfo : APresentor
        {
            public PresentInfo(Sprite icon) // Empty.
            {
                this.Icon = icon;
            }
            public PresentInfo(Sprite _icon, string _name, string _yield, string _miningRate, string _minedCount, string _collectRate, string _collectedCnt)
            {
                Icon = _icon;   Name = _name; Yield = _yield; MineRate = _miningRate; MinedCount = _minedCount; CollectRate = _collectRate;  CollectedCount = _collectedCnt;
            }
            public Sprite Icon { get; private set; }
            public string Name { get; private set; }
            public string Yield { get; private set; }
            public string MineRate { get; private set; }
            public string MinedCount { get; private set; }
            public string CollectRate { get; private set; }
            public string CollectedCount { get; private set; }
        }

        // Start is called before the first frame update
        void Awake()
        {
            UnityEngine.Assertions.Assert.IsNotNull(ImageIcon);
            UnityEngine.Assertions.Assert.IsNotNull(Name);
            UnityEngine.Assertions.Assert.IsNotNull(YieldRate);
            UnityEngine.Assertions.Assert.IsNotNull(MiningRate);
            UnityEngine.Assertions.Assert.IsNotNull(MinedCount);
            UnityEngine.Assertions.Assert.IsNotNull(CollectRate);
            UnityEngine.Assertions.Assert.IsNotNull(CollectedCount);
            UnityEngine.Assertions.Assert.IsNotNull(objClassText);
        }



        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var info = (PresentInfo)presentor;

            ImageIcon.sprite = info.Icon;
            Name.text = info.Name;
            YieldRate.text = info.Yield;        
            MiningRate.text = info.MineRate;    
            MinedCount.text = info.MinedCount;
            CollectRate.text = info.CollectRate;
            CollectedCount.text = info.CollectedCount;

            bool isEmptyItem = string.IsNullOrEmpty(info.Name);

            ImageIcon.color = isEmptyItem ? new Color(1.0f, 1.0f, 1.0f, .0f) : InitIconColor;
            ImageBG.color = isEmptyItem ? new Color(1.0f, 1.0f, 1.0f, .0f) : InitFrameColor;
            objClassText.SetActive(!isEmptyItem);
        }
    }
}