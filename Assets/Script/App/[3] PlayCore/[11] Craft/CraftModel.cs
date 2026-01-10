
using System.Numerics;
//using UnityEngine;
//using Core.Events;
//using Core.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.Assertions;
using UnityEngine;
using App.GamePlay.IdleMiner.Common.Model;
using System.Collections;
using Unity.Collections;

namespace App.GamePlay.IdleMiner.Craft
{
    public class CraftModel : IGCore.MVCS.AModel
    {
        public CraftData CompCraftData { get; private set; }
        public CraftData ItemCraftData { get; private set; }

        public CraftPlayerModel PlayerData => (CraftPlayerModel)playerData;

        public CraftModel(IGCore.MVCS.AContext ctx, IGCore.MVCS.APlayerModel playerData) : base(ctx, playerData) 
        { }
           

        public override void Init(object data = null)
        {
            base.Init(data);

            IdleMinerContext IMCtx = (IdleMinerContext)context;
            Assert.IsNotNull(IMCtx);
            _InitModel();
        }

        void _InitModel()
        {
            IdleMinerContext IMCtx = (IdleMinerContext)context;
            Assert.IsNotNull(IMCtx);

            string gamePath = (string)IMCtx.GetData("gamePath");
            LoadModel(gamePath);

            InitModel4Simulation();

            _isInitialized = true;
        }

        void LoadModel(string gamePath)
        {
            var textData = Resources.Load<TextAsset>(gamePath + "/Data/Craft_comp");
            CompCraftData = JsonUtility.FromJson<CraftData>(textData.text);
            VerifyData(CompCraftData);
            ConvertCraftData(CompCraftData);

            textData = Resources.Load<TextAsset>(gamePath + "/Data/Craft_Item");
            ItemCraftData = JsonUtility.FromJson<CraftData>(textData.text);
            VerifyData(ItemCraftData);
            ConvertCraftData(ItemCraftData);
        }

        public override void Dispose()
        {
            base.Dispose();

            CompCraftData?.Dispose();
            ItemCraftData?.Dispose();

            CompCraftData = null;
            ItemCraftData = null;

            _isInitialized = false;
        }

        void ConvertCraftData(CraftData craftData)
        {
            for (int q = 0; q < craftData.Recipes.Count; ++q)
            {
                var recipe = craftData.Recipes[q];
                
                ResourceInfo targetInfo = null;
                context?.RequestQuery("Resource", endPoint:"GetResourceInfo", (errorMsg, ret) => 
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errorMsg));
                    targetInfo = (ResourceInfo)ret;

                }, recipe.OutcomeId);

                Assert.IsNotNull(targetInfo, "Couldn't find " + recipe.OutcomeId);
                recipe.eTargetRscLevel = targetInfo.eLevel;
            }
        }






        //==========================================================================
        //
        // Recipe Info Control
        //
        //
        public RecipeInfo GetRecipeInfo(eRscStageType eLv, string id)
        {
            List<RecipeInfo> targetList = eLv == eRscStageType.COMPONENT ? CompCraftData.Recipes : ItemCraftData.Recipes;

            for (int q = 0; q < targetList.Count; ++q)
            {
                if (0 == string.Compare(targetList[q].Id, id, ignoreCase:true))
                    return targetList[q];
            }
            return null;
        }
        
        public RecipeInfo GetRecipeInfo(int idx, eRscStageType eLv)
        {
            var recipeList = eLv == eRscStageType.COMPONENT ? CompCraftData.Recipes : ItemCraftData.Recipes;
            if (idx < 0 || idx >= recipeList.Count)
                return null;

            return recipeList[idx];
        }
        public List<RecipeInfo> GetRecipeInfoList(eRscStageType eTargetRscLevel, bool purchasedOnly = true)
        {
            var listRecipe = eTargetRscLevel == eRscStageType.COMPONENT ? CompCraftData.Recipes : ItemCraftData.Recipes;
            int idxPurchased = eTargetRscLevel == eRscStageType.COMPONENT ? PlayerData.PurchasedCompRecipIndex : PlayerData.PurchasedItemRecipIndex;

            // Need Full list ?
            if (!purchasedOnly || context.IsSimulationMode())
                return listRecipe;

            var ret = new List<RecipeInfo>();
            for (int q = 0; q < listRecipe.Count; ++q)
            {
                if (q <= idxPurchased)
                    ret.Add(listRecipe[q]);

                else break;
            }
            return ret;
        }
        public bool PurchaseRecipeInfoSlot(eRscStageType eLevel)
        {
            var recipeList = GetRecipeInfoList(eLevel, purchasedOnly: false);
            int idxNextPurchase = eLevel == eRscStageType.COMPONENT ? PlayerData.PurchasedCompRecipIndex : PlayerData.PurchasedItemRecipIndex;
            ++idxNextPurchase;
            Assert.IsTrue(idxNextPurchase >= 0 && idxNextPurchase < recipeList.Count);
            if (idxNextPurchase < 0 || idxNextPurchase >= recipeList.Count)
                return false;

            return PlayerData.PurchaseRecipe(eLevel, recipeList[idxNextPurchase].BICost);
        }


        //==========================================================================
        //
        // Crafting Slot Control
        //
        //
        public BigInteger GetSlotCost(int idx, eRscStageType eLevel)
        {
            var costList = eLevel == eRscStageType.COMPONENT ? CompCraftData.BISlotCosts : ItemCraftData.BISlotCosts;

            Assert.IsTrue(costList != null && costList.Count > 0);

            idx = Math.Clamp(idx, 0, costList.Count - 1);
            return costList[idx];
        }

        public List<CraftingSlot> GetCraftSlotList(eRscStageType eTargetRscLevel)
        {
            if (eTargetRscLevel == eRscStageType.COMPONENT)
                return PlayerData.CraftingCompSlots;
            else
                return PlayerData.CraftingItemSlots;
        }

        public bool PurchaseCraftSlot(eRscStageType eLevel, bool isSimMode=false)
        {
            var curSlotList = eLevel == eRscStageType.COMPONENT ? PlayerData.CraftingCompSlots : PlayerData.CraftingItemSlots;
            BigInteger cost = isSimMode ? BigInteger.Zero : GetSlotCost(curSlotList.Count, eLevel);
            return PlayerData.ExtendCraftSlot(eLevel, (uint)cost);
        }
        public bool AssignRecipeToSlot(eRscStageType level, int idxSlot, string recipeId)
        {
            var reciptList = GetRecipeInfoList(level);
            if(reciptList == null)
                return false;

            for(int q = 0; q < reciptList.Count; ++q)
            {
                if(0 == string.Compare(reciptList[q].Id, recipeId, ignoreCase:true))
                    return AssignRecipeToSlot(level, idxSlot, idxRecipe:q);
            }
            return false;
        }

        public bool AssignRecipeToSlot(eRscStageType level, int idxSlot, int idxRecipe)
        {
            var reciptList = GetRecipeInfoList(level);

            float fReqCountBuff = GetCraftReqResourceBuffRate(level);

            // Hold the Progress if the slot has any recipes.
            var oldSlotInfo = level == eRscStageType.COMPONENT ? PlayerData.CraftingCompSlots : PlayerData.CraftingItemSlots;
            if (idxSlot >= 0 && idxSlot < oldSlotInfo.Count)
            {
                if (!string.IsNullOrEmpty(oldSlotInfo[idxSlot].RecipeId_))
                {
                    var recipeData = GetRecipeInfo(level, oldSlotInfo[idxSlot].RecipeId_);
                    if (recipeData != null && oldSlotInfo[idxSlot].ProgressedTime_ >= 0)
                    {
                        // Hold Progress.
                        for (int q = 0; q < recipeData.Sources.Count; ++q)
                        {
                            context.RequestQuery("Resource", "PlayerData.UpdateResourceX1000", (errMsg, ret) => { }, 
                                recipeData.Sources[q].ResourceId, (BigInteger)(recipeData.Sources[q].GetCount(fReqCountBuff)*1000), true);
                        }
                        oldSlotInfo[idxSlot].Idle();
                        //
                    }
                }
            }


            // Processing a new Slot.
            RecipeInfo recipeInfo = null;
            if (idxRecipe >= 0 && idxRecipe < reciptList.Count)
                recipeInfo = reciptList[idxRecipe];
            return PlayerData.AssignRecipeToSlot(level, idxSlot, recipeInfo);
        }

        public void UnassignRecipeToSlot(eRscStageType level, int idxSlot)
        {
            AssignRecipeToSlot(level, idxSlot, idxRecipe:-1);
        }

        public RecipeInfo GetAssignedRecipeOnSlot(eRscStageType eType, int idxSlot)
        {
            var slotInfo = eType==eRscStageType.COMPONENT ? PlayerData.CraftingCompSlots : PlayerData.CraftingItemSlots;
            if (idxSlot < 0 || idxSlot < slotInfo.Count)
            {
                if (!string.IsNullOrEmpty(slotInfo[idxSlot].RecipeId_))
                    return GetRecipeInfo(eType, slotInfo[idxSlot].RecipeId_);
            }
            return null;
        }

        public int GetSlotIndexByOutcomeResourceId(eRscStageType eType, string resourceId)
        {
            var slotInfo = eType==eRscStageType.COMPONENT ? PlayerData.CraftingCompSlots : PlayerData.CraftingItemSlots;
            for(int q = 0; q < slotInfo.Count; ++q)
            {
                if (string.IsNullOrEmpty(slotInfo[q].RecipeId_))
                    continue;

                var recipeInfo = GetRecipeInfo(eType, slotInfo[q].RecipeId_);
                if(0 == string.Compare(recipeInfo.OutcomeId, resourceId, ignoreCase:true))
                    return q;
            }
            return -1;
        }

        public int GetFirstEmptySlotIndex(eRscStageType eType)
        {
            var slotInfo = eType==eRscStageType.COMPONENT ? PlayerData.CraftingCompSlots : PlayerData.CraftingItemSlots;
            for(int q = 0; q < slotInfo.Count; ++q)
            {
                if (string.IsNullOrEmpty(slotInfo[q].RecipeId_))
                    return q;
            }
            return -1;
        }



        //==========================================================================
        //
        // Skills.
        //
        //
        public void UpdateCraftSpeedBuff(eRscStageType eCraftLevel, float rate)
        {
            PlayerData.UpdateCraftSpeedBuff(eCraftLevel, rate);
        }
        public void UpdateReqResourceBuff(eRscStageType eCraftLevel, float rate)
        {
            PlayerData.UpdateReqResourceBuff(eCraftLevel, rate);
        }
        public float GetCraftTimeBuffRate(eRscStageType eCraftLevel)
        {
            return PlayerData.GetCraftTimeBuffRate(eCraftLevel);
        }
        public float GetCraftReqResourceBuffRate(eRscStageType eCraftLevel)
        {
            return PlayerData.GetCraftReqResourceBuffRate(eCraftLevel);
        }


        //==========================================================================
        //
        // Util
        //
        void VerifyData(CraftData data)
        {
            Assert.IsTrue(data != null && data.Recipes != null);
            for (int q = 0; q < data.Recipes.Count; ++q)
            {
                var recipe = data.Recipes[q];
                Assert.IsTrue(recipe != null && recipe.Sources != null);
                for (int j = 0; j < recipe.Sources.Count; ++j)
                {
                    ResourceInfo rscInfo = null;
                    context?.RequestQuery("Resource", endPoint:"GetResourceInfo", (errorMsg, ret) => 
                    {
                        Assert.IsTrue(string.IsNullOrEmpty(errorMsg));
                        rscInfo = (ResourceInfo)ret;

                    }, recipe.Sources[j].ResourceId);

                    Assert.IsNotNull(rscInfo);
                }
            }
            Assert.IsTrue(data.SlotCosts != null);
        }
    
    
        void InitModel4Simulation()
        {
#if UNITY_EDITOR
            if(!context.IsSimulationMode())
                return;

            int craftSlotCount = (int)context.GetData("CraftSlotCount", 3);
            for(int q = 0; q < craftSlotCount; ++q)
            {
                PurchaseCraftSlot(eRscStageType.COMPONENT, isSimMode:true);
                PurchaseCraftSlot(eRscStageType.ITEM, isSimMode:true);
                Debug.Log($"<color=green>[SIM] {q+1}th craft slots for both COMP and ITEM level have been added.</color>");
            }
#endif
        }

        public RecipeInfo FindRecipeByOutcomeId(eRscStageType eType, string outcomeResourceId)
        {
            var recipeData = eType==eRscStageType.COMPONENT ? CompCraftData : ItemCraftData;
           
            for(int q = 0; q < recipeData.Recipes.Count; ++q)
            {
                if(0 == string.Compare(recipeData.Recipes[q].OutcomeId, outcomeResourceId, ignoreCase:true))
                    return recipeData.Recipes[q];
            }
            return null;
        }

    }
}
