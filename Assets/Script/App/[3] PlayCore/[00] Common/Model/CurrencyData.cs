using System.Collections.Generic;
using System.Numerics;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.Common.Model
{
    [Serializable]
    public class CurrencyInfo
    {
        [SerializeField] string spriteKey;

        // Runtime.
        public string SpriteKey => spriteKey;
    }

    [Serializable]
    public enum eCurrencyType { MINING_COIN, IAP_COIN, GALAXY_COIN, ADS_COIN, MAX };

    [Serializable]
    public class CurrencyData
    {
        [SerializeField] List<CurrencyInfo> currencies;

        // Accessor.
        public List<CurrencyInfo> Currencies => currencies;
    }

    [Serializable]
    public class CurrencyAmount
    {
        [SerializeField] string amount;       // for Mining Currency.
        [SerializeField] eCurrencyType type;

        public BigInteger BIAmount { get; private set; } = BigInteger.Zero;
        public eCurrencyType Type => type;

        long iAmount;

        public CurrencyAmount() { }
        public CurrencyAmount(string _amount, eCurrencyType _type)
        {
            amount = _amount;   type = _type;
            Init();
        }

        public void Init()
        {
            if (!string.IsNullOrEmpty(amount))
            {
                BigInteger biValue = BigInteger.Zero;
                bool ret = BigInteger.TryParse(amount, out biValue);
                Assert.IsTrue(ret);
                if (ret) BIAmount = biValue;
            }
        }
        public void Update(BigInteger _amount, bool isOffset)
        {
            if (isOffset) BIAmount += _amount;
            else BIAmount = _amount;

            BIAmount = BIAmount < BigInteger.Zero ? BigInteger.Zero : BIAmount;

            amount = BIAmount.ToString();
        }
    }


}