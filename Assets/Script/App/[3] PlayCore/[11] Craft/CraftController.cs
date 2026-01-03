using App.GamePlay.IdleMiner;
using App.GamePlay.IdleMiner.Common;
using App.GamePlay.IdleMiner.Common.Model;
using App.GamePlay.IdleMiner.Common.PlayerModel;
using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using Core.Events;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.Craft
{
    //  Craft, Forge Related stuff Management.-------------------------------------
    //
    public class CraftController : IGCore.MVCS.AController, ISkillLeaner// AMinerModule
    {
        #region SKILL_BEHAVIOR
        
        // Skill Behavior Sub Classes. 
        // Note : Nested classes can access private members to this class.
        //
        class UnlockCraftFeatureSkill : ISkillBehavior
        {
            public const string Id = "UNLOCK_FEATURE";
            // compcraft or itemcraft
            public void Learn(AController ctrler, string ability_param) 
            { 
                if(string.IsNullOrEmpty(ability_param))
                    return;

                CraftController craftCtrl = ctrler as CraftController;
                eRscStageType resourceStageType = ability_param=="compcraft" ? eRscStageType.COMPONENT : eRscStageType.ITEM;
                craftCtrl.Model.PlayerData.OpenFeature( resourceStageType );  
            }
        }
        class CraftBuffSkill : ISkillBehavior
        {
            public const string Id = "CRAFT_BUFF";
            // comp:time=0.95
            public void Learn(AController ctrler, string ability_param) 
            {
                if(string.IsNullOrEmpty(ability_param))
                    return;

                // comp:time=0.95 or item:resource=0.95
                string[] parameters = ability_param.Split(':');
                if(parameters.Length != 2)
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }

                eRscStageType eResType = parameters[0]=="comp" ? eRscStageType.COMPONENT : eRscStageType.ITEM;
                // time=0.95 or resource=0.95
                string[] sub_param = parameters[1].Split('=');
                if(sub_param.Length != 2)
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }

                float rate;
                if(false == float.TryParse(sub_param[1], out rate))
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }

                CraftController craftCtrl = ctrler as CraftController;
                switch(sub_param[0])
                {
                case "time":    craftCtrl.Model.UpdateCraftSpeedBuff(eResType, rate);   break;
                case "resource":craftCtrl.Model.UpdateReqResourceBuff(eResType, rate);  break;
                default:
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }
            }
        }
        class CraftMultiProductionBuffSkill : ISkillBehavior
        {
            public const string Id = "MULTI_CRAFT_BUFF";
            // comp:time=0.95
            public void Learn(AController ctrler, string ability_param) 
            {
                if(string.IsNullOrEmpty(ability_param))
                    return;

                // comp:time=0.95 or item:resource=0.95
                string[] parameters = ability_param.Split(':');
                if(parameters.Length != 2)
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }

                eRscStageType eResType = parameters[0]=="comp" ? eRscStageType.COMPONENT : eRscStageType.ITEM;
                // time=0.95 or resource=0.95
                string[] sub_param = parameters[1].Split('=');
                if(sub_param.Length != 2)
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }

                float rate;
                if(false == float.TryParse(sub_param[1], out rate))
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }

                CraftController craftCtrl = ctrler as CraftController;
                switch(sub_param[0])
                {
                case "time":    craftCtrl.Model.PlayerData.UpdateMultiProductionSpeedBuff(eResType, rate);          break;
                case "resource":craftCtrl.Model.PlayerData.UpdateMultiProductionReqResourceBuff(eResType, rate);    break;
                default:
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }
            }
        }
        class CraftSkipCostSkill : ISkillBehavior
        {
            public const string Id = "CRAFT_SKIP_COST";
            // comp:chance=1.0:interval=60
            public void Learn(AController ctrler, string ability_param) 
            {
                if(string.IsNullOrEmpty(ability_param))
                    return;

                // comp:chance=1.0:interval=60
                string[] parameters = ability_param.Split(':');
                if(parameters.Length != 3)
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }

                eRscStageType eResType = parameters[0]=="comp" ? eRscStageType.COMPONENT : eRscStageType.ITEM;
                
                // chance=0.5
                string[] sub_param = parameters[1].Split('=');
                if(sub_param.Length != 2)
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }
                
                Assert.IsTrue(sub_param[0]=="chance", "Wrong Param : " + ability_param);
                float chance;
                if(false == float.TryParse(sub_param[1], out chance))
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }

                // interval=60
                sub_param = parameters[2].Split('=');
                if(sub_param.Length != 2)
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }

                Assert.IsTrue(sub_param[0]=="interval", "Wrong Param : " + ability_param);
                float interval;
                if(false == float.TryParse(sub_param[1], out interval))
                {
                    Debug.LogWarning("Wrong Parameter : " + ability_param);
                    return;
                }


                // Skip for now.
            }
        }

        #endregion



        CraftView View => (CraftView)view;
        CraftModel Model => (CraftModel)model;

        RecipeListPopupDialog RecipePopupDialogCache = null;
        IdleMinerContext IMContext => (IdleMinerContext)context;

        Dictionary<string, int> dictResumeRscRefInfo = new Dictionary<string, int>();
        protected Dictionary<string, ISkillBehavior> dictSkillBehaviors = new Dictionary<string, ISkillBehavior>();

        class CraftRequirement
        {
            public string recipeId;
            public string outcomeId;
            public eRscStageType level;
            public BigInteger requiredAmount;
            public RecipeInfo recipeInfo;
        }

        #region ===> Initializers

        public CraftController(AUnit unit, AView view, AModel model, AContext ctx)
            : base(unit, view, model, ctx) { }


        //==========================================================================
        //
        // CraftDialog. - OnViewEnter.
        //
        //
        public override void Init()
        {
            ((ISkillLeaner)this).CreateSkillBehaviors();

            //Events = new EventsGroup();
            //Events.RegisterEvent(EventID.SKILL_LEARNED, EventOnSkillLearned);
            //Events.RegisterEvent(EventID.GAME_RESET_REFRESH, EventOnRefreshView);

            EventSystem.UnRegisterAll(this);// EventID.SKILL_LEARNED, EventOnSkillLearned);
            // EventSystem.UnRegisterEvent(EventID.GAME_RESET_REFRESH, EventOnRefreshView);

            EventSystem.RegisterEvent(EventID.SKILL_LEARNED, EventOnSkillLearned);
            EventSystem.RegisterEvent(EventID.GAME_RESET_REFRESH, EventOnResetRefresh);

            Debug.Log("[InitSeq]:[CraftController] Init...");
        
            View?.EventTabIndexChanged.AddListener(OnCraftPnlTabSelectionChanged);
            View?.EventOnEmptySlotClicked.AddListener(OnCraftPnlEmptySlotClicked);
            View?.EventOnBtnShowRecipeClicked.AddListener(OnCraftPnlShowRecipeClicked);
            View?.EventOnLockedSlotClicked.AddListener(OnCraftPnlLockedSlotClicked);
            View?.EventOnBtnProgXClicked.AddListener(OnCraftPnlBtnProgXClicked);

            context.AddRequestDelegate("Craft", "SIM_UpdateResourceReqStatus", sim_updateResourceReqStatus);

#if UNITY_EDITOR
           // TestCraftPumpResumeLogic();
#endif

        }
        
        public override void WriteData() { }

        public override void Dispose()
        {
            EventSystem.UnRegisterEvent(EventID.SKILL_LEARNED, EventOnSkillLearned);
            EventSystem.UnRegisterEvent(EventID.GAME_RESET_REFRESH, EventOnResetRefresh);

            View?.EventTabIndexChanged.RemoveAllListeners();
            View?.EventOnEmptySlotClicked.RemoveAllListeners();
            View?.EventOnBtnShowRecipeClicked.RemoveAllListeners();
            View?.EventOnLockedSlotClicked.RemoveAllListeners();
            View?.EventOnBtnProgXClicked.RemoveAllListeners();

            context.RemoveRequestDelegate("Craft", "SIM_UpdateResourceReqStatus");
        }

        public override void Resume(int durationSec)
        {
            if(context.IsSimulationMode())
            {   
                SIM_AssignRecipesFromSlots((HashSet<string>)context.GetData(SimDefines.SIM_RECIPES_IN_CRAFT_CHAIN));
            }

            if(durationSec > 1)
            {
                dictResumeRscRefInfo.Clear();
                CollectResourceReferenceInfo(eRscStageType.COMPONENT, dictResumeRscRefInfo);
                CollectResourceReferenceInfo(eRscStageType.ITEM, dictResumeRscRefInfo);
            }

            UpdateSlots(eRscStageType.COMPONENT, durationSec);
            UpdateSlots(eRscStageType.ITEM, durationSec);
        }

        public override void Pump()
        {
            UpdateSlots(eRscStageType.COMPONENT, duration:1);
            UpdateSlots(eRscStageType.ITEM, duration:1);

            RefreshCraftView();
        }

        protected override void OnViewEnable() 
        {
            View.StartCoroutine( coTriggerActionWithDelay(-1.0f, () => RefreshCraftView()) );
            View.NotificatorComp?.DisableNotification();
        }

        protected override void OnViewDisable() { }

        #endregion





        void RefreshCraftView()
        {
            // Hasn't ready yet. 
            if (View == null || !View.gameObject.activeSelf || View.InnerSingleCraftItemCount<=0)
                return;
            
            
            // Refrfesh the Craft Pnl View.
            //
            eRscStageType eLv = View.TabIndex == 0 ? eRscStageType.COMPONENT : eRscStageType.ITEM;
            string closedSlotMsg = eLv == eRscStageType.COMPONENT ? "BUILD\n TO-COMP" : "BUILD\n TO-ITEM";
            List<CraftingSlot> listCraftSlots = Model.GetCraftSlotList(eLv);
            List<CraftListMultiItemComp.PresentInfo> presentInfo = new List<CraftListMultiItemComp.PresentInfo>();
            
            bool isFeatureOpened = eLv == eRscStageType.COMPONENT ? Model.PlayerData.IsCompCraftOpened : Model.PlayerData.IsItemCraftOpened;
            if(!isFeatureOpened)
            {
                View.Refresh(new CraftView.PresentInfo());
                return;
            }
            
            float fTimeBuff = Model.GetCraftTimeBuffRate(eLv);
            float fPriceBuff = Model.GetCraftReqResourceBuffRate(eLv);
            
            float fMPTimeBuff = Model.PlayerData.GetMultiProductionTimeBuffRate(eLv);
            float fMPPriceBuff = Model.PlayerData.GetMultiProductionReqResourceBuffRate(eLv);

            int idx = 0;
            bool done = false;
            while (!done)
            {
                List<CraftSingleItemComp.PresentInfo> listCraftComp = new List<CraftSingleItemComp.PresentInfo>();
                for(int z = 0; z < View.InnerSingleCraftItemCount; ++z)
                    listCraftComp.Add(null);

                int q = 0;
                for(; q < listCraftComp.Count; ++q)
                {
                    int idxTarget = idx + q;
                    if(idxTarget >= listCraftSlots.Count)
                    {
                        listCraftComp[q] = done ? new CraftSingleItemComp.CraftPresentInfo(isOpened:false) : new CraftSingleItemComp.CraftPresentInfo(Model.GetSlotCost(idxTarget, eLv), closedSlotMsg);
                        done = true;
                    }
                    else
                    {
                        var recipeInfo = Model.GetRecipeInfo(eLv, listCraftSlots[idxTarget].RecipeId_);
                        bool isMultiProduction = recipeInfo!=null ? Model.PlayerData.IsMultiProduction(eLv, recipeInfo.Id) : false;
                        float multiProductionTimeBuff = isMultiProduction ? fMPPriceBuff : 1.0f;
                        float multiProductionPriceBuff = isMultiProduction ? fMPPriceBuff : 1.0f;
                        listCraftComp[q] = BuildCraftPresentData(recipeInfo, listCraftSlots[idxTarget].ProgressedTime_, fTimeBuff * multiProductionTimeBuff, fPriceBuff * multiProductionPriceBuff, isMultiProduction);
                    }
                }
                idx += q;

                var presentor = new CraftListMultiItemComp.PresentInfo(listCraftComp);
                presentInfo.Add(presentor);
            }
            

            string strMultiProduction = isClose(fMPTimeBuff, 1.0f) && isClose(fMPPriceBuff, 1.0f) ? string.Empty : ", Multi Production Buff has been Activated.";

            string status = string.Empty;
            if(isClose(fTimeBuff, 1.0f) && isClose(fPriceBuff, 1.0f))
                status = $"Slot Count : {listCraftSlots.Count}, No Activated Buff.";
            else 
                status = $"Slot Count : {listCraftSlots.Count}, Buff (Dration x{fTimeBuff.ToString("0.00")}, Price x{fPriceBuff.ToString("0.00")})";

            if(false == string.IsNullOrEmpty(strMultiProduction))
                status += strMultiProduction;

            View.Refresh(new CraftView.PresentInfo(presentInfo, status));
        }

        bool isClose(float value1, float closeTarget, float difference = 0.001f)
        {
            return Math.Abs(value1 - closeTarget) <= difference;
        }

        // ### : This helps interface's accessorbility to like private.
        void ISkillLeaner.CreateSkillBehaviors()
        {
            createSkillBehaviorInternal();
        }

        protected virtual void createSkillBehaviorInternal()
        {
            dictSkillBehaviors.Clear();

            dictSkillBehaviors.Add((UnlockCraftFeatureSkill.Id).ToLower(), new UnlockCraftFeatureSkill());
            dictSkillBehaviors.Add((CraftBuffSkill.Id).ToLower(), new CraftBuffSkill());
            dictSkillBehaviors.Add((CraftMultiProductionBuffSkill.Id).ToLower(), new CraftMultiProductionBuffSkill());
            dictSkillBehaviors.Add((CraftSkipCostSkill.Id).ToLower(), new CraftSkipCostSkill());
        }

        void ISkillLeaner.LearnSkill(string skill_id, string ability_id, string ability_param)
        {
            if(dictSkillBehaviors.ContainsKey(ability_id))
            {
                dictSkillBehaviors[ability_id].Learn(this, ability_param);

#if UNITY_EDITOR
                string strTime = $"===> [{(string)context.GetData("SimTime", string.Empty)}]";
                Debug.Log($"<color=green>[Skill] Learning Skill...{skill_id}:{ability_id}:{ability_param} {strTime}</color> ");
#endif
            }
        }

        void EventOnSkillLearned(object data)
        {
            if (data == null) return;

            Tuple<string, string, string, bool> skill_id_n_ability_id_param = (Tuple< string, string, string, bool>)data;
            string skill_id = skill_id_n_ability_id_param.Item1;
            string abilityId = skill_id_n_ability_id_param.Item2.ToLower();
            string abilityParam = skill_id_n_ability_id_param.Item3.ToLower();
            bool isPartOfInitProcess = (bool)skill_id_n_ability_id_param.Item4;

            // Debug.Log("<color=green>Learning Skills - " + skill_id + " </color>");
            ((ISkillLeaner)this).LearnSkill(skill_id, abilityId, abilityParam);

            // Show noti when this is (NOT a part of init process && Unlock_Feature)
            if(!isPartOfInitProcess && 0==string.Compare(abilityId, UnlockCraftFeatureSkill.Id, ignoreCase:true) && !context.IsSimulationMode())
                View.UpdateNotificator(skill_id, abilityParam.Contains("comp") ? eRscStageType.COMPONENT : eRscStageType.ITEM);
            
            RefreshCraftView();
        }

        void EventOnResetRefresh(object data)
        {
            View.InitAfterReset();

            RefreshCraftView();
        }


        void OnCraftPnlTabSelectionChanged(int idxTab)
        {
            RefreshCraftView();
        }

        void OnCraftPnlEmptySlotClicked(int idxSlot)
        {
            Debug.Log("Performing Empty Slot..." + idxSlot);

            OnCraftPnlShowRecipeClicked(idxSlot);
            eRscStageType eLv = View.TabIndex == 0 ? eRscStageType.COMPONENT : eRscStageType.ITEM;
            Model.AssignRecipeToSlot(eLv, idxSlot, idxRecipe: -1);
            RefreshCraftView();
        }

        void OnCraftPnlLockedSlotClicked(int idxSlot)
        {
            // Try purchasing the slot.
            eRscStageType eLv = View.TabIndex == 0 ? eRscStageType.COMPONENT : eRscStageType.ITEM;
            if (Model.PurchaseCraftSlot(eLv))
                RefreshCraftView();
            else
            {
                var presentInfo = new ToastMessageDialog.PresentInfo( message :  "Insufficient VOLT.", duration:1.5f );
                context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
                    "ToastMessageDialog", presentInfo,
                    new Action<APopupDialog>( (popupDlg) => 
                    { 
                        Debug.Log("ToastMessage Dialog has been closed.");
                    } ) ); 
            }
        }
        void OnCraftPnlBtnProgXClicked(int idxSlot)
        {
            // Try empty slot.
            eRscStageType eLv = View.TabIndex == 0 ? eRscStageType.COMPONENT : eRscStageType.ITEM;
            Model.AssignRecipeToSlot(eLv, idxSlot, idxRecipe:-1);

            RefreshCraftView();
        }

        CraftSingleItemComp.CraftPresentInfo BuildCraftPresentData(RecipeInfo recipeData, int progressTime, float fTimeBuffRate, float fPriceBuffRate, bool isMultiProduction)
        {
            if (recipeData == null)     // This means Empty Slot.
                return new CraftSingleItemComp.CraftPresentInfo(isOpened:true);

            bool isInProgress = progressTime >= 0;
            ResourceInfo rscInfo = null;
            Sprite iconSprite;

            // Build Sources.
            List<FrameResourceComp.PresentInfo> sources = new List<FrameResourceComp.PresentInfo>();
            for (int k = 0; k < recipeData.Sources.Count; ++k)
            {
                var srcInfo = recipeData.Sources[k];
                ResourceCollectInfo srcCollected = null;
                context.RequestQuery("Resource", "PlayerData.GetResourceCollectInfo", (errMsg, ret) =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                    srcCollected = (ResourceCollectInfo)ret;
                }, srcInfo.ResourceId);

                BigInteger collectedCount = BigInteger.Zero;
                if (srcCollected != null)
                    collectedCount = srcCollected.BICount;

                collectedCount = isInProgress ? srcInfo.GetCount(fPriceBuffRate) : collectedCount;

                context.RequestQuery("Resource", "GetResourceInfo", (errMsg, ret) =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                    rscInfo = (ResourceInfo)ret;
                }, srcInfo.ResourceId);

                Assert.IsNotNull(rscInfo);
                iconSprite = IMContext.GetSprite(rscInfo.GetSpriteGroupId(), spriteKey:rscInfo.Id);
                int requiredCount = srcInfo.GetCount(fPriceBuffRate);
                sources.Add(new FrameResourceComp.PresentInfo(iconSprite, srcInfo.ResourceId, rscInfo.GetClassKey(), requiredCount, collectedCount));
            }

            // Build Target.
            context.RequestQuery("Resource", "GetResourceInfo", (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                rscInfo = (ResourceInfo)ret;
            }, recipeData.OutcomeId);

            Assert.IsNotNull(rscInfo);
            iconSprite = IMContext.GetSprite(rscInfo.GetSpriteGroupId(), spriteKey: rscInfo.Id);

            ResourceCollectInfo targetCollected = null;
            context.RequestQuery("Resource", "PlayerData.GetResourceCollectInfo", (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                targetCollected = (ResourceCollectInfo)ret;
            }, recipeData.OutcomeId);

            var target = new FrameResourceComp.PresentInfo(iconSprite, recipeData.OutcomeId, rscInfo.GetClassKey(), targetCollected==null ? 0 : targetCollected.BICount);

            // Returning Final Item Presentor data.
            return new CraftSingleItemComp.CraftPresentInfo(sources, target, recipeData.GetDuration(fTimeBuffRate), progressTime, isMultiProduction);
        }





        //==========================================================================
        //
        // RecipePopupDialog.
        //
        //
        void OnCraftPnlShowRecipeClicked(int idxSlot)
        {            
            Debug.Log("Triggering Recipe List Popup Dialog...." + idxSlot);

            eRscStageType eLv = View.TabIndex == 0 ? eRscStageType.COMPONENT : eRscStageType.ITEM;
            List<CraftingSlot> listCraftStatus = Model.GetCraftSlotList(eLv);
            if (idxSlot >= listCraftStatus.Count)
            {
                // Try Purchase New Slot.
                if (Model.PurchaseCraftSlot(eLv))
                    RefreshCraftView();
                else
                {
                    var presentData = new ToastMessageDialog.PresentInfo( message :  "Insufficient VOLT.", duration:1.5f );
                    context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY), "DisplayPopupDialog", (errMsg, ret) => {}, 
                        "ToastMessageDialog", presentData,
                        new Action<APopupDialog>( (popupDlg) => 
                        { 
                            Debug.Log("ToastMessage Dialog has been closed.");
                        } ) ); 
                }
                return;
            }


            // Trigger Recipe List Dialog.
            //
            var presentInfo = BuildRecipePopupDialogPresentInfo(eLv);
            APopupDialog recipePopupDialog = null;
            context.RequestQuery((string)context.GetData(KeySets.CTX_KEYS.GAME_DLG_KEY), "DisplayPopupDialog", 
                (errMsg, ret) =>  { recipePopupDialog = (APopupDialog)ret;  },  
                "RecipesDialog", presentInfo, new System.Action<APopupDialog>( (popupDialog) =>
                {
                    var recipeDlg = (RecipeListPopupDialog)popupDialog;
                    int idxRecipe = recipeDlg.SelectedSlotIndex;

                    Debug.Log("Dlg has been closed. - Idx " + idxRecipe.ToString());

                    recipeDlg.EventLockedSlotClicked.RemoveAllListeners();

                    if (idxRecipe >= 0)
                        Model.AssignRecipeToSlot(eLv, idxSlot, idxRecipe);

                    RefreshCraftView();

                }) );


            RecipePopupDialogCache = (RecipeListPopupDialog)recipePopupDialog;
            RecipePopupDialogCache.EventLockedSlotClicked.AddListener(RecipePopupDialogOnLockSlotClicked);
        }



        RecipeListPopupDialog.PresentInfo BuildRecipePopupDialogPresentInfo(eRscStageType eLv)
        {
            List<CraftListMultiItemComp.PresentInfo> presentInfo = new List<CraftListMultiItemComp.PresentInfo>();
            List<RecipeInfo> purchasedRecipeInfoList = Model.GetRecipeInfoList(eLv, purchasedOnly:true);
            
            bool done = false;
            ResourceInfo rscInfo;
            Sprite iconSprite;

            const int SingleRecepiItemCount = 4;
            int idx = 0;
            while (!done)
            {
                // Build Left side of data.
                //++idx;
                //RecipeSingleItemComp.RecipePresentInfo singleLeft = null, singleRight = null;

                List<RecipeSingleItemComp.PresentInfo> listComp = new List<RecipeSingleItemComp.PresentInfo>();
                for(int z = 0; z < SingleRecepiItemCount; ++z)
                    listComp.Add(null);

                int q = 0;
                for(; q < listComp.Count; ++q)
                {
                    int idxTarget = idx + q;
                    if (idxTarget >= purchasedRecipeInfoList.Count)
                    {
                        RecipeInfo recipeInfo = Model.GetRecipeInfo(idxTarget, eLv);
                        bool isDisplayNextUnlockedRecipe = recipeInfo!=null && done==false;

                        if(isDisplayNextUnlockedRecipe)
                        {
                            rscInfo = null;
                            context.RequestQuery("Resource", "GetResourceInfo", (errMsg, ret) =>
                            {
                                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                                rscInfo = (ResourceInfo)ret;
                            }, recipeInfo.OutcomeId);

                            Assert.IsNotNull(rscInfo);
                            iconSprite = IMContext.GetSprite(rscInfo.GetSpriteGroupId(), spriteKey: rscInfo.Id);
                            listComp[q] = new RecipeSingleItemComp.RecipePresentInfo(iconSprite, recipeInfo.OutcomeId, rscInfo.GetClassKey(), recipeInfo.BICost);
                        }
                        else
                        {
                            listComp[q] = new RecipeSingleItemComp.RecipePresentInfo();
                        }
                        done = true;
                    }
                    else
                    {
                        listComp[q] = BuildRecipePresentData(purchasedRecipeInfoList[idxTarget], progressTime: -1);
                    }
                }
                idx += q;

                var presentor = new CraftListMultiItemComp.PresentInfo(listComp);
                presentInfo.Add(presentor);
            }

            return new RecipeListPopupDialog.PresentInfo(presentInfo);
        }

        RecipeSingleItemComp.RecipePresentInfo BuildRecipePresentData(RecipeInfo recipeData, int progressTime)
        {
            if (recipeData == null)     // This means Empty Slot.
                return new RecipeSingleItemComp.RecipePresentInfo();

            bool isInProgress = progressTime >= 0;
            ResourceInfo rscInfo;
            Sprite iconSprite;

            // Build Sources.
            List<FrameResourceComp.PresentInfo> sources = new List<FrameResourceComp.PresentInfo>();
            for (int k = 0; k < recipeData.Sources.Count; ++k)
            {
                var srcInfo = recipeData.Sources[k];
                ResourceCollectInfo srcCollected = null;
                context.RequestQuery("Resource", "PlayerData.GetResourceCollectInfo", (errMsg, ret) =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                    srcCollected = (ResourceCollectInfo)ret;
                }, srcInfo.ResourceId);

                BigInteger collectedCount = BigInteger.Zero;
                if (srcCollected != null)
                    collectedCount = srcCollected.BICount;

                collectedCount = isInProgress ? srcInfo.GetCount(1.0f) : collectedCount;

                rscInfo = null;
                context.RequestQuery("Resource", "GetResourceInfo", (errMsg, ret) =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                    rscInfo = (ResourceInfo)ret;
                }, srcInfo.ResourceId);

                Assert.IsNotNull(rscInfo);
                iconSprite = IMContext.GetSprite(rscInfo.GetSpriteGroupId(), spriteKey: rscInfo.Id);
                sources.Add(new FrameResourceComp.PresentInfo(iconSprite, srcInfo.ResourceId, rscInfo.GetClassKey(), srcInfo.GetCount(1.0f), collectedCount));
            }

            // Build Target.
            rscInfo = null;
            context.RequestQuery("Resource", "GetResourceInfo", (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                rscInfo = (ResourceInfo)ret;
            }, recipeData.OutcomeId);

            Assert.IsNotNull(rscInfo);
            iconSprite = IMContext.GetSprite(rscInfo.GetSpriteGroupId(), spriteKey: rscInfo.Id);
            var target = new FrameResourceComp.PresentInfo(iconSprite, recipeData.OutcomeId, rscInfo.GetClassKey(), 1);

            // Returning Final Item Presentor data.
            return new RecipeSingleItemComp.RecipePresentInfo(sources, target, recipeData.OutcomeId, recipeData.GetDuration(1.0f));
        }

        void RecipePopupDialogOnLockSlotClicked(int idxSlot)
        {
            eRscStageType eLv = View.TabIndex == 0 ? eRscStageType.COMPONENT : eRscStageType.ITEM;
            List<RecipeInfo> purchasedRecipeList = Model.GetRecipeInfoList(eLv, purchasedOnly:true);
            if (idxSlot >= purchasedRecipeList.Count)
            {
                if (Model.PurchaseRecipeInfoSlot(eLv))
                {
                    var presentInfo = BuildRecipePopupDialogPresentInfo(eLv);
                    RecipePopupDialogCache.Refresh(presentInfo);
                }
            }
        }

        object sim_updateResourceReqStatus(params object[] args)
        {
            SIM_UpdateResourceReqStatus();
            return null;
        }


        void CollectResourceReferenceInfo(eRscStageType eLevel, Dictionary<string, int> dictRefBuffer)
        {
            List<CraftingSlot> listCraftSlots = Model.GetCraftSlotList(eLevel);
            for(int q = 0; q < listCraftSlots.Count; ++q) 
            {
                var craftSlot = listCraftSlots[q];
                if (string.IsNullOrEmpty(craftSlot.RecipeId_))   // Empty Slot.
                    continue;

                var recipeInfo = Model.GetRecipeInfo(eLevel, craftSlot.RecipeId_);
                if (recipeInfo == null)
                {
                    Assert.IsTrue(false, "Can't fine the recipe Info..." + craftSlot.RecipeId_);
                    continue;
                }

                for (int src = 0; src < recipeInfo.Sources.Count; ++src)
                {
                    string resource_id = recipeInfo.Sources[src].ResourceId.ToLower();
                    if(dictRefBuffer.ContainsKey(resource_id))  
                        dictRefBuffer[resource_id]++;
                    else 
                        dictRefBuffer.Add(resource_id, 1);
                }
            }
        }


        //==========================================================================
        //
        // Forge System Core Per 1 Sec.
        //
        //
        void UpdateSlots(eRscStageType eLv, int duration)
        {
            List<CraftingSlot> listCraftSlots = Model.GetCraftSlotList(eLv);
            
            for (int q = 0; q < listCraftSlots.Count; ++q)
            {
                var craftSlot = listCraftSlots[q];
                if (string.IsNullOrEmpty(craftSlot.RecipeId_))   // Empty Slot.
                {
                    Debug.Log($"[SIM][Status] CraftSlot {eLv} idx:{q}, Empty....");
                    continue;
                }

                var recipeInfo = Model.GetRecipeInfo(eLv, craftSlot.RecipeId_);
                if (recipeInfo == null)
                {
                    Assert.IsTrue(false, "Can't fine the recipe Info..." + craftSlot.RecipeId_);
                    continue;
                }

                if(duration > 1)
                    ProceedMultiCraftCycle(eLv, craftSlot, recipeInfo, duration, q);
                else
                {
                    if (craftSlot.ProgressedTime_ >= 0)
                        ProceedCraft(eLv, craftSlot, recipeInfo);
                    else
                        StartForgeIfReady(eLv, craftSlot, recipeInfo);
                }
            }
        }



        void ProceedMultiCraftCycle(eRscStageType eLv, CraftingSlot craftSlot, RecipeInfo recipeInfo, int duration, int idxSlot)
        {
            if (craftSlot==null || recipeInfo==null) 
                return;
            
            Assert.IsTrue(duration > 1, "Please call ProceedCraft/StartForgeIfReady instead..");

            // PROCEED.
            craftSlot.Proceed(duration);

            // GET CRAFT COUNT
            BigInteger craftCount = CalculatedCraftCount(eLv, craftSlot.ProgressedTime_, recipeInfo);
            
            if(craftCount <= 0)
            {
                // reset slot.
                Debug.Log($"[SIM][Status] : Processing Slot [{eLv}], idx:{idxSlot}, Outcome:{recipeInfo.OutcomeId} - can't craft...");
                craftSlot.Idle();
                return;
            }

            float fTimeBuff = Model.GetCraftTimeBuffRate(eLv);
            float fPriceBuff = Model.GetCraftReqResourceBuffRate(eLv);

            //Fix ME
            Debug.Log($"<color=green>[SIM][Action] : Processing Slot [{eLv}], Outcome:{recipeInfo.OutcomeId} {craftCount}EA crafted!</color>");

            // PAY
            for (int src = 0; src < recipeInfo.Sources.Count; ++src)
            {
                var srcInfo = recipeInfo.Sources[src];
                BigInteger payAmount = (BigInteger)(srcInfo.GetCount(fPriceBuff)*1000*craftCount);
                context.RequestQuery("Resource", "PlayerData.UpdateResourceX1000", (errMsg, ret) => { }, 
                        srcInfo.ResourceId, -payAmount, true);
            }

            // GET
            BigInteger getAmount = (BigInteger)(craftCount*1000);
            context.RequestQuery("Resource", "PlayerData.UpdateResourceX1000", (errMsg, ret) => { }, 
                        recipeInfo.OutcomeId, getAmount, true);
                
            // Update Progress Time
            craftSlot.ProgressedTime_ %= recipeInfo.GetDuration(fTimeBuff);
            if(craftSlot.ProgressedTime_ == 0)
                craftSlot.Idle();
        }

        BigInteger CalculatedCraftCount(eRscStageType eLv, int progressedTimeInSec, RecipeInfo recipeInfo)
        {
            float fTimeBuff = Model.GetCraftTimeBuffRate(eLv);

            // Simple Time Calculation based on the crafting duration.
            BigInteger timeCraftCount = (BigInteger)(progressedTimeInSec / recipeInfo.GetDuration(fTimeBuff));
            
            // Caculation based on the count of the resource the player has ATM.
            BigInteger resourceCraftCount = DoableForgeCountForResource(eLv, recipeInfo);
            
            // Select the shorter one. 
            BigInteger result = BigInteger.Min(timeCraftCount, resourceCraftCount);
            
            return result;
        }


        BigInteger DoableForgeCountForResource(eRscStageType eLv, RecipeInfo recipeInfo)
        {
            if (recipeInfo == null)
                return 0;

            Assert.IsTrue(recipeInfo.Sources.Count > 0);

            float fReqResourceBuff = Model.GetCraftReqResourceBuffRate(eLv);

            List<BigInteger> amountHave = new List<BigInteger>();
            List<BigInteger> amountCost = new List<BigInteger>();

            // Looking for starting craft.
            string countStatus = $"[SIM][Status] Craft {eLv} OutCome:{recipeInfo.OutcomeId} Sources:[[[ ";

            for (int src = 0; src < recipeInfo.Sources.Count; ++src)
            {
                amountCost.Add(recipeInfo.Sources[src].GetCount(fReqResourceBuff));

                string resource_id = recipeInfo.Sources[src].ResourceId.ToLower();
                ResourceInfo rscInfo = null;
                context.RequestQuery("Resource", endPoint:"GetResourceInfo", (errorMsg, ret) => 
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errorMsg));
                    rscInfo = (ResourceInfo)ret;
                }, resource_id);

                eRscStageType rscLevel = rscInfo.eLevel;

                ResourceCollectInfo collectionInfo = null;
                context.RequestQuery("Resource", "PlayerData.GetResourceCollectInfo", (errMsg, ret) =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                    if(ret != null) 
                        collectionInfo = (ResourceCollectInfo)ret;

                }, resource_id);
        
                BigInteger haveCount = collectionInfo==null ? 0 : collectionInfo.BICount;

                BigInteger cost = amountCost[amountCost.Count-1];
                BigInteger available;
                if(false == context.IsSimulationMode())
                {
                    if(dictResumeRscRefInfo.ContainsKey(resource_id) && haveCount>dictResumeRscRefInfo[resource_id])
                        available = haveCount / dictResumeRscRefInfo[resource_id];
                }
                else
                {
#if UNITY_EDITOR
                    // SIM MODE ONLY !!!
                    //
                    if (rscLevel < eLv)
                    {
                        // Need to device this count from the referenced count so it can be distributed evenly.
                        // Lower level resource (Material/Component) used in higher level craft (Component/Item)
                        // Need to share with other crafts
                        available = CalculateSharedResource(haveCount, resource_id, rscLevel, eLv, cost, recipeInfo);
                    }
                    else
                        available = haveCount;
#endif
                }
                amountHave.Add(available);
            }

            BigInteger result = CalculateMinForgeCount(amountHave, amountCost);
            return result;
        }

        
        

        BigInteger CalculateMinForgeCount(List<BigInteger> amountHave, List<BigInteger> amountCost)
        {
            Assert.IsNotNull(amountCost);
            Assert.IsNotNull(amountHave);
            Assert.IsTrue(amountCost.Count > 0 && amountCost.Count == amountHave.Count);
            
            // Looking for starting craft.
            BigInteger resultCount = BigInteger.Zero;
            for (int q = 0; q < amountHave.Count; ++q)
            {   
                BigInteger count = amountHave[q] / amountCost[q];
                resultCount = q==0 ? count : BigInteger.Min(count, resultCount);
            }
            return resultCount;
        }

        void ProceedCraft(eRscStageType eLv, CraftingSlot craftSlot, RecipeInfo recipeInfo)
        {
            if (craftSlot == null) return;

            float fTimeBuff = Model.GetCraftTimeBuffRate(eLv);

            // 1 sec proceed.
            craftSlot.Proceed(1);
            if(craftSlot.ProgressedTime_ >= recipeInfo.GetDuration(fTimeBuff))
            {
                context.RequestQuery("Resource", "PlayerData.UpdateResourceX1000", (errMsg, ret) => { }, 
                        recipeInfo.OutcomeId, (BigInteger)(1000), true);

                craftSlot.Spend(recipeInfo.GetDuration(fTimeBuff));    
                craftSlot.Idle();

                EventSystem.DispatchEvent(EventID.CRAFT_SUCCESSED, eLv);
            }
        }

        void StartForgeIfReady(eRscStageType eLv, CraftingSlot craftSlot, RecipeInfo recipeInfo)
        {
            if (craftSlot == null || recipeInfo == null)
                return;

            Assert.IsTrue(craftSlot.ProgressedTime_ == -1, "This should be call only when the socket is idle." );

            float fPriceBuff = Model.GetCraftReqResourceBuffRate(eLv);

            // Looking for starting craft.
            if (IsReadyToForge(eLv, craftSlot, recipeInfo))
            {
                for (int src = 0; src < recipeInfo.Sources.Count; ++src)
                {
                    var srcInfo = recipeInfo.Sources[src];
                    context.RequestQuery("Resource", "PlayerData.UpdateResourceX1000", (errMsg, ret) => { }, 
                        srcInfo.ResourceId, (BigInteger)(-srcInfo.GetCount(fPriceBuff)*1000), true);
                }
                craftSlot.Start();
            }
        }

        bool IsReadyToForge(eRscStageType eLv, CraftingSlot craftSlot, RecipeInfo recipeInfo)
        {
            if (craftSlot == null || recipeInfo == null)
                return false;

            float fPriceBuff = Model.GetCraftReqResourceBuffRate(eLv);

            // Looking for starting craft.
            for (int src = 0; src < recipeInfo.Sources.Count; ++src)
            {
                ResourceCollectInfo collectionInfo = null;
                context.RequestQuery("Resource", "PlayerData.GetResourceCollectInfo", (errMsg, ret) =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                    collectionInfo = (ResourceCollectInfo)ret;
                }, recipeInfo.Sources[src].ResourceId);

                if (collectionInfo == null || collectionInfo.BICount < recipeInfo.Sources[src].GetCount(fPriceBuff))
                    return false;
            }
            return true;
        }

        IEnumerator coTriggerActionWithDelay(float delay, System.Action action)
        {
            if(delay < .0f) yield return null;
            else            yield return new WaitForSeconds(delay);

            action?.Invoke();
        }



        void TestCraftPumpResumeLogic()
        {
            Assert.IsTrue(CalculateMinForgeCount(amountHave:new List<BigInteger> { 10 }, amountCost:new List<BigInteger> { 2 }) == 5);
            Assert.IsTrue(CalculateMinForgeCount(amountHave: new List<BigInteger> { 10, 5 }, amountCost: new List<BigInteger> { 2, 5 }) == 1);
            Assert.IsTrue(CalculateMinForgeCount(amountHave: new List<BigInteger> { 10, 20 }, amountCost: new List<BigInteger> { 5, 2 }) == 2);



            Debug.Log("===== Craft Pump Resume Logic Test has been PASSED Successfully !!! =====");
        }











        
        #region SIMULATOR
        // Defining of reserved resources. 
        //
        // - 시뮬레이션은 사람의 행동 방식 차용한다.
        // - 한번에 하나의 스킬트리 해금에 총력을 기울인다.
        // - 진행중인 모든 skill node의 필요한 자원을 craft 의존성을 포함하여 파악한다.
        // - 가장 낮은 단계의 채굴 material자원을 필요로하는 스킬노드를 target 노드로 한다.
        // - 이 타겟노드에 관련된 모든 자원을 reserve 한다.
        // - 이외 자원은 PDR 에 기반해서 판매한다. 
        //
        void SIM_UpdateResourceReqStatus()
        {
            List<SkillInfo> workingSkills = null;
            context.RequestQuery(unitName:"SkillTree", endPoint:"SIM_CollectAllWorkingNodes",  (errMsg, ret) =>
            {
                Assert.IsTrue(string.IsNullOrEmpty(errMsg) && ret!=null);
                workingSkills = (List<SkillInfo>)ret;
            });

            if(workingSkills.Count == 0)
            {
                Debug.Log($"[SkillTree] - Looks like all skill tree nodes have been unlocked !!!");
                return;
            }
            _sim_updateResourceReqStatusForSkills(workingSkills);
        }

        void _sim_updateResourceReqStatusForSkills(List<SkillInfo> workingSkillNodes)
        {
            if(workingSkillNodes == null)       return;

            HashSet<string> resourcesShouldBeReserved = new HashSet<string>();
            int minZone = int.MaxValue;    int minPlanet = int.MaxValue;
            string focusingSkillNodeId = string.Empty;
            HashSet<string> recipesInChain = new HashSet<string>();

            for(int q = 0; q < workingSkillNodes.Count; q++)
            {
                Dictionary<string, int> skillNodeCosts = null;
                
                // Collect Cost Resources from the SkillNode.
                context.RequestQuery(unitName:"SkillTree", endPoint:"SIM_GetRequiredResources",  (errMsg, ret) =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errMsg) && ret!=null);
                    skillNodeCosts = (Dictionary<string, int>)ret;

                }, workingSkillNodes[q].Id);

                Debug.Log($"[SIM] Collecting [{workingSkillNodes[q].Id}] required resouces info....");

                Dictionary< string, int> reqResourcesFromCraft = new Dictionary<string, int>(skillNodeCosts);
                List<string> matResources = new List<string>();
                recipesInChain.Clear();
                    
                // In order to get above resources, collect all sub resources from the craft chain
                // - All required resources should fall in the reqResourcesFromCraft + matResources.
                CollectResourcesInfoInTheCraftChain(skillNodeCosts, ref reqResourcesFromCraft, ref matResources, ref recipesInChain);

                Assert.IsTrue(matResources.Count>0, $"Material Resource should not be ZERO ! - SkillNode:[{workingSkillNodes[q].Id}]");

                bool refreshBuffer = false;

                for(int z = 0; z < matResources.Count; z++)
                {
                    string matRscId = matResources[z];

                    List<Tuple<int, int>> zonePlanets = null;
                    context.RequestQuery("GamePlay", "FindCollectableZonePlanetId", (errMsg, ret) =>
                    {
                        Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                        zonePlanets = (List<Tuple<int, int>>)ret;

                    }, matRscId);
                
                    //
                    // We only focus on the requried resources for the skill_node where its req resources can get from the MIN zone & planet.
                    // -> Pursuing the Easiest one first.
                    for(int k = 0; k < zonePlanets.Count; k++)
                    {
                        Tuple<int, int> tupleZ_P = zonePlanets[k];
                        if(tupleZ_P.Item1 < minZone)
                        {
                            minZone = tupleZ_P.Item1;
                            minPlanet = tupleZ_P.Item2;
                            Debug.Log($"[SIM][Zone] Min Zone Item has been updated due to [{matRscId}] zone [{tupleZ_P.Item1}], planet [{tupleZ_P.Item2}] ....");
                            refreshBuffer = true;
                        }
                        else if(tupleZ_P.Item1 == minZone)
                        {
                            if(tupleZ_P.Item2 < minPlanet)
                            {
                                minPlanet = tupleZ_P.Item2;
                                Debug.Log($"[SIM][Planet] Min Zone Item has been updated due to [{matRscId}] zone [{tupleZ_P.Item1}], planet [{tupleZ_P.Item2}] ....");
                                refreshBuffer = true;
                            }
                        }
                        // Do not break here.
                    }
                }

                // Updated the easiest one.
                if(refreshBuffer)
                {
                    // Update Target Resources & store it in the buffer.
                    resourcesShouldBeReserved.Clear();
                    resourcesShouldBeReserved.AddRange(matResources);
                    foreach(string key in reqResourcesFromCraft.Keys)
                        resourcesShouldBeReserved.Add(key);   

                    focusingSkillNodeId = workingSkillNodes[q].Id;

                    context.AddData(SimDefines.SIM_RECIPES_IN_CRAFT_CHAIN, new HashSet<string>(recipesInChain));
                    context.AddData(SimDefines.SIM_SKILLTREE_EASIEST_NODES_REQ_RESOURCES, new HashSet<string>(resourcesShouldBeReserved));
                    context.AddData(SimDefines.SIM_SKILLTREE_TARGET_NODE_NAME, focusingSkillNodeId);
                }
            }


            // Logger.
            string strReqRsc = "";
            foreach(var resourceId in resourcesShouldBeReserved)
                strReqRsc += $"{resourceId}, ";
            Debug.Log($"<color=grey>[SIM][Focusing Skill] id:[{focusingSkillNodeId}], {resourcesShouldBeReserved.Count}:{strReqRsc} </color>");
        }

        void SIM_AssignRecipesFromSlots(HashSet<string> usingRecipes) 
        {
            _sim_assignRecipesFromSlotsIfNotNeeded(eRscStageType.ITEM, usingRecipes);
            _sim_assignRecipesFromSlotsIfNotNeeded(eRscStageType.COMPONENT, usingRecipes);
        }

        void _sim_assignRecipesFromSlotsIfNotNeeded(eRscStageType eType, HashSet<string> usingRecipes)
        {
            if(usingRecipes == null)
                return;

            var slots = eType==eRscStageType.COMPONENT ? Model.PlayerData.CraftingCompSlots : Model.PlayerData.CraftingItemSlots;
            
            for(int q = 0; q < slots.Count; q++) 
            {
                var recipeInfo = Model.GetAssignedRecipeOnSlot(eType, q);
                if(recipeInfo == null)
                    continue;       // Empty slot.

                Model.UnassignRecipeToSlot(eType, q);
            }

            int idx = 0;
            foreach(string recipe_id in usingRecipes) 
            {
                var info = Model.GetRecipeInfo(eType, recipe_id);
                if(info == null)
                    continue;
                
                Model.AssignRecipeToSlot(eType, idx, info.Id);
                idx++;
            }
        }

        List<CraftRequirement> FindAllCraftsNeedingResource(string resourceId, float buffRate)
        {
            List<CraftRequirement> result = new List<CraftRequirement>();
            resourceId = resourceId.ToLower();
    
            for(int q = 0; q < Model.CompCraftData.Recipes.Count; q++)
            {
                var recipe = Model.CompCraftData.Recipes[q];
    
                for(int src = 0; src < recipe.Sources.Count; src++)
                {
                    if(recipe.Sources[src].ResourceId.ToLower() == resourceId)
                    {
                        result.Add(new CraftRequirement
                        {
                            recipeId = recipe.Id,
                            outcomeId = recipe.OutcomeId,
                            level = eRscStageType.COMPONENT,
                            requiredAmount = recipe.Sources[src].GetCount(buffRate), 
                            recipeInfo = recipe
                        });
                        break;  
                    }
                }
            }
    
            for(int q = 0; q < Model.ItemCraftData.Recipes.Count; q++)
            {
                var recipe = Model.ItemCraftData.Recipes[q];
    
                for(int src = 0; src < recipe.Sources.Count; src++)
                {
                    if(recipe.Sources[src].ResourceId.ToLower() == resourceId)
                    {
                        result.Add(new CraftRequirement
                        {
                            recipeId = recipe.Id,
                            outcomeId = recipe.OutcomeId,
                            level = eRscStageType.ITEM,
                            requiredAmount = recipe.Sources[src].GetCount(buffRate),
                            recipeInfo = recipe
                        });
                        break;  
                    }
                }
            }
    
            return result;
        }
        
        // Calculate craft difficulty based on resource requirements
        // Easier crafts: fewer resource types, smaller amounts, mining-obtainable resources
        float CalculateCraftDifficulty(RecipeInfo recipeInfo)
        {
            if(recipeInfo == null || recipeInfo.Sources == null || recipeInfo.Sources.Count == 0)
                return float.MaxValue;
            
            float totalDifficulty = 0.0f;
            int resourceTypeCount = recipeInfo.Sources.Count;
            BigInteger totalAmount = 0;
            int miningObtainableCount = 0;
            
            foreach(var source in recipeInfo.Sources)
            {
                totalAmount += source.GetCount(1.0f);
                
                // Check if resource is obtainable through mining (not craftable)
                string resourceId = source.ResourceId.ToLower();
                RecipeInfo miningRecipe = FindRecipeInfo(eRscStageType.COMPONENT, outcomeRscId: resourceId);
                if(miningRecipe == null)
                {
                    miningRecipe = FindRecipeInfo(eRscStageType.ITEM, outcomeRscId: resourceId);
                }
                
                // If no recipe found, it's mining-only (easier)
                if(miningRecipe == null)
                {
                    miningObtainableCount++;
                }
            }
            
            // Difficulty factors:
            // 1. Resource type count (more types = harder): 0.0 ~ 0.3
            float typeDifficulty = (resourceTypeCount - 1) * 0.15f; // 1 type = 0.0, 2 types = 0.15, 3 types = 0.3
            
            // 2. Total amount (more amount = harder): 0.0 ~ 0.3
            // Normalize by typical amounts (assume 100 is "normal")
            float amountDifficulty = Mathf.Min(0.3f, (float)totalAmount / 100.0f * 0.3f);
            
            // 3. Mining-obtainable ratio (more mining = easier): 0.0 ~ 0.4
            float miningRatio = resourceTypeCount > 0 ? (float)miningObtainableCount / (float)resourceTypeCount : 0.0f;
            float miningDifficulty = (1.0f - miningRatio) * 0.4f; // All mining = 0.0, all craftable = 0.4
            
            totalDifficulty = typeDifficulty + amountDifficulty + miningDifficulty;
            
            return totalDifficulty;
        }
        
        BigInteger CalculateSharedResource(BigInteger totalAvailable,string resourceId,
                                            eRscStageType resourceLevel, eRscStageType craftLevel,
                                            BigInteger currentRequirementAmount,
                                            RecipeInfo currentRecipeInfo = null)
        {
            // Find all crafts that need this resource (regardless of level)
            float fReqResourceBuff = Model.GetCraftReqResourceBuffRate(craftLevel);
            var allCraftsNeedingThis = FindAllCraftsNeedingResource(resourceId, fReqResourceBuff);
    
            if(allCraftsNeedingThis.Count == 0)
                return 0;
            
            // OPTION 1: Unified Allocation - All crafts compete equally based on difficulty
            // Collect all crafts with their difficulties (no level separation)
            List<(CraftRequirement craft, float difficulty)> allCraftsWithDifficulty = new List<(CraftRequirement, float)>();
            
            foreach(var craft in allCraftsNeedingThis)
            {
                float difficulty = CalculateCraftDifficulty(craft.recipeInfo);
                allCraftsWithDifficulty.Add((craft, difficulty));
            }
            
            // Sort by difficulty (easier first) for logging
            allCraftsWithDifficulty.Sort((a, b) => a.difficulty.CompareTo(b.difficulty));
            
            // Calculate weighted allocation for ALL crafts together
            // Weight = 2.0 - difficulty (easier crafts get higher weight)
            // Weighted value = weight * requirement
            double totalWeightedValue = 0.0;
            List<(CraftRequirement craft, float difficulty, float weight, double weightedValue)> craftAllocations = 
                new List<(CraftRequirement, float, float, double)>();
            
            foreach(var (craft, difficulty) in allCraftsWithDifficulty)
            {
                float weight = 2.0f - difficulty; // Easier = higher weight (0.0 difficulty -> 2.0 weight, 1.0 difficulty -> 1.0 weight)
                double weightedValue = (double)craft.requiredAmount * (double)weight;
                craftAllocations.Add((craft, difficulty, weight, weightedValue));
                totalWeightedValue += weightedValue;
            }
            
            // Allocate resources proportionally based on weighted values
            // But don't exceed each craft's requirement
            BigInteger totalAllocated = 0;
            Dictionary<string, BigInteger> craftAllocationMap = new Dictionary<string, BigInteger>();
            
            // First pass: allocate based on weighted ratios
            foreach(var (craft, difficulty, weight, weightedValue) in craftAllocations)
            {
                if(totalWeightedValue > 0 && totalAvailable > 0)
                {
                    double ratio = weightedValue / totalWeightedValue;
                    BigInteger allocated = (BigInteger)((double)totalAvailable * ratio);
                    
                    // Don't exceed craft's requirement
                    allocated = BigInteger.Min(allocated, craft.requiredAmount);
                    
                    craftAllocationMap[craft.recipeId] = allocated;
                    totalAllocated += allocated;
                }
                else
                {
                    craftAllocationMap[craft.recipeId] = 0;
                }
            }
            
            // Second pass: distribute remaining resources to crafts that haven't reached their requirement
            // Prioritize easier crafts
            BigInteger remaining = totalAvailable - totalAllocated;
            if(remaining > 0)
            {
                // Sort by difficulty (easier first) for remaining allocation
                var sortedForRemaining = craftAllocations
                    .Where(x => craftAllocationMap[x.craft.recipeId] < x.craft.requiredAmount)
                    .OrderBy(x => x.difficulty)
                    .ToList();
                
                foreach(var (craft, difficulty, weight, weightedValue) in sortedForRemaining)
                {
                    if(remaining <= 0) break;
                    
                    BigInteger currentAllocated = craftAllocationMap[craft.recipeId];
                    BigInteger stillNeeded = craft.requiredAmount - currentAllocated;
                    
                    if(stillNeeded > 0)
                    {
                        BigInteger additional = BigInteger.Min(remaining, stillNeeded);
                        craftAllocationMap[craft.recipeId] = currentAllocated + additional;
                        totalAllocated += additional;
                        remaining -= additional;
                    }
                }
            }
            
            // Find current craft's allocation
            BigInteger currentCraftAllocation = 0;
            if(currentRecipeInfo != null)
            {
                // Find the craft that matches current recipe
                foreach(var (craft, _, _, _) in craftAllocations)
                {
                    if(craft.recipeInfo != null && craft.recipeInfo.Id == currentRecipeInfo.Id)
                    {
                        currentCraftAllocation = craftAllocationMap.ContainsKey(craft.recipeId) ? craftAllocationMap[craft.recipeId] : 0;
                        break;
                    }
                }
            }
            else
            {
                // If no recipe info, try to find by matching requirement amount (less accurate)
                foreach(var (craft, _, _, _) in craftAllocations)
                {
                    if(craft.level == craftLevel && craft.requiredAmount == currentRequirementAmount)
                    {
                        currentCraftAllocation = craftAllocationMap.ContainsKey(craft.recipeId) ? craftAllocationMap[craft.recipeId] : 0;
                        break;
                    }
                }
            }
            
            // Don't exceed current requirement
            currentCraftAllocation = BigInteger.Min(currentCraftAllocation, currentRequirementAmount);
            return currentCraftAllocation;
        }

        RecipeInfo FindRecipeInfo(eRscStageType eType, string outcomeRscId)
        {
            var craftData = eType==eRscStageType.COMPONENT ? Model.CompCraftData : Model.ItemCraftData;
            
            for(int q = 0; q < craftData.Recipes.Count; q++) 
            {
                var craftRecipeData = craftData.Recipes[q];
                if(0 == string.Compare(craftRecipeData.OutcomeId, outcomeRscId, ignoreCase:true))
                    return craftRecipeData;
            }
            return null;
        }

        void addFrequencyKey(ref Dictionary<string, int> dictBuff, string key, int offset)
        {
            if(false == dictBuff.ContainsKey(key))
                dictBuff.Add(key, offset);
            else 
                dictBuff[key] += offset;
        }

        void CollectResourcesInfoInTheCraftChain(Dictionary<string, int> reqResourcesForTheSkillNode, ref Dictionary<string, int> rscForCrafts, ref List<string> materialResources, ref HashSet<string> recipesInChain)
        {
            Assert.IsTrue(context.IsSimulationMode());
    
            Dictionary<string, int> listCopied = new Dictionary < string, int>(reqResourcesForTheSkillNode);
            foreach(string rscId in listCopied.Keys)
                innerAddResourcesInTheCraftChain(rscId, ref rscForCrafts, ref materialResources, ref recipesInChain);
        }

        void innerAddResourcesInTheCraftChain(string rscId, ref Dictionary<string, int> dictResourceForCraft, ref List<string> matResources, ref HashSet<string> recipesInChain)
        {
            Assert.IsTrue(context.IsSimulationMode());
            Assert.IsNotNull(dictResourceForCraft);

            var recipeInfo = FindRecipeInfo(eRscStageType.ITEM, outcomeRscId:rscId);
            if(recipeInfo == null)
            {
                recipeInfo = FindRecipeInfo(eRscStageType.COMPONENT, outcomeRscId:rscId);
                if(recipeInfo == null)
                {
                    // Couldn't find the resource in the crafts info? ===> Then should be able to get this from minning.
                    //
                    // addFrequencyKey(ref dictResourceForCraft, rscId);

                    ResourceInfo rscInfo = null;
                    context.RequestQuery("Resource", "GetResourceInfo", (errMsg, ret) =>
                    {
                        Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                        rscInfo = (ResourceInfo)ret;
                    }, rscId);
                    Assert.IsTrue(rscInfo!=null && rscInfo.eLevel==eRscStageType.MATERIAL, $"There're no way to get the resource [{rscId}] !!!");

                    matResources.Add(rscId);
                    Debug.Log($"<color=green>[SIM][Action] Adding Resource:[{rscId}] into the MINING-REQ hash....</color>");
                    return;
                }
            }

            if(!recipesInChain.Contains(recipeInfo.Id.ToLower()))
                recipesInChain.Add(recipeInfo.Id.ToLower());

            for(int q = 0; q < recipeInfo.Sources.Count; ++q)
            {
                string inputRscId = recipeInfo.Sources[q].ResourceId;
                /*
                ResourceCollectInfo srcCollected = null;
                context.RequestQuery("Resource", "PlayerData.GetResourceCollectInfo", (errMsg, ret) =>
                {
                    Assert.IsTrue(string.IsNullOrEmpty(errMsg));
                    srcCollected = (ResourceCollectInfo)ret;
                }, inputRscId);
                */
                // Already has enough ?? => Then, skip searching in the next craft chain.
                bool skip_searching = false;// srcCollected!=null && srcCollected.BICount>=recipeInfo.Sources[q].GetCount();
                    
                inputRscId = inputRscId.ToLower();
                addFrequencyKey(ref dictResourceForCraft, inputRscId, (int)recipeInfo.Sources[q].GetCount(1.0f));
 
                Debug.Log($"<color=yellow>[SIM][Action] Craft RscId : [{inputRscId}] has been added into the CRAFT-REQ hash.!...Ref Count:[{dictResourceForCraft[inputRscId]}] </color>");
                
                if (false == skip_searching)
                    innerAddResourcesInTheCraftChain(inputRscId, ref dictResourceForCraft, ref matResources, ref recipesInChain);
            }
        }

        #endregion

        

    }
}

/*


BigInteger DoableForgeCountForResource(eRscStageType eLv, RecipeInfo recipeInfo)
{
    // ...
}
// CollectReqResourcesFromSkillTree ����
void CollectReqResourcesFromSkillTree()
{
    Assert.IsTrue(context.IsSimulationMode());    
}

// AddResourcesInTheCraftChain ����
void AddResourcesInTheCraftChain(
    Dictionary<string, int> neededResourcesFromSkillTree, 
    ref Dictionary<string, int> rscForCrafts)
{
    
}*/