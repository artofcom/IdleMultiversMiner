using System.Collections;
using System.Collections.Generic;
using Core.Utils;
//using UnityEngine;
using Core.Events;
//us/ing App.GamePlay.IdleGame.SubSystem.GamePlay;
//using App.GamePlay.IdleGame.SubSystem;
//using App.GamePlay.IdleGame.SubSystem.Data;
//using App.GamePlay.IdleGame.SubSystem.UIComponent.TownDialog;
//using App.GamePlay.IdleGame.SubSystem.UIComponent.ElementDialog;
//using System;
//using Unity.Services.Analytics;
//using Unity.Services.Core;
using System;
using System.Globalization;
using App.GamePlay.IdleMiner.PopupDialog;
using IGCore.MVCS;


namespace App.GamePlay.IdleMiner
{

    //  SkillTree Controlling.-------------------------------------
    //
    public class DailyAttendController : AController// AMinerModule
    {
       // APopupDialog dailyAttendDialogCache = null;

        //public DailyAttendController(IdleMinerController controller) : base(controller) { }
        
        public DailyAttendController(AUnit unit, AView view, AModel model, AContext ctx)
            : base(unit, view, model, ctx)
        { }

        public override void Init()
        {
            base.Init();
        }


        protected override void OnViewEnable() { }
        protected override void OnViewDisable() { }

        public override void Resume(int awayTimeInSec) { }
        public override void Pump() { }
        public override void WriteData() { }

        /*public override void Resume(int duration)
        {
            controller.Model.ResumeDailyAttend();
        }

        public override void Pump()
        {
            controller.Model.PumpDailyAttend();
        }






        //  Interval funcs.-------------------------------------
        //
        protected override void InitModule()
        {
            IdleMinerView.EventOnBtnDailyAttendClicked += View_OnBtnDailyAttendClicked;
            DailyAttendPopupDialog.EventOnBtnClaimClicked += DailyAttendPopupDialog_OnBtnClaimClicked;
            events.RegisterEvent(IdleMinerModel.EVENT_ON_DAILY_ATTEND_CLAIMED, EventOnDailyAttendClaimed);
        }

        //  Events.-------------------------------------
        //
        void View_OnBtnDailyAttendClicked()
        {
            List<DailyAttendItem.PresentInfo> listPresent = new List<DailyAttendItem.PresentInfo>();
            var dailyAttendPlayerInfo = controller.Model.PlayerData.DailyAttendProgInfo;
            for(int q = 0; q < controller.Model.DailyAttendData.ListItemData.Count; ++q)
            {
                DailyAttendItem.State state = DailyAttendItem.State.UNREACHABLE;
                if(q < dailyAttendPlayerInfo.IndexCurDay)          
                    state = DailyAttendItem.State.CLAIMED;
                else if(q == dailyAttendPlayerInfo.IndexCurDay)
                    state = dailyAttendPlayerInfo.CanClaim ? DailyAttendItem.State.CLAIMABLE : DailyAttendItem.State.CLAIMED;
                else 
                    state = DailyAttendItem.State.UNREACHABLE;
                
                DailyAttendItemData itemData = controller.Model.DailyAttendData.ListItemData[q];
                string currency = itemData.Reward.Type==0 ? "Gold ":"Mana ";
                string amount = itemData.Reward.BIAmount.ToAbbString();
                string desc = $"{currency} {amount}";
                listPresent.Add(new DailyAttendItem.PresentInfo(q, $"Day {q+1}", desc, state));
            }

            DateTime nextRewardTime = dailyAttendPlayerInfo.GetNextRewardTime(true);
            string instruction = dailyAttendPlayerInfo.CanClaim ? "Click Attend and Claim Reward!" : "Wait for next reward time : " + nextRewardTime.ToString(CultureInfo.InvariantCulture);

            dailyAttendDialogCache = controller.Context.PopUpScreen.DisplayPopupDialog(DailyAttendPopupDialog.sID, 
                new DailyAttendPopupDialog.PresentInfo(listPresent, dailyAttendPlayerInfo.CanClaim, instruction),
                (a) => 
                {
                    dailyAttendDialogCache = null;
                });
        }

        void DailyAttendPopupDialog_OnBtnClaimClicked(int idxDay)
        {
            if(idxDay < 0)
                idxDay = controller.Model.PlayerData.DailyAttendProgInfo.IndexCurDay;

            controller.Model.ClaimDailyAttendReward(idxDay);
        }

        void EventOnDailyAttendClaimed(object data)
        {
            int idxDay = (int)data;
            UnityEngine.Debug.Log("EventOnDailyAttendClaimed!! " + idxDay.ToString());

            if(dailyAttendDialogCache != null)
                dailyAttendDialogCache.OnClose();

           View_OnBtnDailyAttendClicked();
        }*/
    }
}
