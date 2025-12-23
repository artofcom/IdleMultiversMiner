using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    public class ManagerList3ItemComp : MonoBehaviour
    {
        public const int ITEM_COUNT = 3;

        [SerializeField] List<ManagerItemComp> ManagerItems;

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsTrue(ManagerItems != null && ManagerItems.Count == ITEM_COUNT);
        }

        public class PresentInfo
        {
            public PresentInfo(List<ManagerItemComp.PresentInfo> _listItemPresentor)
            {
                ItemPresentor = _listItemPresentor;
            }

            public List<ManagerItemComp.PresentInfo> ItemPresentor { get; private set; }
        }

        public void Refresh(PresentInfo presentor)
        {
            if (presentor == null)
                return;

            Assert.IsTrue(presentor.ItemPresentor.Count == ITEM_COUNT);

            for (int q = 0; q < ITEM_COUNT; ++q)
                ManagerItems[q].Refresh(presentor.ItemPresentor[q]);
        }
    }
}
