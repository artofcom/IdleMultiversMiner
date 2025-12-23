//using System.Collections;
//using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
//using Core.Events;
//using System;

namespace App.GamePlay.IdleMiner
{

    //  SkillTree Controlling.-------------------------------------
    //
    public class BoosterController : IGCore.MVCS.AController// AMinerModule
    {
        public BoosterController(IGCore.MVCS.AView view, IGCore.MVCS.AModel model, IGCore.MVCS.AContext ctx)
            : base(view, model, ctx)
        { }

        public override void Init(){}

        protected override void OnViewEnable() { }

        protected override void OnViewDisable() { }

        public override void Resume(int awayTimeInSec) { }
        
        public override void Pump() { }
        
        public override void WriteData() { }

        /*BoosterListPanelView boosterPnlViewCache => (BoosterListPanelView)view;

        public BoosterController(IdleMinerController _controller) : base(_controller) { }
        
        
        public override void Pump()
        {            
            var usingBoosters = controller.Model.PlayerData.UsingBoostersInfo;
            for(int q = 0; q < usingBoosters.Count; ++q)
            {
                --usingBoosters[q].RemainTime;
                if(usingBoosters[q].RemainTime <= 0)
                {
                    controller.Model.PlayerData.ExpireBooster(controller.Model.GetBoosterInfo(usingBoosters[q].BoosterId), q);

                    // Delete Only One at a time!
                    break;
                }
            }
        }





        void BoosterListPnlView_OnEnable(ARefreshable refreshable)
        {
            view = refreshable;
            
            RefreshPanelView();

            Debug.Log("Booster View Enabled.");
        }
        void BoosterListPnlView_OnDisable()
        {

        }
        void BoosterItemComp_OnPnlClicked(string boosterId)
        {

        }




        protected override void InitModule()
        {
            BoosterListPanelView.EventOnEnable += BoosterListPnlView_OnEnable;
            BoosterListPanelView.EventOnDisable += BoosterListPnlView_OnDisable;
            BoosterItemComp.EventOnPnlClicked += BoosterItemComp_OnPnlClicked;
        }
        void RefreshPanelView()
        {
            Assert.IsNotNull(boosterPnlViewCache);

            List<BoosterItemComp.PresentInfo> compListInfo = new List<BoosterItemComp.PresentInfo>();
            List<BoosterInfo> listBoosters = Model.BoosterData.BoostersInfo;
            for(int q = 0; q < listBoosters.Count; ++q)
            {
                BoosterInfo booster = listBoosters[q];
                int ownedBCount = Model.PlayerData.GetOwnedBoosterCount(booster.Id);
                var compInfo = new BoosterItemComp.PresentInfo(booster.Id, 
                    _sprIcon: controller.View.GetBoosterSprite(booster.SpriteKey), 
                    booster.Name, booster.Desc,
                    booster.Cost.ToString(), ownedBCount.ToString());

                compListInfo.Add(compInfo);
            }
            boosterPnlViewCache.Refresh(new BoosterListPanelView.PresentInfo(compListInfo));
        }
        */
    }



}
