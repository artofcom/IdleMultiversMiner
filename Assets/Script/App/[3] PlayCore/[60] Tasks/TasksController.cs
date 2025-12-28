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
using App.GamePlay.IdleMiner.PopupDialog;
using IGCore.MVCS;

namespace App.GamePlay.IdleMiner
{

    //  Tasks Controlling.-------------------------------------
    //
    public class TasksController : AController//  AMinerModule
    {
        // APopupDialog dailyAttendDialogCache = null;

       // public TasksController(IdleMinerController controller) : base(controller) { }
        
        public TasksController(AUnit unit, AView view, AModel model, AContext ctx)
            : base(unit, view, model, ctx)
        { }


        public override void Init() {}
        protected override void OnViewEnable() { }
        protected override void OnViewDisable() { }

        public override void Resume(int awayTimeInSec) { }
        public override void Pump() { }
        public override void WriteData() { }




        //  Interval funcs.-------------------------------------
        //
       // protected override void InitModule()
      //  {
       //     IdleMinerView.EventOnBtnTaskClicked += View_OnBtnTasksClicked;
            //DailyAttendPopupDialog.EventOnBtnClaimClicked += DailyAttendPopupDialog_OnBtnClaimClicked;
            //Events.RegisterEvent(IdleMinerModel.EVENT_ON_DAILY_ATTEND_CLAIMED, EventOnDailyAttendClaimed);
     //   }

        //  Events.-------------------------------------
        //
        
        void View_OnBtnTasksClicked()
        {
          //  controller.Context.PopUpScreen.DisplayPopupDialog(TasksPopupDialog.sID, new TasksPopupDialog.PresentInfo(), (a) => { });

            /*
            List<DailyAttendItem.PresentInfo> listPresent = new List<DailyAttendItem.PresentInfo>();
            var dailyAttendPlayerInfo = controller.Model.PlayerData.DailyAttendProgInfo;
            for(int q = 0; q < controller.Model.DailyAttendData.ListItemData.Count; ++q)
            {
                DailyAttendItem.State state = DailyAttendItem.State.UNREACHABLE;
                if(q < dailyAttendPlayerInfo.IndexOpenedDay)          
                    state = DailyAttendItem.State.CLAIMED;
                else if(q == dailyAttendPlayerInfo.IndexOpenedDay)
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
            string instruction = dailyAttendPlayerInfo.CanClaim ? "Claim Attend Reward!" : "Wait for next reward time : " + nextRewardTime.ToString();

            dailyAttendDialogCache = controller.Context.PopUpScreen.DisplayPopupDialog(DailyAttendPopupDialog.sID, 
                new DailyAttendPopupDialog.PresentInfo(listPresent, instruction),
                (a) => 
                {
                    dailyAttendDialogCache = null;
                });*/
        }
        /*
         * public void Resume(int duration)
        {
            controller.Model.ResumeDailyAttend(duration);
        }

        public void Pump()
        {
            controller.Model.PumpDailyAttend();
        }

        void DailyAttendPopupDialog_OnBtnClaimClicked(int idxDay)
        {
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
