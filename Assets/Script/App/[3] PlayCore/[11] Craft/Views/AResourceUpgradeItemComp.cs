using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Core.Events;

namespace App.GamePlay.IdleMiner.Craft
{
    public abstract class AResourceUpgradeItemComp : MonoBehaviour
    {
        // const.
        public const int MAX_SRC_SLOT = 3;

        // Enum.
        public enum EStatus { NODATA, EMPTY, LOCKED, OPENED, MAX };


        // Serialize Field.
        [SerializeField] protected List<GameObject> StateRoots;           // Need all roots for all EStatus Type.
        [SerializeField] protected List<FrameResourceComp> SourceItems;   // list count should be 1 ~ MAX_SRC_SLOT.
        [SerializeField] protected FrameResourceComp TargetItem;
        

        // Accessor.
        public int SlotIndex => mSlotIndex;
        public string ParentDialogKey => mParentDlgKey;


        // Members.
        protected string mParentDlgKey = string.Empty;
        protected int mSlotIndex = -1;


        public class PresentInfo
        { 
            // Empty Slot.
            public PresentInfo()
            {
                Status = EStatus.EMPTY;
            }
            public EStatus Status { get; protected set; }
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            Assert.IsTrue(StateRoots != null && StateRoots.Count == (int)EStatus.MAX);
            Assert.IsNotNull(TargetItem);
            Assert.IsTrue(SourceItems!=null && SourceItems.Count>0);
        }

        public void Init(int slotIndex, string parentDlgKey)
        {
            mSlotIndex = slotIndex;
            mParentDlgKey = parentDlgKey;
        }

        public virtual void Refresh(PresentInfo presentor)
        {
            Assert.IsTrue(mSlotIndex >= 0, "Should call Init() first.");
            Assert.IsNotNull(presentor);

            for(int q = 0; q < StateRoots.Count; ++q)
            {
                StateRoots[q].SetActive(q == (int)presentor.Status);
            }
        }
    }
}
