using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.Common.PlayerModel
{
    [Serializable]
    public class ResourceCollectInfo : ISerializationCallbackReceiver
    {
        [SerializeField] string RscId;
        [SerializeField] string CountX1000Str;
        [SerializeField] bool AutoSell;

        BigInteger _biCountX1000;
        public BigInteger BICountX1000              // Real Value x 1000.
        {
            get => _biCountX1000;
            set
            {
                _biCountX1000 = value;
                CountX1000Str = _biCountX1000.ToString();
                // Debug.Log("Resource cnt(x1000) has been udpated..." + CountX1000Str);
            }
        }
        public BigInteger BICount                   // Real Value. - Why we do this? Cuz values 0 ~ 0.999 need to get into our BigInteger calculation.
        {
            get { return BICountX1000 / 1000; }
        }
        public bool AutoSell_   { get => AutoSell;  set => AutoSell = value; } 
        public string RscId_    { get => RscId;     set => RscId = value; }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            BigInteger biCountx1000;
            bool ret = BigInteger.TryParse(CountX1000Str, out biCountx1000);
            if (ret)
            {
                biCountx1000 = biCountx1000 < BigInteger.Zero ? BigInteger.Zero : biCountx1000;
                BICountX1000 = biCountx1000;
            }
            Assert.IsTrue(ret, "Parse Error! " + CountX1000Str);
        }
    }

}
