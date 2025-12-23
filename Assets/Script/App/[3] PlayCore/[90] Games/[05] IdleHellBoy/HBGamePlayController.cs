using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.GamePlay;


namespace App.GamePlay.IdleMiner.Games.IdleHellBoy
{
    //  SkillTree Controlling.-------------------------------------
    //
    public class HBGamePlayController : GamePlayController
    {
         public HBGamePlayController(IGCore.MVCS.AView view, IGCore.MVCS.AModel model, IGCore.MVCS.AContext ctx)
            : base(view, model, ctx)
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