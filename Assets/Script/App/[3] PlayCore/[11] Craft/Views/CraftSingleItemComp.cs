using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Numerics;
using TMPro;
using Core.Utils;
using Core.Events;
using UnityEngine.UI;

namespace App.GamePlay.IdleMiner.Craft
{
    public sealed class CraftSingleItemComp : AResourceUpgradeItemComp
    {
        // const string for Events.
        public const string EVENT_SLOT_CLICKED = "SingleCraftItemComp_OnBtnRecipeClicked";
        public const string EVENT_EMPTY_CLICKED = "SingleCraftItemComp_OnBtnEmptySlotClicked";
        public const string EVENT_LOCKED_CLICKED = "SingleCraftItemComp_OnBtnLockedSlotClicked";
        public const string EVENT_PROG_X_CLICKED = "SingleCraftItemComp_OnBtnProgXClicked";



        [Header("[ Craft Item - Locked ]")]
        [SerializeField] TMP_Text ClosedSlotMessage;
        [SerializeField] TMP_Text SlotOpenCost;

        [Header("[ Craft Item - Opended ]")]
        [SerializeField] TMP_Text TimeProcessing;
        [SerializeField] Slider TimeSlider;
        [SerializeField] GameObject ArrowOneSrc;
        [SerializeField] GameObject ArrowMultiSrc;
        [SerializeField] GameObject MultiProductionIcon;

        public class CraftPresentInfo : PresentInfo
        {
            // Empty or (Opened but not assigned).
            public CraftPresentInfo(bool isOpened) : base() 
            {
                Status = isOpened ? EStatus.EMPTY : EStatus.NODATA;
            }

            // Locked. 
            public CraftPresentInfo(BigInteger _slotCost, string _closedSlotMsg)
            {
                Status = EStatus.LOCKED;
                SlotOpenCost = _slotCost;
                ClosedSlotMessage = _closedSlotMsg;
            }

            // Unlocked.
            public CraftPresentInfo(
                List<FrameResourceComp.PresentInfo> _srcList,
                FrameResourceComp.PresentInfo _target,
                int _duration,
                int _progressTime, bool isMultiProduction)
            {
                Status = EStatus.OPENED;
                SouceList = _srcList;
                Progresstime = _progressTime;
                TargetInfo = _target;
                Duration = _duration;
                IsMultiProduction = isMultiProduction;
            }


            public BigInteger SlotOpenCost { get; private set; }
            public string ClosedSlotMessage { get; private set; }
            public List<FrameResourceComp.PresentInfo> SouceList { get; private set; }
            public FrameResourceComp.PresentInfo TargetInfo { get; private set; }
            public int Duration { get; private set; }
            public int Progresstime { get; private set; }
            public bool IsMultiProduction { get; private set; }
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            Assert.IsNotNull(SlotOpenCost);
            Assert.IsNotNull(ClosedSlotMessage);

            Assert.IsNotNull(TimeProcessing);
            Assert.IsNotNull(TimeSlider);
            Assert.IsNotNull(ArrowOneSrc);
            Assert.IsNotNull(ArrowMultiSrc);
        }

        public override void Refresh(PresentInfo presentor)
        {
            if (presentor == null)
                return;

            base.Refresh(presentor);

            var craftPresentor = (CraftPresentInfo)presentor;

            switch (presentor.Status)
            {
                case EStatus.LOCKED:
                    SlotOpenCost.text = craftPresentor.SlotOpenCost.ToAbbString();
                    ClosedSlotMessage.text = craftPresentor.ClosedSlotMessage;
                    break;
                case EStatus.OPENED:
                    {
                        int srcCnt = craftPresentor.SouceList.Count;
                        Assert.IsTrue(srcCnt>0 && srcCnt<=MAX_SRC_SLOT);

                        SourceItems[0].Refresh(srcCnt >= 1 ? craftPresentor.SouceList[0] : new FrameResourceComp.PresentInfo());
                        SourceItems[1].Refresh(srcCnt >= 2 ? craftPresentor.SouceList[1] : new FrameResourceComp.PresentInfo());
                        SourceItems[2].Refresh(srcCnt >= 3 ? craftPresentor.SouceList[2] : new FrameResourceComp.PresentInfo());
                        TargetItem.Refresh(craftPresentor.TargetInfo);

                        ArrowOneSrc.SetActive(srcCnt == 1);
                        ArrowMultiSrc.SetActive(srcCnt > 1);

                        string progressStatus = "IDLE";
                        if(craftPresentor.Progresstime >= 0)
                        {
                            long prog = craftPresentor.Duration - craftPresentor.Progresstime;
                            progressStatus = TimeExt.ToTimeString(prog, TimeExt.UnitOption.NO_USE, TimeExt.TimeOption.HOUR);
                        }
                        TimeProcessing.text = progressStatus;
                        TimeSlider.value = ((float)craftPresentor.Progresstime) / ((float)craftPresentor.Duration);

                        MultiProductionIcon.SetActive(craftPresentor.IsMultiProduction);

                        break;
                    }
                default:
                    break;
            }
        }


        // Event Recvers.
        //
        public void OnBtnEmptySlotClicked()
        {
            EventSystem.DispatchEvent(EVENT_EMPTY_CLICKED, this);   // Try to Assign ?
        }
        public void OnBtnLockedSlotClicked()
        {
            EventSystem.DispatchEvent(EVENT_LOCKED_CLICKED, this);  // Try to Unlock ?
        }
        public void OnBtnRecipeClicked()
        {
            EventSystem.DispatchEvent(EVENT_SLOT_CLICKED, this);    // Open Recipe popup.?
        }
        public void OnBtnProgressXClicked()
        {
            EventSystem.DispatchEvent(EVENT_PROG_X_CLICKED, this);  // Stop and Empty Slot.?
        }
    }
}
