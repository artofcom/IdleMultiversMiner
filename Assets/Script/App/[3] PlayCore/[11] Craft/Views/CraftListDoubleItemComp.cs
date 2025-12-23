using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.Craft
{
    public class CraftListDoubleItemComp : MonoBehaviour
    {
        [SerializeField] List<AResourceUpgradeItemComp> items;


        public int SingleItemCount => items!=null ? items.Count : 0;


        // Start is called before the first frame update
        void Start()
        {
            Assert.IsTrue(items!=null && items.Count>0);
        }


        public class PresentInfo
        {
            public PresentInfo(List<AResourceUpgradeItemComp.PresentInfo> presentInfos)
            {
                DisplayInfos = presentInfos;
            }

            public List<AResourceUpgradeItemComp.PresentInfo> DisplayInfos { get; private set; }
        }

        public void Init(int idxStart, string mainDlgName)
        {
            for(int q = 0; q < items.Count; q++) 
            {  
                items[q].Init(idxStart+q, mainDlgName);
            }
        }

        public void Refresh(PresentInfo info)
        {
            if (info == null) return;

            Assert.IsTrue(info.DisplayInfos.Count == items.Count);

            for(int q = 0; q < items.Count; q++)
                items[q].Refresh(info.DisplayInfos[q]);
        }
    }
}