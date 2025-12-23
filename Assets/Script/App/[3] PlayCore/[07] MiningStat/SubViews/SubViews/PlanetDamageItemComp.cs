using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static App.GamePlay.IdleMiner.ARefreshable;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class PlanetDamageItemComp : IGCore.MVCS.AView
    {
        [SerializeField] TMP_Text txtTitle;
        [SerializeField] Slider sliderProgress;
        [SerializeField] TMP_Text txtStat;

        public class PresentInfo : APresentor
        {
            // Closed.
            public PresentInfo(string itemName, float progress, string stat)
            {
                ItemName = itemName;
                ProgressRate = progress;
                Stats = stat;
            }

            public string ItemName { get; private set; }
            public float ProgressRate { get; private set; } // ~ 1.0f
            public string Stats { get; private set; }

        }

        // Start is called before the first frame update
        protected void Awake()
        {
            Assert.IsNotNull(txtTitle);
            Assert.IsNotNull(sliderProgress);
            Assert.IsNotNull(txtStat);
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null) return;

            PresentInfo info = (PresentInfo) presentor;
            if(info == null) return;

            txtTitle.text = info.ItemName;
            sliderProgress.value = info.ProgressRate;
            txtStat.text = info.Stats;
        }
    }
}