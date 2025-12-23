using App.GamePlay.IdleMiner.Common.Model;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using Core.Util;
using Core.Utils;
using IGCore.MVCS;
using System;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    public class TopUICompController : AController
    {
        TopUIComp topUIView;

        EventsGroup Events = new EventsGroup();

        public TopUICompController(AView view, AModel model, AContext context) : base(view, model, context)
        {  
            topUIView = view as TopUIComp;
        }

        public override void Init() {}
        protected override void OnViewEnable() 
        {
            Debug.Log("[TopUICompController] : On View Enabled !!!");

            if(topUIView == null)
                topUIView = view as TopUIComp;

            Events.RegisterEvent(EventID.IAP_MONEY_CHANGED, EventOnMainCurrencyChanged);
            Events.RegisterEvent(EventID.STAR_AMOUNT_CHANGED, EventOnMainCurrencyChanged);
            //Events.RegisterEvent(EventID.GAME_CURRENCY_UPDATED, EventOnGameCurrencyUpdated);
            Events.RegisterEvent(EventID.GAME_RESET_REFRESH, EventOnRefreshView);
            
            DelayedAction.TriggerActionWithDelay(view, 0.01f, () => { RefreshView(); } );
        }
        protected override void OnViewDisable() 
        { 
            Events.UnRegisterEvent(EventID.IAP_MONEY_CHANGED, EventOnMainCurrencyChanged);
            Events.UnRegisterEvent(EventID.STAR_AMOUNT_CHANGED, EventOnMainCurrencyChanged);
            Events.UnRegisterEvent(EventID.GAME_RESET_REFRESH, EventOnRefreshView);
            //Events.UnRegisterEvent(EventID.GAME_CURRENCY_UPDATED, EventOnGameCurrencyUpdated);
        }
        public override void Resume(int awayTimeInSec) { }
        public override void Pump() { }
        public override void WriteData() { }

        void EventOnMainCurrencyChanged(object data)
        {
            RefreshView();
        }

        void EventOnRefreshView(object data) 
        {
            RefreshView();
        }

        /*
        void EventOnGameCurrencyUpdated(object data)
        {
            CurrencyAmount currencyAmount = data as CurrencyAmount;
            Assert.IsNotNull(currencyAmount);

            switch(currencyAmount.Type)
            {
            case eCurrencyType.MINING_COIN:
                goldAmount = currencyAmount.BIAmount.ToAbbString();
                RefreshView();
                break;
            default:
                break;
            }
        }*/

        void RefreshView()
        {
            long iapAmount = (long)context.RequestQuery("AppPlayerModel", "GetIAPCurrency");
            long starAmount = (long)context.RequestQuery("AppPlayerModel", "GetStarCurrency");
            object objGold = context.RequestQuery("IdleMiner", "GetMoney", eCurrencyType.MINING_COIN);
            string goldAmount = string.Empty;
            if(objGold != null) 
                goldAmount = ((BigInteger)objGold).ToAbbString();
            topUIView.Refresh(new TopUIComp.PresentInfo(goldAmount, iapAmount.ToString(), starAmount.ToString()));
        }
    }



    public class TopUIComp : AView
    {
        [SerializeField] TMP_Text GoldAmount, IAPAmount, StarAmount;

        //  Events ----------------------------------------
        //
        //EventsGroup Events = new EventsGroup();
        public Action EventOnBtnOptionClicked;
        public Action EventOnBtnShopClicked;
        public Action EventOnBtnTimedBonusClicked;
        public Action EventOnBtnDailyAttendClicked;
        public Action EventOnBtnTaskClicked;


        public class PresentInfo : APresentor
        {
            public PresentInfo(string amountGold, string amountIAP_Currency, string amountStar)
            {
                AmountGold  = amountGold;
                AmountIAP   = amountIAP_Currency;
                AmountStar  = amountStar;
            }   

            public string AmountGold        { get; private set; }
            public string AmountIAP         { get; private set; }
            public string AmountStar        { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            // Events.RegisterEvent(PlayerData.EVENT_MONEY_UPDATED, OnMoneyUpdated);
        }

        public override void Refresh(APresentor presentor)
        {
            PresentInfo info = presentor as PresentInfo;
            if(info == null)     return;

            if(GoldAmount != null)
                GoldAmount.text = info.AmountGold;
            
            if(IAPAmount != null)
                IAPAmount.text = info.AmountIAP;

            if(StarAmount != null)
                StarAmount.text = info.AmountStar;
        }


        //  Events ----------------------------------------
        //
        
        public void OnSettingClicked()
        {
            EventOnBtnOptionClicked?.Invoke();
        }
        public void OnInfoClicked()
        {

        }
        public void OnShopClicked()
        {
            EventOnBtnShopClicked?.Invoke();
        }
        public void OnBtnTaskClicked()
        {
            EventOnBtnTaskClicked?.Invoke();
        }
        public void OnTimedBonusClicked()
        {
            EventOnBtnTimedBonusClicked?.Invoke();
        }
        public void OnDailyAttendClicked()
        {
            EventOnBtnDailyAttendClicked?.Invoke();
        }
    }
}