using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.MiningStat
{
    // Damage Sector for Boss Battle.
    public class PlanetDamageSectorComp : IGCore.MVCS.AView
    {
        [SerializeField] GameObject openedRoot;
        [SerializeField] GameObject closedRoot;
        [SerializeField] GameObject clearedRoot;
        [SerializeField] List<PlanetDamageItemComp> planetDamageItemComps;

        public int ItemCount => planetDamageItemComps.Count;

        public class PresentInfo : APresentor
        {
            public PresentInfo(List<PlanetDamageItemComp.PresentInfo> listDmgItems)
            {
                listDmgItemComps = listDmgItems;
            }
            public List<PlanetDamageItemComp.PresentInfo> listDmgItemComps { get; private set; }
        }

        // Start is called before the first frame update
        void Awake()
        {
            Assert.IsNotNull(planetDamageItemComps);
            Assert.IsTrue(planetDamageItemComps.Count > 0);
            Assert.IsNotNull(openedRoot);
            Assert.IsNotNull(closedRoot);
            Assert.IsNotNull(clearedRoot);
        }

        // Refresh When Opened.
        public override void Refresh(APresentor presentor)   // List<PlanetDamageItemComp.PresentInfo> presentsInfo)
        {
            var presentsInfo = presentor as PresentInfo;

            if(presentsInfo == null)    return;

            openedRoot.SetActive(true);
            closedRoot.SetActive(false);
            clearedRoot.SetActive(false);

            Assert.IsTrue(presentsInfo.listDmgItemComps.Count == planetDamageItemComps.Count);

            for (int q = 0; q < planetDamageItemComps.Count; q++)
            {
                planetDamageItemComps[q].Refresh(presentsInfo.listDmgItemComps[q]);
            }
        }

        // Refresh When Closed.
        public void Refresh()
        {
            openedRoot.SetActive(false);
            closedRoot.SetActive(true);
            clearedRoot.SetActive(false);
        }
        public void Refresh(bool dontcare_4clear)
        {
            openedRoot.SetActive(false);
            closedRoot.SetActive(false);
            clearedRoot.SetActive(true);
        }
    }
}