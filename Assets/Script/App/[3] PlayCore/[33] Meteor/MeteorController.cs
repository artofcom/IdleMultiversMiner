using System;
using UnityEngine;
using IGCore.MVCS;

namespace App.GamePlay.IdleMiner
{

    public class MeteorController : IGCore.MVCS.AController// AMinerModule
    {
        // public MeteorController(IdleMinerController controller) : base(controller) { }
        
        public MeteorController(AUnit unit, AView view, AModel model, AContext ctx)
            : base(unit, view, model, ctx)
        { }

        public override void Init() {   base.Init();    }
        protected override void OnViewEnable() { }
        protected override void OnViewDisable() { }

        public override void Resume(int awayTimeInSec) { }
        public override void Pump() { }
        public override void WriteData() { }

        /*
        public override void Pump()
        {
            if(Model.PlayerData.MeteorInfo == null || View.MeteorObject.activeSelf)
                return;

            long lastTick;
            if(long.TryParse(Model.PlayerData.MeteorInfo.LastRewardTime, out lastTick))
            {
                long elapsedTicks = PlayerData.UTCNowTick - lastTick;
                double seconds = elapsedTicks / 10000000; // 초 단위 변환

                if(seconds >= Model.MeteorData.ShowUpIntervalInMin*60)
                {
                    View.ShowMeteor(true);
                }
            }
        }







        protected override void InitModule()
        {
            View.ShowMeteor(false);

            IdleMinerView.EventOnBtnMeteorClicked += View_OnBtnMeteorClicked;
        }

        void View_OnBtnMeteorClicked()
        {
            View.TriggerMagicBall(View.MeteorObject.transform.position, data:null, (tempData) =>
            {
                var moneyAmount = Model.GetBonusCurrencyAmount(weight: Model.MeteorData.RewardMultiply);
                Debug.Log("TimedBonus Amount : " + moneyAmount.BIAmount);
                Model.PlayerData.AddMoney( moneyAmount );
                Model.PlayerData.RecordMeteorRewardedTime();

                View.ShowMeteor(false);
            });
        }
        */

    }
}