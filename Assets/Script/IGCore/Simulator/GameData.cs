using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace IGCore.Simulator.GameData
{
    [Serializable]
    public class Resource
    {
        public enum Class
        {
            Material, Component, Item
        }

        public string id;
        [SerializeField] string sellPrice;
        
        public BigInteger GetSellPrice()
        {
            return BigInteger.Parse(sellPrice);
        }
        
        public void SetSellPrice(BigInteger value)
        {
            sellPrice = value.ToString();
        }
        public Class eClass;
    }

    [Serializable]
    public class ResourceAmount
    {
        public string resourceId;
        public string amount;
        
        public BigInteger GetAmount()
        {
            return BigInteger.Parse(amount);
        }
    }

    [Serializable]
    public class Planet
    {
        [Serializable]
        public class Obtainables
        {
            public string resourceId;
            public float rate;
        }
        [Serializable]
        public class LevelFloat
        {
            public float GetValue(int level)
            {
                return baseValue + increaseValue * ((float)level);
            }
            public float baseValue;
            public float increaseValue;
        }

        [Serializable]
        public class LevelBigInt
        {
            public BigInteger GetValue(int level)
            {
                var baseVal = BigInteger.Parse(baseValue);
                var increaseVal = BigInteger.Parse(increaseValue);
                return baseVal + increaseVal * level;
            }
            public string baseValue;
            public string increaseValue;
        }

        public string id;
        public List<Obtainables> obtainables;
        public LevelFloat performances; 
        public LevelBigInt upgradeCost;
    }

    [Serializable]
    public class CraftRecipe
    {
        public string id;
        public List<ResourceAmount> sources;
        public string outputId;
        public int duration;
    }

    [Serializable]
    public class SkillNode
    {
        public string id;
        public List<ResourceAmount> sources;
        public string feature;
        public float effectRate;
        public List<int> unlockPlanets;
    }

    [Serializable]
    public class GameData
    {        
        public List<Resource> resources;
        public List<Planet> planets;
        public List<CraftRecipe> compRecipes;
        public List<CraftRecipe> itemRecipes;
        public List<SkillNode> skillTree;
    }
}