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
    public sealed class RecipeSingleItemComp : AResourceUpgradeItemComp
    {
        // const string for Events.
        public const string EVENT_SLOT_CLICKED = "SingleRecipeItemComp_OnBtnRecipeClicked";
        public const string EVENT_LOCKED_CLICKED = "SingleRecipeItemComp_OnBtnLockedSlotClicked";



        [Header("[ Recipe Item - Locked ]")]
        [SerializeField] Image ResourceIcon;
        [SerializeField] TMP_Text UnlockResourceName;
        [SerializeField] TMP_Text UnlockCost;
        [SerializeField] TMP_Text LockedRscClass;

        [Header("[ Recipe Item - Opended ]")]
        [SerializeField] TMP_Text TargetResourceName;
        [SerializeField] TMP_Text Duration;
        [SerializeField] GameObject ArrowOneSrc;
        [SerializeField] GameObject ArrowMultiSrc;


        public class RecipePresentInfo : PresentInfo
        {
            // Empty.
            public RecipePresentInfo() : base() 
            {
                Status = EStatus.NODATA;
            }

            // Locked. 
            public RecipePresentInfo(Sprite _icon, string _rscName, string _lockedClass, BigInteger _unlockCost)
            {
                Status = EStatus.LOCKED;
                ResourceIcon = _icon;
                SlotOpenCost = _unlockCost;
                ResourceName = _rscName;
                Class = _lockedClass;
            }

            // Unlocked.
            public RecipePresentInfo(
                List<FrameResourceComp.PresentInfo> _srcList,
                FrameResourceComp.PresentInfo _target,
                string _rscName, int _durationInSec)
            {
                Status = EStatus.OPENED;
                SouceList = _srcList;
                ResourceName = _rscName;
                TargetInfo = _target;
                DurationInSec = _durationInSec;
            }

            public Sprite ResourceIcon { get; private set; }
            public string ResourceName { get; private set; }
            public BigInteger SlotOpenCost { get; private set; }
            public string Class { get; private set; }

            public int DurationInSec { get; private set; }
            public List<FrameResourceComp.PresentInfo> SouceList { get; private set; }
            public FrameResourceComp.PresentInfo TargetInfo { get; private set; }
        }


        // Start is called before the first frame update
        protected override void Start()
        {
            Assert.IsNotNull(UnlockResourceName);
            Assert.IsNotNull(UnlockCost);
            Assert.IsNotNull(LockedRscClass);

            Assert.IsNotNull(TargetResourceName);
            Assert.IsNotNull(Duration);
            Assert.IsNotNull(ArrowOneSrc);
            Assert.IsNotNull(ArrowMultiSrc);
        }

        public override void Refresh(PresentInfo presentor)
        {
            if (presentor == null)
                return;

            base.Refresh(presentor);

            var recipePresentor = (RecipePresentInfo)presentor;

            switch (presentor.Status)
            {
                case EStatus.LOCKED:
                    ResourceIcon.sprite = recipePresentor.ResourceIcon;
                    UnlockCost.text = recipePresentor.SlotOpenCost.ToAbbString();
                    UnlockResourceName.text = recipePresentor.ResourceName;
                    LockedRscClass.text = recipePresentor.Class;
                    break;
                case EStatus.OPENED:
                    {
                        int srcCnt = recipePresentor.SouceList.Count;
                        Assert.IsTrue(srcCnt>0 && srcCnt<=MAX_SRC_SLOT);

                        SourceItems[0].Refresh(srcCnt >= 1 ? recipePresentor.SouceList[0] : new FrameResourceComp.PresentInfo());
                        SourceItems[1].Refresh(srcCnt >= 2 ? recipePresentor.SouceList[1] : new FrameResourceComp.PresentInfo());
                        SourceItems[2].Refresh(srcCnt >= 3 ? recipePresentor.SouceList[2] : new FrameResourceComp.PresentInfo());
                        TargetItem.Refresh(recipePresentor.TargetInfo);

                        TargetResourceName.text = recipePresentor.ResourceName;
                        Duration.text = "Duration : " + SecondToTimeString(recipePresentor.DurationInSec);
                        ArrowOneSrc.SetActive(srcCnt == 1);
                        ArrowMultiSrc.SetActive(srcCnt > 1);
                        break;
                    }
                default:
                    break;
            }
        }


        // Event Recvers.
        //
        public void OnBtnLockedSlotClicked()
        {
            EventSystem.DispatchEvent(EVENT_LOCKED_CLICKED, this);  // Try to Unlock ?
        }
        public void OnBtnSlotClicked()
        {
            EventSystem.DispatchEvent(EVENT_SLOT_CLICKED, this);    // Open Recipe popup.?
        }



        // Simple unitls.
        string SecondToTimeString(int second)
        {
            int ToSec(int sec) => sec % 60;
            int ToHour(ref int sec)
            {
                int ret = sec / (60 * 60);
                if (ret > 0)
                    sec -= (ret * 60 * 60);
                return ret;
            }
            int ToMin(int sec)
            {
                return sec / 60;
            }


            int hour = ToHour(ref second);
            int min = ToMin(second);
            int sec = ToSec(second);

            string strTime = "";
            if (hour > 0)
                strTime = $"{hour}:{min}:{sec}";
            else
            {
                if (min > 0)
                    strTime = $"{min}:{sec}";
                else
                    strTime = $"{sec} sec";
            }
            return strTime;
        }
    }
}
