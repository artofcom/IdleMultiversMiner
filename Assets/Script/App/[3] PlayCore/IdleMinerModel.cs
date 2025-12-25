using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System;
using System.Numerics;
using System.IO;
using Core.Utils;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner
{
    internal class IdleMinerModel : IGCore.MVCS.AModel
    {
        CurrencyData CurrencyData { get; set; }
        public IdleMinerPlayerModel PlayerData => (IdleMinerPlayerModel)playerData;

        public IdleMinerModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData)  { }

        public override void Init(object data = null)
        {
            IdleMinerContext IMCtx = (IdleMinerContext)context;
            Assert.IsNotNull(IMCtx);

            LoadData((string)IMCtx.GetData("gamePath"));
            _isInitialized = true;
        }

        
        public void SavePlayerData()
        {
           PlayerData.SaveData();
        }

        public void EventOnFocused()
        {
           PlayerData.RefreshAwayTime();
        }

        public override void Dispose()
        {
            base.Dispose();
            _isInitialized = false;
        }

        public void Resume()
        {
            PlayerData.Resume();
        }

        void LoadData(string gamePath)
        {
            var textData = Resources.Load<TextAsset>(gamePath + "/Data/CurrencyData");
            CurrencyData = JsonUtility.FromJson<CurrencyData>(textData.text);

            Assert.IsNotNull(CurrencyData);
            Assert.IsTrue(CurrencyData.Currencies.Count == (int)eCurrencyType.MAX);
        }
    }
}