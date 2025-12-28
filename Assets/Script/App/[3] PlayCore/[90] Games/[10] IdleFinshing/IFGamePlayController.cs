using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.GamePlay;
using IGCore.MVCS;
using App.GamePlay.IdleMiner.Common;


namespace App.GamePlay.IdleMiner.Games.IdleFinishing
{
    //  SkillTree Controlling.-------------------------------------
    //
    public class IFGamePlayController : GamePlayController
    {
        // int tempValue;

        // Skill Sector.
        class TestTestTestSkill : App.GamePlay.IdleMiner.Common.ISkillBehavior
        {
            public void Learn(AController ctrler, string abilityParam)
            {
                IFGamePlayController ifGameCtrler = (IFGamePlayController)ctrler;

                // Accessable to private member.
                // ifGameCtrler.tempValue = 10;
                
                // Accessable to protected member from parent class.
              //  ifGameCtrler.bZoneInitizlized = ifGameCtrler.bZoneInitizlized;// true;
            }
        }

        public IFGamePlayController(AUnit unit, AView view, AModel model, AContext ctx)
            : base(unit, view, model, ctx)
        { }


        protected override void createSkillBehaviorInternal()
        {
            base.createSkillBehaviorInternal();

            dictSkillBehaviors.Add("NEW_SKILL_ID", new TestTestTestSkill());
        }


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