using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using static App.GamePlay.IdleMiner.ARefreshable;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class PlanetResourceSectorComp : IGCore.MVCS.AView
    {
        [SerializeField] PlanetResourceItemComp[] ElementItems;

        public class PresentInfo : APresentor
        {
            public PresentInfo(List<PlanetResourceItemComp.PresentInfo> miningStats)
            {
                this.miningStats = miningStats;
            }
            
            public List<PlanetResourceItemComp.PresentInfo> miningStats { get; private set; }
        }

        // Start is called before the first frame update
        private void Awake()
        {
            Assert.IsNotNull(ElementItems);
            Assert.IsTrue(ElementItems.Length > 0);
        }
        

        public override void Refresh(APresentor presentor)//  List<PlanetResourceItemComp.PresentInfo> miningStats)
        {
            var presentInfo = presentor as PresentInfo;
            if(presentInfo == null)     return;


         //   Assert.IsTrue(ElementItems.Length == PlanetData.MAX_MINING);
            //var townInfo = _model.GetPlanetData(info.planetInfo.PlanetId_);
            for (int q = 0; q < ElementItems.Length; ++q)
            {
                //var obtainStat = q < townInfo.Obtainables.Count ? townInfo.Obtainables[q] : null;
                var isOpened = q < presentInfo.miningStats.Count;
                ElementItems[q].gameObject.SetActive(isOpened);
                if (!isOpened) continue;

                ElementItems[q].Refresh(presentInfo.miningStats[q]);
            }
        }

    }
}