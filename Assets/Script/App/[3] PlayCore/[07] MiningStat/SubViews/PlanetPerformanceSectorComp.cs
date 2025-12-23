using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//using static App.GamePlay.IdleMiner.ARefreshable;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Types;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class PlanetPerformanceSectorComp : IGCore.MVCS.AView
    {
        [SerializeField] GameObject openedRoot;
        [SerializeField] GameObject closedRoot;
        [SerializeField] GameObject clearedRoot;

        [SerializeField] PlanetStatComp[] townStats;
        [SerializeField] TMP_Text upgradeLevelMultiply;

        public class PresentInfo : APresentor
        {
            public PresentInfo(string lvMode, PlanetStatComp.PresentInfo shotInterval, PlanetStatComp.PresentInfo shotAccuracy, 
                PlanetStatComp.PresentInfo shipSpeed, PlanetStatComp.PresentInfo cargoSize)
            {
                this.levelMode = lvMode;    
                this.shotInterval = shotInterval;
                this.shotAccuracy = shotAccuracy;
                this.shipSpeed = shipSpeed;
                this.cargoSize = cargoSize;
            }
            
            public string levelMode { get; private set; }
            public PlanetStatComp.PresentInfo shotInterval { get; private set; }
            public PlanetStatComp.PresentInfo shotAccuracy { get; private set; }
            public PlanetStatComp.PresentInfo shipSpeed { get; private set; }
            public PlanetStatComp.PresentInfo cargoSize { get; private set; }
        }

        // Start is called before the first frame update
        private void Awake()
        {
            Assert.IsNotNull(townStats);
            Assert.IsNotNull(upgradeLevelMultiply);
            Assert.IsTrue(townStats.Length == (int)eABILITY.MAX);
            Assert.IsNotNull(openedRoot);
            Assert.IsNotNull(closedRoot);
            Assert.IsNotNull(clearedRoot);
        }

        public override void Refresh(APresentor presentor)
        {
            var presentInfo = presentor as PresentInfo;
            if (presentInfo == null)    return;
            

            openedRoot.SetActive(true);
            closedRoot.SetActive(false);
            clearedRoot.SetActive(false);

            upgradeLevelMultiply.text = presentInfo.levelMode;

            townStats[(int)eABILITY.SHOT_INTERVAL].Refresh(presentInfo.shotInterval);
            townStats[(int)eABILITY.SHOT_ACCURACY].Refresh(presentInfo.shotAccuracy);

            townStats[(int)eABILITY.DELIVERY_SPEED].gameObject.SetActive(true);
            townStats[(int)eABILITY.CARGO_SIZE].gameObject.SetActive(true);
            townStats[(int)eABILITY.DELIVERY_SPEED].Refresh(presentInfo.shipSpeed);
            townStats[(int)eABILITY.CARGO_SIZE].Refresh(presentInfo.cargoSize);

            townStats[(int)eABILITY.MINING_RATE].gameObject.SetActive(false);
        }
        public void Refresh(string lvMode, PlanetStatComp.PresentInfo _miningRate, PlanetStatComp.PresentInfo _shotInterval, PlanetStatComp.PresentInfo _shotAccuracy)
        {
            openedRoot.SetActive(true);
            closedRoot.SetActive(false);
            clearedRoot.SetActive(false);

            upgradeLevelMultiply.text = lvMode;

            townStats[(int)eABILITY.SHOT_INTERVAL].Refresh(_shotInterval);
            townStats[(int)eABILITY.SHOT_ACCURACY].Refresh(_shotAccuracy);

            townStats[(int)eABILITY.MINING_RATE].gameObject.SetActive(true);
            townStats[(int)eABILITY.MINING_RATE].Refresh(_miningRate);

            townStats[(int)eABILITY.DELIVERY_SPEED].gameObject.SetActive(false);
            townStats[(int)eABILITY.CARGO_SIZE].gameObject.SetActive(false);
        }
        // Closed.
        public void Refresh()
        {
            openedRoot.SetActive(false);
            closedRoot.SetActive(true);
            clearedRoot.SetActive(false);
        }
        // Battle Finished - Cleared.
        public void Refresh(bool dontcare_4cleared)
        {
            openedRoot.SetActive(false);
            closedRoot.SetActive(false);
            clearedRoot.SetActive(true);
        }
    }
}
