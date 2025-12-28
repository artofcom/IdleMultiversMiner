using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.GamePlay;
using IGCore.MVCS;

namespace App.GamePlay.IdleMiner.Games.IdleHellBoy
{
    //  SkillTree Controlling.-------------------------------------
    //
    public class HBGamePlayController : GamePlayController
    {
         public HBGamePlayController(AUnit unit, AView view, AModel model, AContext ctx)
            : base(unit, view, model, ctx)
        { }

        /*
        protected override void InitController() 
        {
            base.InitController();
        }

        protected override void OnViewEnable()
        {
            base.OnViewEnable();
        }
        protected override void OnViewDisable()
        {
            base.OnViewDisable();
        }

        public override void Resume(int awayTimeInSec)
        {
            base.Resume(awayTimeInSec);
        }
        public override void Pump() 
        {
            base.Pump();
        }
        public override void WriteData() 
        {
            base.WriteData();
        }*/
    }
}