
using App.GamePlay.IdleMiner.Common.Model;
using Core.Events;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using App.GamePlay.IdleMiner.Common.Types;

namespace App.GamePlay.IdleMiner.Craft
{
    [Serializable]
    public class CraftingSlot
    {
        [SerializeField] string RecipeId;             // string.Empty means Empty Slot.
        [SerializeField] int ProgressedTime = -1;     // in sec. (-1 : ready to run, > 0 : in progress )


        //
        public string RecipeId_ { get { return RecipeId; } set { RecipeId = value; } }
        public int ProgressedTime_ { get => ProgressedTime; set => ProgressedTime = value; }


        public void Start() 
        { 
            if(ProgressedTime == -1) ProgressedTime = 0; 
        }
        public void Proceed(int sec) { ProgressedTime += sec; }
        public void Spend(int sec)   { ProgressedTime -= sec; }
        public void Idle() { ProgressedTime = -1; }
    }

    public class CraftBuffStat
    {
        float durationRate;     // 1.0f ~ 0.0f
        float reqRate;          // 1.0f ~ 0.0f

        public float DurationRate => durationRate;
        public float RequestRate => reqRate;

        public CraftBuffStat() { durationRate = 1.0f;   reqRate = 1.0f; }
        public CraftBuffStat(float fDurationRate, float reqRate)
        {
            SetStat(fDurationRate, reqRate);
        }   
        public void SetStat(float fDurationRate, float reqRate)
        {
            this.durationRate = fDurationRate;
            this.reqRate = reqRate;
        }   
    }


    public class CraftPlayerModel : GatewayWritablePlayerModel
    {
        public class CraftStoreData
        {
            [SerializeField] bool isFeatureOpened;
            [SerializeField] List<CraftingSlot> craftingSlots = new List<CraftingSlot>();
            [SerializeField] int purchasedRecipeIndex = -1;

            public bool IsFeatureOpened { get => isFeatureOpened; set => isFeatureOpened = value; }
            public List<CraftingSlot> CraftingSlots => craftingSlots;
            public int PurchasedRecipeIndex { get => purchasedRecipeIndex; set => purchasedRecipeIndex = value; }
            public void Reset()
            {
                isFeatureOpened = false;
                craftingSlots.Clear();
                purchasedRecipeIndex = -1;
            }
        }

        CraftStoreData compCraftData, itemCraftData;

        //List<CraftingSlot> craftingCompSlots = new List<CraftingSlot>();
        //List<CraftingSlot> craftingItemSlots = new List<CraftingSlot>();
        //int purchasedCompRecipIndex;
        //int purchasedItemRecipIndex;
        //bool isCompCraftOpened;
        //bool isItemCraftOpened;

        CraftBuffStat compBuffStat = new CraftBuffStat();
        CraftBuffStat itemBuffStat = new CraftBuffStat();
        CraftBuffStat multiProductionCompBuffStat = new CraftBuffStat();
        CraftBuffStat multiProductionItemBuffStat = new CraftBuffStat();

        // Accessor.
        public List<CraftingSlot> CraftingCompSlots => compCraftData?.CraftingSlots;
        public List<CraftingSlot> CraftingItemSlots => itemCraftData?.CraftingSlots;
        public int PurchasedCompRecipIndex => compCraftData==null ? 0 : compCraftData.PurchasedRecipeIndex;
        public int PurchasedItemRecipIndex => itemCraftData==null? 0 : itemCraftData.PurchasedRecipeIndex;
        public bool IsCompCraftOpened => compCraftData==null ? false : compCraftData.IsFeatureOpened;
        public bool IsItemCraftOpened => itemCraftData==null ? false : itemCraftData.IsFeatureOpened;

        EventsGroup Events = new EventsGroup();

        static string DataKey_CraftData(eRscStageType type)
        {
            string key = type==eRscStageType.COMPONENT ? "Comp" : "Item";
            return $"{nameof(CraftPlayerModel)}_Craft{key}Data";
        }
        static string DataKey_CraftBuffData(eRscStageType type)
        {
            string key = type==eRscStageType.COMPONENT ? "Comp" : "Item";
            return $"{nameof(CraftPlayerModel)}_Craft{key}BuffData";
        }

        public CraftPlayerModel(AContext ctx, IDataGatewayService gatewayService) : base(ctx, gatewayService)  { }

        public override void Init()
        {
            base.Init();

            LoadCraftData();

            Events.RegisterEvent(EventID.SKILL_RESET_GAME_INIT, ResetGamePlay_InitGame);
            IsInitialized = true;
        }

        public override void Dispose()
        {
            base.Dispose();

            Events.UnRegisterEvent(EventID.SKILL_RESET_GAME_INIT, ResetGamePlay_InitGame);
            IsInitialized= false;
        }       

        void LoadCraftData()
        {
            LoadCraftData(eRscStageType.COMPONENT);
            LoadCraftData(eRscStageType.ITEM);
        }
        
        void LoadCraftData(eRscStageType type)
        {
            CraftStoreData craftData = null;
            if(context.IsSimulationMode())
                craftData = new CraftStoreData();
            else 
                FetchData(DataKey_CraftData(type), out craftData, new CraftStoreData());
            
            if(type == eRscStageType.COMPONENT) 
                compCraftData = craftData;
            else 
                itemCraftData = craftData;

            
            //if(type == eRscStageType.COMPONENT)     ReadFileInternal(DataKey_CraftBuffData(type), ref compBuffStat);
            //else                                    ReadFileInternal(DataKey_CraftBuffData(type), ref itemBuffStat);
        }

        //==========================================================================
        //
        // Resource Craft Control.
        //
        //
        public bool AssignRecipeToSlot(eRscStageType level, int idxSlot, RecipeInfo info)
        {
            var craftSlots = level == eRscStageType.COMPONENT ? CraftingCompSlots : CraftingItemSlots;
            if (idxSlot < 0 || idxSlot >= craftSlots.Count)
            {
                Assert.IsTrue(false, "Recipe Slot Index Error!");
                return false;
            }

            craftSlots[idxSlot].RecipeId_ = info==null ? string.Empty : info.Id;
            craftSlots[idxSlot].Idle();

            EventSystem.DispatchEvent(EventID.CRAFT_RECIPE_ASSIGNED);
            return true;
        }

        public bool IsMultiProduction(eRscStageType eLevel, string recipe_Id)
        {
            CraftStoreData craftData = eLevel==eRscStageType.COMPONENT ? compCraftData : itemCraftData;
            int count = 0;
            for(int q = 0; q < craftData.CraftingSlots.Count; ++q)
            {
                if(0 == string.Compare(craftData.CraftingSlots[q].RecipeId_, recipe_Id, StringComparison.OrdinalIgnoreCase))
                    ++count;
            }
            return count>=2;
        }

        public bool ExtendCraftSlot(eRscStageType eLevel, BigInteger cost)
        {      
            bool isAffordable = false;
            if(cost > BigInteger.Zero)
            {
                context.RequestQuery("IdleMiner", "IsAffordableCurrency", (errorMsg, ret) => 
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errorMsg), errorMsg);
                    isAffordable = (bool)ret;

                }, cost, eCurrencyType.IAP_COIN);

                if (!isAffordable)
                    return false;

                context.RequestQuery("IdleMiner", "AddMoney", (errMsg, ret) => { }, 
                        new CurrencyAmount((-1*cost).ToString(), eCurrencyType.IAP_COIN) );
            }
            
            var listRecipeList = eLevel == eRscStageType.COMPONENT ? CraftingCompSlots : CraftingItemSlots;
            CraftingSlot slot = new CraftingSlot();
            slot.RecipeId_ = string.Empty;
            listRecipeList.Add(slot);

            EventSystem.DispatchEvent(EventID.CRAFT_SLOT_EXTENDED);
            return true;
        }

        public bool PurchaseRecipe(eRscStageType eLevel, BigInteger cost)
        {
            bool isAffordable = false;
            context.RequestQuery("IdleMiner", "IsAffordableCurrency", (errorMsg, ret) => 
            {
                Assert.IsTrue(string.IsNullOrEmpty(errorMsg));
                isAffordable = (bool)ret;

            }, cost, eCurrencyType.IAP_COIN);

            if (!isAffordable)
                return false;

            context.RequestQuery("IdleMiner", "AddMoney", (errMsg, ret) => { }, 
                    new CurrencyAmount((-1*cost).ToString(), eCurrencyType.IAP_COIN) );

            CraftStoreData craftData = eLevel==eRscStageType.COMPONENT ? compCraftData : itemCraftData;
            craftData.PurchasedRecipeIndex++;
            
            EventSystem.DispatchEvent(EventID.CRAFT_RECIPE_PURCHASED);
            return true;
        }

        public void OpenFeature(eRscStageType eLevel)
        {
            CraftStoreData craftData = eLevel==eRscStageType.COMPONENT ? compCraftData : itemCraftData;
            craftData.IsFeatureOpened = true;
        }

        //==========================================================================
        //
        // Skill
        //
        //public void UnlockCraftBooster(eRscStageType eCraftLevel, float durationBuffRate, float priceBuffRate)
        //{
        //    UpdateCraftBooster(eCraftLevel, durationBuffRate, priceBuffRate);
        //}
        public void UpdateCraftSpeedBuff(eRscStageType eCraftLevel, float rate)
        {
            CraftBuffStat buffState = eCraftLevel==eRscStageType.COMPONENT ? compBuffStat : itemBuffStat;
            buffState.SetStat(buffState.DurationRate*rate, buffState.RequestRate);
        }
        public void UpdateReqResourceBuff(eRscStageType eCraftLevel, float rate)
        {
            CraftBuffStat buffState = eCraftLevel==eRscStageType.COMPONENT ? compBuffStat : itemBuffStat;
            buffState.SetStat(buffState.DurationRate, buffState.RequestRate*rate);
        }

        public float GetCraftTimeBuffRate(eRscStageType eCraftLevel) 
        {
            CraftBuffStat buffState = eCraftLevel==eRscStageType.COMPONENT ? compBuffStat : itemBuffStat;
            return buffState.DurationRate;
        }
        public float GetCraftReqResourceBuffRate(eRscStageType eCraftLevel) 
        {
            CraftBuffStat buffState = eCraftLevel==eRscStageType.COMPONENT ? compBuffStat : itemBuffStat;
            return buffState.RequestRate;
        }

        public void UpdateMultiProductionSpeedBuff(eRscStageType eCraftLevel, float rate)
        {
            CraftBuffStat buffState = eCraftLevel==eRscStageType.COMPONENT ? multiProductionCompBuffStat : multiProductionItemBuffStat;
            buffState.SetStat(buffState.DurationRate*rate, buffState.RequestRate);
        }
        public void UpdateMultiProductionReqResourceBuff(eRscStageType eCraftLevel, float rate)
        {
            CraftBuffStat buffState = eCraftLevel==eRscStageType.COMPONENT ? multiProductionCompBuffStat : multiProductionItemBuffStat;
            buffState.SetStat(buffState.DurationRate, buffState.RequestRate*rate);
        }

        public float GetMultiProductionTimeBuffRate(eRscStageType eCraftLevel)
        {
            CraftBuffStat buffState = eCraftLevel==eRscStageType.COMPONENT ? multiProductionCompBuffStat : multiProductionItemBuffStat;
            return buffState.DurationRate;
        }
        public float GetMultiProductionReqResourceBuffRate(eRscStageType eCraftLevel) 
        {
            CraftBuffStat buffState = eCraftLevel==eRscStageType.COMPONENT ? multiProductionCompBuffStat : multiProductionItemBuffStat;
            return buffState.RequestRate;
        }

        void ResetGamePlay_InitGame(object data) 
        {
            LoadCraftData();

            compCraftData.Reset();
            itemCraftData.Reset();
        }

        #region IWritableModel

        public override List<Tuple<string, string>> GetSaveDataWithKeys()
        {
            List<Tuple<string, string>> listDataSet = new List<Tuple<string, string>>();
            
            Assert.IsNotNull(compCraftData);
            listDataSet.Add(new Tuple<string, string>(DataKey_CraftData(eRscStageType.COMPONENT), JsonUtility.ToJson(compCraftData)));
            Assert.IsNotNull(itemCraftData);
            listDataSet.Add(new Tuple<string, string>(DataKey_CraftData(eRscStageType.ITEM), JsonUtility.ToJson(itemCraftData)));
            
            // Let's do Buffs later.

            return listDataSet;
        }
        
        #endregion

#if UNITY_EDITOR
        //==========================================================================
        //
        // Editor - Reset Data Prefab
        //
        [UnityEditor.MenuItem("PlasticGames/Clear PlayerData/Craft")]
        public static void EditorClearCraftData()
        {
            // ClearCraftData();
        }
#endif
    }
}
