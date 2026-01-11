using App.GamePlay.Common;
using App.GamePlay.IdleMiner.Common.Model;
using App.GamePlay.IdleMiner.Common.Types;
using Core.Events;
using Core.Utils;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using IGCore.PlatformService;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class PlayerMoneyData
    {
        [SerializeField] List<CurrencyAmount> money = null;

        public List<CurrencyAmount> Money => money;
        public void Init()
        {
            if (money == null)
                money = new List<CurrencyAmount>();

            for (int q = 0; q < money.Count; ++q)
                money[q].Init();
        }
    }

    [Serializable]
    public class NewPlayerData
    {
        [SerializeField] PlayerMoneyData currency;
        [SerializeField] int zoneId;
        [SerializeField] List<string> openedTabBtns;

        public PlayerMoneyData Currency => currency;
        public int ZoneId => zoneId;
        public List<string> OpenedTabBtns => openedTabBtns;

        public static NewPlayerData LoadFromJson(string jsonFileName)
        {
            var textData = Resources.Load<TextAsset>(jsonFileName);
            NewPlayerData ret = JsonUtility.FromJson<NewPlayerData>(textData.text);
            ret.Currency.Init();
            return ret;
        }
    }


    internal class IdleMinerPlayerModel : MultiGatewayWritablePlayerModel
    {
        public const string PREFAB_ACCOUNT = "RecentAccount";

        // Serialize Fields.
        //[SerializeField] List<CurrencyAmount> money;
        //[SerializeField] string dateTime;



        // string MODEL_KEY = nameof(IdleMinerPlayerModel);

        #region ===> Properties.

        PlayerMoneyData moneyData = new PlayerMoneyData();
        string timeStamp;
        NewPlayerData newPlayerData;
        List<string> listOpenedTabBtn = new List<string>();

        bool mIsDataLoaded = false;
        IdleMinerContext IMCTX => (IdleMinerContext)context;

        // Accessor.
        public List<CurrencyAmount> Money { get; private set; }
        public bool IsNewPlayer { get; private set; } = false;
        public List<string> OpenedTabBtns => listOpenedTabBtn;

        //
        double awayTimeInSec = 1;
        EventsGroup Events = new EventsGroup();

        const string KEY_TIME_TEAK = "TweakedDateTick";
        static long sTweakedDateTick = 0;
        static long sAppStartTick = 0;
        public int IdleAwayTime => (int)awayTimeInSec;

        public static long UTCNowTick
        {
            get
            {
#if UNITY_EDITOR
                if(sTweakedDateTick > 0)
                    return sTweakedDateTick + (DateTime.UtcNow.Ticks - sAppStartTick);
                else 
#endif
                return DateTime.UtcNow.Ticks;
            }
        }

        string DataKey_TimeStamp => $"{nameof(IdleMinerPlayerModel)}_TimeStamp";
        string DataKey_MoneyData => $"{nameof(IdleMinerPlayerModel)}_MoneyData";
        string DataKey_OpenedTabBtns => $"{nameof(IdleMinerPlayerModel)}_OpenedTabBtns";
        string DataPath_NewPlayer => "/Data/NewPlayerData";
#endregion


        #region ===> Interfaces

        public IdleMinerPlayerModel(AContext ctx, List<IDataGatewayService> gatewayService) : base(ctx, gatewayService) { }
        

        public double FlushAwayTime()
        {
            double ret = awayTimeInSec;
            awayTimeInSec = 1;
            return ret;
        }

        public void RefreshAwayTime()
        {            
            const int _10Hour = 60 * 60 * 10;
            if (!string.IsNullOrEmpty(timeStamp))
            {
                long lastTick;
                if (long.TryParse(timeStamp, out lastTick))
                {
                    long elapsedTicks = UTCNowTick - lastTick;
                    TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                    awayTimeInSec = Math.Min(_10Hour, elapsedSpan.TotalSeconds);

                    Debug.Log("[Resume] Away Idle Time in Sec " + awayTimeInSec.ToString());
                }
            }
            timeStamp = UTCNowTick.ToString();
        }

        //public void SaveData()
        //{
            // IMCTX.SavePlayerData();

            //SaveTimeStamp();
            //SaveMoneyData();
            //SaveOpenedTabBtns();
            //PlayerPrefs.Save();
        //}

        public override void Dispose()
        {
            UpdateGameCardsTimeStamp();

            base.Dispose(); 

            UnRegisterRequestables();
        }

        public void Resume()
        {
            if(context.IsSimulationMode())
            {
                for(int q = 0; q < moneyData.Money.Count; ++q)
                {
                    CurrencyAmount amount = moneyData.Money[q];
                    Debug.Log($"<color=yellow>[SIM][Status] Currency Type:{amount.Type}, amount:{amount.BIAmount.ToAbbString()}</color>");
                }
            }
        }


        #region ===> Money Control

        //==========================================================================
        //
        // Money Control
        //
        //
        public BigInteger GetMoney(eCurrencyType currencyType)
        {
            CurrencyAmount amount = GetCurrencyAmount(currencyType);
            if(amount != null)
                return amount.BIAmount;

            return BigInteger.Zero;
        }
        public CurrencyAmount GetCurrencyAmount(eCurrencyType currencyType)
        {
            if(currencyType == eCurrencyType.IAP_COIN)
                return new CurrencyAmount(((long)context.RequestQuery("AppPlayerModel", "GetIAPCurrency")).ToString(), eCurrencyType.IAP_COIN);

            if (moneyData.Money == null)
                return null;

            for (int q = 0; q < moneyData.Money.Count; ++q)
            {
                if (moneyData.Money[q].Type == currencyType)
                    return moneyData.Money[q];
            }
            return null;
        }
        public bool IsAffordable(CurrencyAmount cost)
        {
            return IsAffordableCurrency(cost.BIAmount, cost.Type);
        }
        bool IsAffordableCurrency(BigInteger cost, eCurrencyType currencyType)
        {
            return GetMoney(currencyType) >= cost;
        }
        public void AddMoney(CurrencyAmount amount)
        {
            UpdateMoney(amount.BIAmount, amount.Type);

            EventSystem.DispatchEvent(EventID.GAME_CURRENCY_UPDATED, GetCurrencyAmount(amount.Type));
        }


        #endregion


        #endregion


        #region IWritableModel
        public override List<Tuple<string, string>> GetSaveDataWithKeys()
        {
            List<Tuple<string, string>> listDataSet = new List<Tuple<string, string>>();
            
            SaveMoneyData(listDataSet);
            SaveTimeStamp(listDataSet);
            SaveOpenedTabBtns(listDataSet);

            return listDataSet;
        }
        #endregion



        #region ===> Helpers


        // Note : Data Load should be done before call init.
        // public override void Init() { } //List<string> listStartSkills)
        public override void Init()
        {
            base.Init();
            
            InitPlayerModel();
        }

        void InitPlayerModel()
        {
            string gamePath = (string)IMCTX.GetData("gamePath");
            timeStamp = string.Empty;

#if UNITY_EDITOR
            string strTweakedDateTick = IMCTX.IsSimulationMode() ? string.Empty : PlayerPrefs.GetString(KEY_TIME_TEAK, string.Empty);

            if(!string.IsNullOrEmpty(strTweakedDateTick))
            {
                long.TryParse(strTweakedDateTick, out sTweakedDateTick);
                Debug.Log("[TimeTweaker] Using Tweaked Time..." + new DateTime(sTweakedDateTick).ToString());
            }
            else 
                Debug.Log("[TimeTweaker] NewPlayer : Using Real Time..." + DateTime.UtcNow.ToString());

            sAppStartTick = DateTime.UtcNow.Ticks;
#endif
            LoadNewPlayer(gamePath);

            LoadData();
            
            IsNewPlayer = string.IsNullOrEmpty(timeStamp);
            
            if(IsNewPlayer)
                moneyData = newPlayerData.Currency;

            Assert.IsNotNull(moneyData);
            moneyData.Init();

            // Logger.
            for(int q = 0; q < moneyData.Money.Count; ++q)
                Debug.Log($"Initializing..... Currency Data : {moneyData.Money[q].Type} : {moneyData.Money[q].BIAmount}");
            
            context.AddData("DefaultZoneId", newPlayerData.ZoneId);

            // PlayerPrefs.SetString(PREFAB_ACCOUNT, IdleMinerContext.AccountName);

            RefreshAwayTime();

            RegisterRequestables();

            UpdateGameCardsTimeStamp();

            IsInitialized = true;
        }
        
        void UpdateGameCardsTimeStamp()
        {
            var gameCardInfo = (GameCardInfo)context.RequestQuery("AppPlayerModel", "GetGameCardInfo", IdleMinerContext.GameKey);
            if(gameCardInfo == null)
                gameCardInfo = new GameCardInfo(IdleMinerContext.GameKey);
            gameCardInfo.LastPlayedTimeStamp = UTCNowTick.ToString();
            if(string.IsNullOrEmpty(gameCardInfo.FirstPlayedTimeStamp))
                gameCardInfo.FirstPlayedTimeStamp = UTCNowTick.ToString();
            context.RequestQuery("AppPlayerModel", "UpdateGameCardInfo", gameCardInfo);
        }

        void RegisterRequestables()
        {
            context.AddRequestDelegate("IdleMiner", "IsAffordableCurrency", isAffordableCurrency);
            context.AddRequestDelegate("IdleMiner", "IsNewPlayer", isNewPlayer);
            context.AddRequestDelegate("IdleMiner", "AddMoney", addMoney);
            context.AddRequestDelegate("IdleMiner", "GetMoney", getMoney);
        }
        void UnRegisterRequestables()
        {
            context.RemoveRequestDelegate("IdleMiner", "IsAffordableCurrency");
            context.RemoveRequestDelegate("IdleMiner", "IsNewPlayer");
            context.RemoveRequestDelegate("IdleMiner", "AddMoney");
            context.RemoveRequestDelegate("IdleMiner", "GetMoney");
        }
        object isAffordableCurrency(params object[] data)
        {
            if(data.Length < 2)
                return null;

            BigInteger cost = (BigInteger)data[0];
            eCurrencyType type = (eCurrencyType)data[1];    
            return IsAffordableCurrency(cost, type);
        }
        object addMoney(params object[] data)
        {
            if(data.Length < 1)
                return null;

            CurrencyAmount amount = (CurrencyAmount)data[0];
            AddMoney(amount);
            return null;
        }
        object getMoney(params object[] data) 
        {
            if(data.Length < 1) 
                return null;

            eCurrencyType type = (eCurrencyType)data[0];
            return GetMoney(type);
        }
        object isNewPlayer(params object[] data)
        {
            return IsNewPlayer;
        }
        void UpdateMoney(BigInteger _money, eCurrencyType currencyType, bool offset = true)
        {
            if(currencyType == eCurrencyType.IAP_COIN)
            {
                context.RequestQuery("AppPlayerModel", "UpdateIAPCurrency", (int)_money, offset);
                return;
            }

            bool updated = false;
            for (int q = 0; q < moneyData.Money.Count; ++q)
            {
                if (moneyData.Money[q].Type == currencyType)
                {
                    updated = true;
                    moneyData.Money[q].Update(_money, offset);
                    break;
                }
            }

            if(!updated)
            {
                var newMoney = new CurrencyAmount(_money.ToString(), currencyType);
                moneyData.Money.Add(newMoney);
            }
            SetDirty();
        }
        
        void LoadTimeStamp()
        {
            int idxGatewayService = (context as IdleMinerContext).TargetGameDataGatewayServiceIndex;
            FetchData<string>(idxGatewayService, DataKey_TimeStamp, out timeStamp, fallback:string.Empty);
        }

        void LoadMoneyData()
        {
            int idxGatewayService = (context as IdleMinerContext).TargetGameDataGatewayServiceIndex;
            FetchData<PlayerMoneyData>(idxGatewayService, DataKey_MoneyData, out moneyData, fallback:new PlayerMoneyData());
        }

        void SaveMoneyData(List<Tuple<string, string>> listDataSet)
        {
            Assert.IsNotNull(moneyData);
            listDataSet.Add(new Tuple<string, string>(DataKey_MoneyData, JsonUtility.ToJson(moneyData)));
        }
        void SaveTimeStamp(List<Tuple<string, string>> listDataSet)
        {
            timeStamp = UTCNowTick.ToString();
            listDataSet.Add(new Tuple<string, string>(DataKey_TimeStamp, timeStamp));
        }

        void SaveOpenedTabBtns(List<Tuple<string, string>> listDataSet)
        {
            string fullData = string.Empty;
            for(int q = 0; q < listOpenedTabBtn.Count; ++q)
            {
                fullData += listOpenedTabBtn[q];
                if(q < listOpenedTabBtn.Count - 1)
                    fullData += ":";
            }
            // WriteFileInternal(DataKey_OpenedTabBtns, fullData, false);
            listDataSet.Add(new Tuple<string, string>(DataKey_OpenedTabBtns, fullData));
        }
        void LoadOpenedTabBtns()
        {
            int idxGatewayService = (context as IdleMinerContext).TargetGameDataGatewayServiceIndex;
            string textData = string.Empty;
            FetchData<string>(idxGatewayService, DataKey_OpenedTabBtns, out textData, fallback:string.Empty);
            
            listOpenedTabBtn.Clear();
            string[] tabNames = string.IsNullOrEmpty(textData) ? null : textData.Split(':');
            if(tabNames==null || tabNames.Length == 0)
            {
                for(int q = 0; q < newPlayerData.OpenedTabBtns.Count; ++q)
                    listOpenedTabBtn.Add(newPlayerData.OpenedTabBtns[q]);
                
               // SaveOpenedTabBtns();
                return;
            }
            
            for(int q = 0; q < tabNames.Length; ++q)
                listOpenedTabBtn.Add(tabNames[q]);
        }

        void LoadNewPlayer(string gamePath)
        {
            Assert.IsTrue(!string.IsNullOrEmpty(gamePath));
            newPlayerData = NewPlayerData.LoadFromJson(gamePath + DataPath_NewPlayer);
        }

        void LoadData()
        {
            if(!mIsDataLoaded && !context.IsSimulationMode())
            {
                LoadTimeStamp();

                LoadMoneyData();

                LoadOpenedTabBtns();
            }
        }
        #endregion


        public static void ClearAllData()
        {
            //GamePlayPlayerModel.ClearPlanetData();
            //ResourcePlayerModel.ClearResourceData();
            //CraftPlayerModel.ClearCraftData();
            //SkillTreePlayerModel.ClearSkillTreeData();

        //    ClearCraftData();
       //     ClearManagerData();
       //     ClearPlanetData();
         //   ClearResourceData();
        //    ClearSkillTreeData();
         //   ClearBoosterData();
        }

#if UNITY_EDITOR

        //==========================================================================
        //
        // Editor - Reset Data Prefab
        //
        [UnityEditor.MenuItem("PlasticGames/Clear PlayerData/Main")]
        public static void EditorClearPlayerData()
        {
            // ClearPlayerData();
        }

        //==========================================================================
        //
        // Editor - Time Tweaker Tools.
        //
        [UnityEditor.MenuItem("PlasticGames/Tweak Time Tick/Set to 1 day ago")]
        private static void TimeTweak01DayAgo()      {   TweakTime(-1);  }
        [UnityEditor.MenuItem("PlasticGames/Tweak Time Tick/Set to 5 day ago")]
        private static void TimeTweak05DayAgo()      {   TweakTime(-5);  }
        [UnityEditor.MenuItem("PlasticGames/Tweak Time Tick/Set to 1 day forward")]
        private static void TimeTweak01DayForward()  {   TweakTime(1);   }
        [UnityEditor.MenuItem("PlasticGames/Tweak Time Tick/Set to 5 day forward")]
        private static void TimeTweak05DayForward()  {   TweakTime(5);   }

        [UnityEditor.MenuItem("PlasticGames/Tweak Time Tick/Clear Time Tweak")]
        private static void ClearTimeTweak()
        {
            PlayerPrefs.SetString(KEY_TIME_TEAK, string.Empty);
            sTweakedDateTick = 0;

            Debug.Log("[TimeTweaker] Time Tweak has been cleared.");
        }
        static void TweakTime(int day)
        {
            long tweakedTick = 0;
            string strTweakTick = PlayerPrefs.GetString(KEY_TIME_TEAK, string.Empty);
            if (!string.IsNullOrEmpty(strTweakTick))
                long.TryParse(strTweakTick, out tweakedTick);
            
            else
                tweakedTick = DateTime.UtcNow.Ticks;
            
            DateTime aDayAgo = new DateTime(tweakedTick);
            aDayAgo = aDayAgo.AddDays( day );
            PlayerPrefs.SetString(KEY_TIME_TEAK, aDayAgo.Ticks.ToString());

            Debug.Log("[TimeTweaker] Time Tweaked to " + aDayAgo.ToString());

            sTweakedDateTick = aDayAgo.Ticks;
        }
#endif
    }    
}