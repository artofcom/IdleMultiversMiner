using System;
using System.Collections.Generic;
using UnityEngine;

namespace App.GamePlay.Common
{
    [Serializable]
    public class  EnvironmentInfo
    {
        [SerializeField] string dataVersion;
        [SerializeField] string appVersion;
        [SerializeField] string deviceName;
        [SerializeField] long timeStamp;

        public EnvironmentInfo(string version)
        {
            this.dataVersion = version;     
            this.appVersion = Application.version;    this.deviceName = SystemInfo.deviceName;
            this.timeStamp = DateTime.UtcNow.Ticks;
        }
        public string DataVersion => dataVersion;
        public string DeviceName => deviceName;
        public long TimeStamp => timeStamp;
        public string AppVersion => appVersion;
    }

    [Serializable]
    public class MetaCurrency
    {
        [SerializeField] string type;
        [SerializeField] long amount;
        public MetaCurrency(string type, long amount) 
        {
            this.type = type; this.amount = amount;
        }
        public string Type => type;
        public long Amount { get=> amount; set => amount = value; }
    }

    [Serializable]
    public class MetaCurrencyBundle
    {
        [SerializeField] List<MetaCurrency> currencies = new List<MetaCurrency>();
        
        public void AddCurrency(MetaCurrency currency) {    currencies.Add(currency); }
        public MetaCurrency GetCurrency(string type) 
        {
            for(int q = 0; q < currencies.Count; q++) 
            {
                if(string.Compare(currencies[q].Type, type, ignoreCase:true) == 0)
                    return currencies[q];
            }
            return null;
        }
        public void SetCurrency(string type, long amount) 
        {
            MetaCurrency currency = GetCurrency(type);
            if(currency != null) 
                currency.Amount = amount;
            else
                currencies.Add(new MetaCurrency(type, amount));
        }
    }

    [Serializable]
    public class GameCardInfo
    {
        [SerializeField] string game_id;
        [SerializeField] string releaseDate;
        [SerializeField] string lastPlayedTimeStamp;
        [SerializeField] int resetCount;

        public GameCardInfo(string gameId)
        {
            this.game_id = gameId;
        }

        public string GameId => game_id;
        public string ReleaseDate => releaseDate;
        public string LastPlayedTimeStamp { get => lastPlayedTimeStamp; set => lastPlayedTimeStamp = value; }
        public int ResetCount { get => resetCount; set => resetCount = value; }
    }

    [Serializable]
    public class GameCardBundle
    {
        [SerializeField] List<GameCardInfo> cardsInfos = new List<GameCardInfo>();
        //public void AddCurrency(MetaCurrency currency) {    currencies.Add(currency); }
        public GameCardInfo GetCardInfo(string key) 
        {
            for(int q = 0; q < cardsInfos.Count; q++) 
            {
                if(string.Compare(cardsInfos[q].GameId, key, ignoreCase:true) == 0)
                    return cardsInfos[q];
            }
            return null;
        }

        public void UpdateGameCardInfo(GameCardInfo info)
        {
            if(cardsInfos == null)
                cardsInfos = new List<GameCardInfo>();

            var cardInfo = GetGameCardInfo(info.GameId);
            if(cardInfo == null)
                cardsInfos.Add(info);
            else
            {
                cardsInfos.Remove(cardInfo);
                cardsInfos.Add(cardInfo);
            }
        }
        public GameCardInfo GetGameCardInfo(string gameId)
        {
            if(cardsInfos == null)
                return null;

            for(int q = 0; q < cardsInfos.Count; ++q)
            {
                if(0 == string.Compare(cardsInfos[q].GameId, gameId, ignoreCase:true))
                    return cardsInfos[q];
            }
            return null;
        }
        public GameCardInfo GetGameCardInfo(int idx)
        {
            if(idx>=0 && idx<cardsInfos.Count)
                return cardsInfos[idx];
            return null;
        }
        public int GameCardCount()  { return cardsInfos==null ? 0 : cardsInfos.Count; }
    }
}