using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Numerics;
using Core.Events;
using UnityEngine.UI;
using Core.Utils;
using System;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Types;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class PlanetStatComp : IGCore.MVCS.AView
    {
        // public const string EVENT_TRY_UPGRADE_CLICKED = "TownStatComp_OnBtnTryUpgradeClicked";

        [SerializeField] TMP_Text Name;
        [SerializeField] TMP_Text Level;
        [SerializeField] TMP_Text Value;
        [SerializeField] TMP_Text UpgradeCost;
        [SerializeField] Button BtnLevelUp;

        static public Action<Tuple<int, int, eABILITY>> EventOnUpgradeClicked = null;
        static public Action<Tuple<int, int, eABILITY>> EventOnResetClicked = null;

        public int ZoneId { get; private set; }
        public int PlanetId { get; private set; }
        public eABILITY StatType { get; private set; }

        public class PresentInfo : APresentor
        {
            public PresentInfo(int _zoneId, int _planetId, eABILITY _statType, string _name, int _level, string _performance, string _cost, bool _canBuy)
            {
                ZoneId = _zoneId;
                PlanetId = _planetId;   MineStatType = _statType;
                Name = _name; Level = _level; Performance = _performance; NextCost = _cost; IsPurchable = _canBuy;  
            }
            public PresentInfo() { }

            public int ZoneId { get; private set; }
            public int PlanetId { get; private set; }
            public eABILITY MineStatType { get; private set; }
            public string Name { get; private set; }
            public int Level { get; private set; }
            public string Performance { get; private set; }
            public string NextCost { get; private set; }
            public bool IsPurchable { get; private set; }
        }


        // Start is called before the first frame update
        private void Awake()
        {
            Assert.IsNotNull(Name);
            Assert.IsNotNull(Level);
            Assert.IsNotNull(Value);
            Assert.IsNotNull(UpgradeCost);
            Assert.IsNotNull(BtnLevelUp);
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var info = (PresentInfo)presentor;

            ZoneId = info.ZoneId;
            PlanetId = info.PlanetId;
            StatType = info.MineStatType;

            // const string RuneIcon = "<sprite name=\"Rune\">";
            Name.text = info.Name;
            Level.text = $"Lv. {info.Level}";
            Value.text = info.Performance;      //  $"{info.Value}" + " " + info.PostStrAfterValue;
            UpgradeCost.text = info.NextCost;   //  RuneIcon + $" {info.Cost.ToAbbString()}";
            BtnLevelUp.interactable = info.IsPurchable;
        }

        public void OnClickTryUpgrade()
        {
            EventOnUpgradeClicked?.Invoke(new Tuple<int, int, eABILITY>(ZoneId, PlanetId, StatType));
        }
        public void OnClickReset()
        {
            EventOnResetClicked?.Invoke(new Tuple<int, int, eABILITY>(ZoneId, PlanetId, StatType));
        }
    }
}
