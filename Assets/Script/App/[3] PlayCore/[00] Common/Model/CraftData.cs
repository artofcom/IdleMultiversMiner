
using System.Numerics;
//using UnityEngine;
//using Core.Events;
//using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using UnityEngine;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class Requirement {}


    [Serializable]
    public class ResourceRequirement : Requirement
    {
        [SerializeField] string resourceId;
        [SerializeField] int count;

        public string ResourceId => resourceId;
        //public int Count => count;

        public ResourceRequirement(string rscId, int count)
        {
            this.resourceId = rscId;    this.count = count;
        }
        public int GetCount(float fReqCountBuff = 1.0f)
        {
            int ret = (int)( ((float)count) * fReqCountBuff );
            return ret<=0 ? 1 : ret;
        }
    }

    [Serializable]
    public class RecipeInfo : ISerializationCallbackReceiver
    {
        [SerializeField] string id;
        [SerializeField] List<ResourceRequirement> sources;    // 1 ~ 3
        [SerializeField] string outcomeId;
        [SerializeField] int duration;            // sec.
        [SerializeField] string cost = "0";


        // Accessor.
        public string Id        { get => id; set => id = value; }
        public List<ResourceRequirement> Sources { get => sources; set => sources = value; }    // 1 ~ 3
        public string OutcomeId {  get => outcomeId; set => outcomeId = value; }
        public int Duration     {  set => duration = value; }           // sec.
        public string Cost      {  get => cost; set => cost = value; }

        // Runtime Data.
        public eRscStageType eTargetRscLevel { get; set; } = eRscStageType.eMax;
        public BigInteger BICost { get; private set; }

        public RecipeInfo(string id, string outcomeId, List<ResourceRequirement> listSrc)
        {
            this.id = id;   this.outcomeId = outcomeId;
            this.sources = listSrc;
        }
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            BigInteger biCost;
            bool ret = BigInteger.TryParse(Cost, out biCost);
            if (ret) BICost = biCost;

            Assert.IsTrue(ret, "Cost should be a BigInteger");
        }
        public int GetDuration(float timeBuff)
        {
            return (int)( ((float)duration) * timeBuff );
        }

#if UNITY_EDITOR

#endif
    }

    [Serializable]
    public class CraftData : ISerializationCallbackReceiver
    {
        public const int MAX_SLOT = 50;

        [SerializeField] List<RecipeInfo> recipes;
        [SerializeField] List<string> slotCosts;

        public List<RecipeInfo> Recipes {  get => recipes; set => recipes = value; }
        public List<string> SlotCosts   {  get => slotCosts; set => slotCosts = value; }
    

        public CraftData(List<RecipeInfo> listRecipe)
        {
            recipes = listRecipe;
        }

        // RT data.
        public List<BigInteger> BISlotCosts { get; private set; } = new List<BigInteger>();
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            if(BISlotCosts == null )    BISlotCosts = new List<BigInteger>();
            BISlotCosts.Clear();
            for (int q = 0; q < SlotCosts.Count; ++q)
            {
                BigInteger biValue;
                BigInteger.TryParse(SlotCosts[q], out biValue);
                BISlotCosts.Add(BigInteger.Zero + biValue);
            }
        }
    }
}
