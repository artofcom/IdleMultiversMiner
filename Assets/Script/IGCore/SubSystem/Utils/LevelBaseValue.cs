using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using System;
using UnityEngine.Assertions;

namespace Core.Utils
{
    [Serializable]
    public class LevelBasedFloat
    {
        [SerializeField] float defaultValue;      // value at level 1.
        [SerializeField] int increasePercent;     // ex) 110 (%)
        [SerializeField] float increaseBase;


        // Accessor.
        public float DefaultValue => defaultValue;
        public int IncreasePercent => increasePercent;
        public float IncreaseBase => increaseBase;

        public void Convert() {}

        public LevelBasedFloat(float _defaultValue, int _increasePercent, float _increaseBase)
        {
            defaultValue = _defaultValue;
            increasePercent = _increasePercent;
            increaseBase = _increaseBase;
        }
        public LevelBasedFloat(string inputData)
        {
            // "base=0.25:incPercent=10:incBase=1",
            //
            Assert.IsTrue(!string.IsNullOrEmpty(inputData));

            inputData = inputData.Replace(" ", "");
            string[] strValues = inputData.Split(":");
            Assert.IsTrue(strValues.Length == 3);
            for(int q = 0; q < strValues.Length; ++q)
            {
                string[] strKeyAndValues = strValues[q].Split("=");
                Assert.IsTrue(strKeyAndValues.Length == 2);
                if (strKeyAndValues[0] == "base")           defaultValue = float.Parse(strKeyAndValues[1]);
                else if (strKeyAndValues[0] == "incPercent")increasePercent = (int)( float.Parse(strKeyAndValues[1]) );
                else if (strKeyAndValues[0] == "incBase")   increaseBase = float.Parse(strKeyAndValues[1]);
            }
        }

        public float Value(int level)   // level :  1 ~ 
        {
            --level;
            float increasedAmount = IncreaseBase * IncreasePercent * ((float)level);
            increasedAmount *= 0.01f;
            return DefaultValue + increasedAmount;
        }
    }




    [Serializable]
    public class LevelBasedInt
    {
        [SerializeField] int defaultValue;      // value at level 1.
        [SerializeField] int increasePercent;     // ex) 110 (%)
        [SerializeField] int increaseBase;


        // Accessor.
        public int DefaultValue => defaultValue;
        public int IncreasePercent => increasePercent;
        public int IncreaseBase => increaseBase;

        public void Convert() {}

        public LevelBasedInt(int _defaultValue, int _increasePercent, int _increaseBase)
        {
            defaultValue = _defaultValue;
            increasePercent = _increasePercent;
            increaseBase = _increaseBase;
        }
        public LevelBasedInt(string inputData)
        {
            // "base=0.25:incPercent=10:incBase=1",
            //
            Assert.IsTrue(!string.IsNullOrEmpty(inputData));

            inputData = inputData.Replace(" ", "");
            string[] strValues = inputData.Split(":");
            Assert.IsTrue(strValues.Length == 3);
            for(int q = 0; q < strValues.Length; ++q)
            {
                string[] strKeyAndValues = strValues[q].Split("=");
                Assert.IsTrue(strKeyAndValues.Length == 2);
                if (strKeyAndValues[0] == "base")           defaultValue = int.Parse(strKeyAndValues[1]);
                else if (strKeyAndValues[0] == "incPercent")increasePercent = int.Parse(strKeyAndValues[1]);
                else if (strKeyAndValues[0] == "incBase")   increaseBase = int.Parse(strKeyAndValues[1]);
            }
        }

        public int Value(int level)   // level :  1 ~ 
        {
            --level;
            float increasedAmount = IncreaseBase * IncreasePercent * ((float)level);
            increasedAmount *= 0.01f;
            return (int)( DefaultValue + increasedAmount );
        }
    }





    [Serializable]
    public class LevelBasedBigInteger : IBigIntegerConverter
    {
        [SerializeField] string defaultValue;
        [SerializeField] int increasePercent;
        [SerializeField] string increaseBase;

        // Accessor.
        public string DefaultValue => defaultValue;
        public int IncreasePercent => increasePercent;
        public string IncreaseBase => increaseBase;

        public BigInteger BIDefaultValue { get; private set; }
        public BigInteger BIIncreaseBase { get; private set; }

        public void Convert()
        {
            BigInteger biDefaultValue;
            bool ret = BigInteger.TryParse(DefaultValue, out biDefaultValue);
            if (ret) BIDefaultValue = biDefaultValue;
            else
            {
                Assert.IsTrue(false, DefaultValue);
            }
            BigInteger biIncreaseBase;
            ret = BigInteger.TryParse(IncreaseBase, out biIncreaseBase);
            if (ret) BIIncreaseBase = biIncreaseBase;
            else
            {
                Assert.IsTrue(ret, IncreaseBase);
            }
        }

        public LevelBasedBigInteger(BigInteger _defaultValue, int _increasePercent, BigInteger _increaseBase)
        {
            BIDefaultValue = _defaultValue;
            increasePercent = _increasePercent;
            BIIncreaseBase = _increaseBase;
        }
        public LevelBasedBigInteger(string inputData)
        {
            //
            // "base=0.25:incPercent=10:incBase=1",
            //
            Assert.IsTrue(!string.IsNullOrEmpty(inputData));

            inputData = inputData.Replace(" ", "");
            string[] strValues = inputData.Split(":");
            Assert.IsTrue(strValues.Length == 3);
            for (int q = 0; q < strValues.Length; ++q)
            {
                string[] strKeyAndValues = strValues[q].Split("=");
                Assert.IsTrue(strKeyAndValues.Length == 2);
                if (strKeyAndValues[0] == "base")           defaultValue = strKeyAndValues[1];
                else if (strKeyAndValues[0] == "incPercent")increasePercent = int.Parse(strKeyAndValues[1]);
                else if (strKeyAndValues[0] == "incBase")   increaseBase = strKeyAndValues[1];
            }
            Convert();
        }

        public BigInteger Value(int level)
        {
            --level;
            BigInteger increasedAmount = BIIncreaseBase * IncreasePercent * level;
            increasedAmount /= 100;
            return BIDefaultValue + increasedAmount;
        }
    }
}
