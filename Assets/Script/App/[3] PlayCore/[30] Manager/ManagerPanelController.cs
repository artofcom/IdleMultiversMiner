using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Core.Events;
using App.GamePlay.IdleMiner.PopupDialog;

namespace App.GamePlay.IdleMiner
{
    //  SkillTree Controlling.-------------------------------------
    //
    public class ManagerPanelController : IGCore.MVCS.AView
    {
        IdleMinerController controller;

        EventsGroup Events = new EventsGroup();
        ManagerPanelView ManagerPnlView = null;
        int CurPlanetId = -1;
        string SelectedManagerInfoId = string.Empty;
        List<string> SelecteManagerInfoIdList = new List<string>();
        PromoteManagerPopupDialog PromoteDlg = null;

        IdleMinerModel Model => controller?.Model;

        public IncreaseManagerSlotDialog IncreaseSlotDlg { get; private set; } = null;


        public override void Refresh(APresentor presentData)
        {}  

        public ManagerPanelController(IdleMinerController _controller)
        {
            controller = _controller;

            InitSystem();
        }

        void ManagerPanelView_OnEnabled(object data)
        {
            if (data == null)
                return;
            /*
            ManagerPnlView = (ManagerPanelView)data;
            CurPlanetId = Model.GetNeighborPlanetId(CurPlanetId, isUpper: true);
            PromoteDlg = null;
            IncreaseSlotDlg = null;
            */
            RefreshView();
        }

        void ManagerPanelView_OnPlanetBrowseToLeft()
        {
            //CurPlanetId = Model.GetNeighborPlanetId(CurPlanetId, isUpper: false);
            RefreshView();
        }
        // 
        void ManagerPanelView_OnPlanetBrowseToRight()
        {
            //CurPlanetId = Model.GetNeighborPlanetId(CurPlanetId, isUpper: true);
            RefreshView();
        }

        //
        // Planet Manager View----------------------------------------
        //
        //
        // Un-hire manager from the planet.
        void ManagerPanelView_OnBtnPlanetManagerClicked()
        {
            if (CurPlanetId < 0)
                return;

          //  Model.PlayerData.HireManagerToPlanet(string.Empty, CurPlanetId);
            RefreshView();
        }


        void ManagerPanelView_OnBtnManagerImageClicked()
        {
            if (CurPlanetId < 0)
                return;

         //   Model.PlayerData.HireManagerToPlanet(string.Empty, CurPlanetId, isHiring:false);
            RefreshView();
        }

        // Recruit Handler----------------------------------------
        //
        //
        // Adding new Manager to player.
        void ManagerPanelView_OnBtnRecruitClicked()
        {
            /*
            var recruitInfo = Model.ManagerData.RecruitInfo;
            Assert.IsTrue(recruitInfo.Products.Count >= RecruitManagerPopupDialog.SUPPORTING_ITEM_COUNT);

            List<RecruitManagerItemComp.PresentInfo> listItemCompData = new List<RecruitManagerItemComp.PresentInfo>();
            for (int q = 0; q < RecruitManagerPopupDialog.SUPPORTING_ITEM_COUNT; ++q)
            {
                var product = recruitInfo.Products[q];
                RecruitManagerItemComp.PresentInfo info = new RecruitManagerItemComp.PresentInfo(product.Id,
                    product.Name, product.Desc, $"RECRUI \n{product.Cost.Amount}",
                    Model.PlayerData.IsAffordable(product.Cost));
                listItemCompData.Add(info);
            }

            RecruitManagerPopupDialog.PresentInfo presentInfo = new RecruitManagerPopupDialog.PresentInfo(listItemCompData[0], listItemCompData[1], listItemCompData[2], listItemCompData[3] );
            controller.Context.PopUpScreen.DisplayPopupDialog(RecruitManagerPopupDialog.sID, presentInfo, (dlg) => { });
            */
        }


        // Increase Slot Handler----------------------------------------
        //
        //
        // Adding new slot capacity.
        void ManagerPanelView_OnBtnAddSlotClicked()
        {
         //   var mngSlots = Model.PlayerData.ManagerSlots;
         //   int slotCount = mngSlots != null ? mngSlots.Count : 0;

            /*int cost = Model.ManagerData.SlotInfo.GetCost(slotCount + 1);

            CurrencyAmount costAmount = new CurrencyAmount(cost.ToString(), (eCurrencyType)Model.ManagerData.SlotInfo.CostType);
            bool affordable = Model.PlayerData.IsAffordable(costAmount);
            IncreaseManagerSlotDialog.PresentInfo presentInfo = new IncreaseManagerSlotDialog.PresentInfo(cost.ToString(), affordable);

            IncreaseSlotDlg = (IncreaseManagerSlotDialog)controller.Context.PopUpScreen.DisplayPopupDialog(IncreaseManagerSlotDialog.sID, presentInfo, (dlg) =>
            {
                IncreaseManagerSlotDialog incDlg = (IncreaseManagerSlotDialog)dlg;
                if (incDlg.IsPurchasedClicked)
                {
                    Debug.Log("Trying to Purchase more manager slot...");
                    if (Model.TryPurchaseManagerSlot(costAmount))
                        RefreshView();
                }
                else
                    Debug.Log("IncreaseManagerSlot has been closed.");
            });*/
        }



        // Manager Card group > Hire ----------------------------------------
        //
        //
        // Assign Manager with the planet. (ManagerInfoId, planetId)
        void ManagerPanelView_OnBtnHireClicked()
        {
            if (string.IsNullOrEmpty(SelectedManagerInfoId))
                return;
            if (CurPlanetId < 0)
                return;

          //  Model.PlayerData.HireManagerToPlanet(SelectedManagerInfoId, CurPlanetId);
            RefreshView();
        }

        // Manager Card group > Prmote ----------------------------------------
        //
        //
        void ManagerPanelView_OnBtnPromoteClicked()
        {
            if (string.IsNullOrEmpty(SelectedManagerInfoId))
                return;

            SelecteManagerInfoIdList.Clear();

            PromoteManagerPopupDialog.PresentInfo presentInfo = BuildPromoteDialogPresentInfo(SelectedManagerInfoId);
            Assert.IsNotNull(presentInfo);

            /*
            PromoteDlg = (PromoteManagerPopupDialog)controller.Context.PopUpScreen.DisplayPopupDialog(PromoteManagerPopupDialog.sID, presentInfo, (dlg) =>
            {
                PromoteDlg = null;
                RefreshView();
            });*/
        }

        // Manager Card group > Discard ----------------------------------------
        //
        //
        void ManagerPanelView_OnBtnDiscardClicked()
        {
            if (string.IsNullOrEmpty(SelectedManagerInfoId))
                return;
            /*
            controller.Context.PopUpScreen.DisplayPopupDialog(MessageDialog.sID, 
                new MessageDialog.PresentInfo(
                    message : "Are you sure? \nThis action cannot be undone.", 
                    title : "Confirm", 
                    type : MessageDialog.Type.CONFIRM, 
                    () =>
                    {
                        Model.PlayerData.DiscardManager(SelectedManagerInfoId);
                        RefreshView();

                    }), null);
            */
        }

        // Manager Card group View----------------------------------------
        //
        //
        void ManagerItemComp_OnBtnCardClicked(string managerInfoId)
        {
            if (PromoteDlg != null)
            {
                if (SelecteManagerInfoIdList.Contains(managerInfoId))
                    SelecteManagerInfoIdList.Remove(managerInfoId);
                else
                    SelecteManagerInfoIdList.Add(managerInfoId);

                PromoteDlg.Refresh(BuildPromoteDialogPresentInfo(SelectedManagerInfoId));
            }
            else
            {
                if (!string.IsNullOrEmpty(SelectedManagerInfoId) && SelectedManagerInfoId == managerInfoId)
                    SelectedManagerInfoId = string.Empty;
                else
                    SelectedManagerInfoId = managerInfoId;

                RefreshView();
            }
        }

        void PromoteManagerPopupDialog_OnBtnPromoteClicked()
        {
            /*
            Assert.IsNotNull(PromoteDlg);
            Debug.Log("PromoteManagerPopupDialog_OnBtnPromoteClicked) Promoting " + SelectedManagerInfoId);

            OwnedManagerInfo mainChr = Model.PlayerData.GetOwnedManagerInfo(SelectedManagerInfoId);
            int reqCount = Model.GetRequiredOtherCardToPromote(mainChr.Level);

            Assert.IsTrue(SelecteManagerInfoIdList.Count >= reqCount, "Insufficient Manager Count!");
            if (SelecteManagerInfoIdList.Count < reqCount)
                return;

            while (SelecteManagerInfoIdList.Count > reqCount)
                SelecteManagerInfoIdList.RemoveAt(SelecteManagerInfoIdList.Count - 1);

            if (Model.Promote(SelectedManagerInfoId, SelecteManagerInfoIdList))
                DisplayManagerCard(SelectedManagerInfoId, () => PromoteDlg.OnClose());
            else
                Debug.Log("Promte has been failed...." + SelectedManagerInfoId);*/
        }

        public void RefreshView()
        {
            /*
            if (ManagerPnlView == null)
                return;

            PlanetData planetData = Model.GetPlanetData(CurPlanetId);
            string strPlanetName = planetData == null ? string.Empty : planetData.Name;

            var listItem3Presentor = new List<ManagerList3ItemComp.PresentInfo>();

            List<OwnedManagerInfo> listMng = Model.PlayerData.OwnedManagers;
            for (int q = 0; q < listMng.Count; q += 3)
            {
                List<ManagerItemComp.PresentInfo> list3PresentsInfo = new List<ManagerItemComp.PresentInfo>();

                int idx = q;
                ManagerItemComp.PresentInfo presentInfo0 = null;
                if (idx < listMng.Count)
                    presentInfo0 = BuildManagerItemCompPresentInfo(listMng[idx].Id, SelectedManagerInfoId == listMng[idx].Id);
                list3PresentsInfo.Add(presentInfo0);

                ++idx;
                ManagerItemComp.PresentInfo presentInfo1 = null;
                if (idx < listMng.Count)
                    presentInfo1 = BuildManagerItemCompPresentInfo(listMng[idx].Id, SelectedManagerInfoId == listMng[idx].Id);
                list3PresentsInfo.Add(presentInfo1);

                ++idx;
                ManagerItemComp.PresentInfo presentInfo2 = null;
                if (idx < listMng.Count)
                    presentInfo2 = BuildManagerItemCompPresentInfo(listMng[idx].Id, SelectedManagerInfoId == listMng[idx].Id);
                list3PresentsInfo.Add(presentInfo2);

                listItem3Presentor.Add(new ManagerList3ItemComp.PresentInfo(list3PresentsInfo));
            }


            // Planet Info.
            //
            float miningRatePerSec = planetData == null ? .0f : Model.GetPlanetStat(planetData.Id, eABILITY.MINING_RATE);
            float deliverySpeed = planetData == null ? .0f : Model.GetPlanetStat(planetData.Id, eABILITY.DELIVERY_SPEED);
            float cargoSize = planetData == null ? .0f : Model.GetPlanetStat(planetData.Id, eABILITY.CARGO_SIZE);
            Sprite planetSprite = planetData == null ? null : controller.View.GetPlanetSprite(planetData.Id);
            var planetInfo = planetData==null ? null : Model.PlayerData.GetVisiblePlanetInfo(planetData.Id);

            // Planet Manager.
            var ownedMngInfo = Model.PlayerData.GetAssignedManagerInfoForPlanet(CurPlanetId);
            //ManagerItemComp.PresentInfo planetMngCard = BuildManagerItemCompPresentInfo(ownedMngInfo == null ? string.Empty : ownedMngInfo.Id, selected: false);


            OwnedManagerInfo selectedMgr = Model.PlayerData.GetOwnedManagerInfo(SelectedManagerInfoId);

            var presentor = new ManagerPanelView.PresentInfo( //strPlanetName, miningRatePerSec, deliverySpeed, cargoSize, planetSprite, planetMngCard,
                BuildPlanetSectionInfo(planetData, planetInfo!=null ? planetInfo.Distance :.0f), 
                //new PlanetManagerCardComp.PresentInfo(),
                BuildManagerSectionCompPresentInfo(planetData==null ? -1 : planetData.Id),
                SelectedManagerInfoId != string.Empty,
                _usingSlot: Model.PlayerData.GetManagerSlotCount().Item1, _maxSlot: Model.PlayerData.GetManagerSlotCount().Item2,
                _canPromote: selectedMgr != null && selectedMgr.Level < ManagerInfo.MAX_LEVEL,
                _canHire: Model.PlayerData.ManagerSlots.Count > 0,
                listItem3Presentor);

            ManagerPnlView.Refresh(presentor);
            */
        }

        // Manager Card Dialog ----------------------------------------
        //
        //
        public void DisplayManagerCard(string ownedMngId, System.Action onCloseCallback)
        {
            /*
            OwnedManagerInfo focusedManager = Model.PlayerData.GetOwnedManagerInfo(ownedMngId);
            if (focusedManager == null)
                return;

            var mngInfo = Model.GetManagerInfo(focusedManager.ManagerId);
            Assert.IsNotNull(mngInfo);


            Sprite spriteManager = controller.View.GetManagerSprite(mngInfo.SpriteKey);
            int planetId = Model.PlayerData.GetPlanetIdForManager(focusedManager.Id);
            PlanetData planetData = Model.GetPlanetData(planetId);
            Sprite planetSprite = planetData == null ? null : controller.View.GetPlanetSprite(planetData.Id);

            int levelidx = focusedManager.Level - 1;
            ManagerItemComp.PresentInfo item = new ManagerItemComp.PresentInfo(focusedManager.Id, mngInfo.Name_, focusedManager.Level,
                mngInfo.BuffMiningRate_[levelidx], mngInfo.BuffShipSpeedRate_[levelidx], mngInfo.BuffCargoRate_[levelidx],
                spriteManager, planetSprite, false);
            ManagerCardPopupDialog.PresentInfo presentInfo = new ManagerCardPopupDialog.PresentInfo(item);
            controller.Context.PopUpScreen.DisplayPopupDialog(ManagerCardPopupDialog.sID, presentInfo, (dlg) =>
            {
                onCloseCallback?.Invoke();
            });
            */
        }







        void InitSystem()
        {
            Events.RegisterEvent(ManagerPanelView.EVENT_ONENABLED, ManagerPanelView_OnEnabled);

            ManagerPanelView.EventOnBtnBrowseToLeft += ManagerPanelView_OnPlanetBrowseToLeft;
            ManagerPanelView.EventOnBtnBrowseToRight += ManagerPanelView_OnPlanetBrowseToRight;
            ManagerPanelView.EventOnBtnHireClicked += ManagerPanelView_OnBtnHireClicked;
            ManagerPanelView.EventOnBtnPromoteClicked += ManagerPanelView_OnBtnPromoteClicked;
            ManagerPanelView.EventOnBtnDiscardClicked += ManagerPanelView_OnBtnDiscardClicked;
            ManagerPanelView.EventOnBtnRecruitClicked += ManagerPanelView_OnBtnRecruitClicked;
            ManagerPanelView.EventOnBtnAddSlotClicked += ManagerPanelView_OnBtnAddSlotClicked;
            ManagerPanelView.EventOnBtnPlanetManagerClicked += ManagerPanelView_OnBtnPlanetManagerClicked;
            ManagerPanelView.EventOnBtnManagerImageClicked += ManagerPanelView_OnBtnManagerImageClicked;

            ManagerItemComp.EventOnBtnCardClicked += ManagerItemComp_OnBtnCardClicked;
            PromoteManagerPopupDialog.EventOnPromoteBtnClicked += PromoteManagerPopupDialog_OnBtnPromoteClicked;
        }

        

        // Panel View Management ----------------------------------------
        //
        ManagerItemComp.PresentInfo BuildManagerItemCompPresentInfo(string managerInfoId, bool selected)
        {
            /*
            OwnedManagerInfo ownedManagerInfo = Model.PlayerData.GetOwnedManagerInfo(managerInfoId);
            if (ownedManagerInfo == null)
                return new ManagerItemComp.PresentInfo();

            var managerInfo = Model.GetManagerInfo(ownedManagerInfo.ManagerId);
            if (managerInfo == null)
                return new ManagerItemComp.PresentInfo();
            else
            {
                Sprite spriteManager = controller.View.GetManagerSprite(managerInfo.SpriteKey);
                int planetId = Model.PlayerData.GetPlanetIdForManager(ownedManagerInfo.Id);
                PlanetData planetData = Model.GetPlanetData(planetId);
                Sprite planetSprite = planetData == null ? null : controller.View.GetPlanetSprite(planetData.Id);

                int idxLv = ownedManagerInfo.Level - 1;
                return new ManagerItemComp.PresentInfo(ownedManagerInfo.Id, managerInfo.Name_, ownedManagerInfo.Level,
                                managerInfo.BuffMiningRate_[idxLv], managerInfo.BuffShipSpeedRate_[idxLv], managerInfo.BuffCargoRate_[idxLv],
                                spriteManager, planetSprite, selected);
                                // MNG.SelectedManagerInfoId==ownedManagerInfo.Id);
            }
            */
            return null;
        }

       //// PlanetManagerCardComp.PresentInfo BuildManagerSectionCompPresentInfo(int planetId)
       // {
            /*
            OwnedManagerInfo ownedMngInfo = Model.PlayerData.GetAssignedManagerInfoForPlanet(planetId);
            if(ownedMngInfo == null)
                return new PlanetManagerCardComp.PresentInfo();

            ManagerInfo mngInfo = Model.GetManagerInfo(ownedMngInfo.ManagerId);
            int levelidx = ownedMngInfo.Level - 1;
            
            return new PlanetManagerCardComp.PresentInfo(
                _icon : controller.View.GetManagerSprite(mngInfo.SpriteKey), 
                mngInfo.Name_, 
                $"MR:x{mngInfo.BuffMiningRate_[levelidx]} \nDS:x{mngInfo.BuffShipSpeedRate_[levelidx]} \nPS:x{mngInfo.BuffCargoRate_[levelidx]} ", 
                $"{ownedMngInfo.Level}/{ManagerInfo.MAX_LEVEL}");
            */
         //   return null;
      //  }

        // Promotion Dialog ----------------------------------------
        //
        //
        PromoteManagerPopupDialog.PresentInfo BuildPromoteDialogPresentInfo(string selectedOwnedManagerId)
        {
            if (string.IsNullOrEmpty(selectedOwnedManagerId))
                return null;

           /* OwnedManagerInfo mainChr = Model.PlayerData.GetOwnedManagerInfo(selectedOwnedManagerId);
            Assert.IsNotNull(mainChr);

            List<OwnedManagerInfo> listMng = new List<OwnedManagerInfo>();
            for (int q = 0; q < Model.PlayerData.OwnedManagers.Count; ++q)
            {
                if (Model.PlayerData.OwnedManagers[q].Level == mainChr.Level && Model.PlayerData.OwnedManagers[q].Id != selectedOwnedManagerId)
                    listMng.Add(Model.PlayerData.OwnedManagers[q]);
            }
           */
            bool IsSelected(string ownedId)
            {
                for (int q = 0; q < SelecteManagerInfoIdList.Count; ++q)
                {
                    if (SelecteManagerInfoIdList[q] == ownedId)
                        return true;
                }
                return false;
            }

            var listItem3Presentor = new List<ManagerList3ItemComp.PresentInfo>();
           /* for (int q = 0; q < listMng.Count; q += 3)
            {
                List<ManagerItemComp.PresentInfo> list3PresentsInfo = new List<ManagerItemComp.PresentInfo>();

                int idx = q;
                ManagerItemComp.PresentInfo presentInfo0 = null;
                if (idx < listMng.Count)
                    presentInfo0 = BuildManagerItemCompPresentInfo(listMng[idx].Id, IsSelected(listMng[idx].Id));
                list3PresentsInfo.Add(presentInfo0);

                ++idx;
                ManagerItemComp.PresentInfo presentInfo1 = null;
                if (idx < listMng.Count)
                    presentInfo1 = BuildManagerItemCompPresentInfo(listMng[idx].Id, IsSelected(listMng[idx].Id));
                list3PresentsInfo.Add(presentInfo1);

                ++idx;
                ManagerItemComp.PresentInfo presentInfo2 = null;
                if (idx < listMng.Count)
                    presentInfo2 = BuildManagerItemCompPresentInfo(listMng[idx].Id, IsSelected(listMng[idx].Id));
                list3PresentsInfo.Add(presentInfo2);

                listItem3Presentor.Add(new ManagerList3ItemComp.PresentInfo(list3PresentsInfo));
            }
           */
            // Main Manager.
            ManagerItemComp.PresentInfo mainMngCard = BuildManagerItemCompPresentInfo(selectedOwnedManagerId, selected: false);

            int count = 0;// Model.GetRequiredOtherCardToPromote(mainChr.Level);
            string msg = $"Discard other {count} of {mainMngCard.Level} star managers to promote.";
            return new PromoteManagerPopupDialog.PresentInfo(msg, SelecteManagerInfoIdList.Count >= count, mainMngCard, listItem3Presentor);
        }

       // PlanetSectorComp.PresentInfo  BuildPlanetSectionInfo(PlanetData planetData, float dist)
       // {
            //float fSIBuff = Model.GetSkillBuffValue(eABILITY.SHOT_INTERVAL);
            //float fSABuff = Model.GetSkillBuffValue(eABILITY.SHOT_ACCURACY);
            //float fDSBuff = Model.GetSkillBuffValue(eABILITY.DELIVERY_SPEED);
            //float fCSBuff = Model.GetSkillBuffValue(eABILITY.CARGO_SIZE);

            //string SIBuff = fSIBuff != 1.0f ? "x " + fSIBuff.ToString("0.00") : string.Empty;
            //string SABuff = fSABuff != 1.0f ? "x " + fSABuff.ToString("0.00") : string.Empty;
            //string DSBuff = fDSBuff != 1.0f ? "x " + fDSBuff.ToString("0.00") : string.Empty;
            //string CSBuff = fCSBuff != 1.0f ? "x " + fCSBuff.ToString("0.00") : string.Empty;

         //   if(planetData == null) 
         //      return new PlanetSectorComp.PresentInfo();

         //   return new PlanetSectorComp.PresentInfo(
        //        controller.View.GetPlanetSprite(planetData.Id),
         //       $"[{planetData.Id}]-{planetData.Name}",
                //$"{Model.GetPlanetStat(planetData.Id, eABILITY.SHOT_INTERVAL)}/sec " + SIBuff,
                //$"{Model.GetPlanetStat(planetData.Id, eABILITY.SHOT_ACCURACY)} % " + SABuff,
                //$"{Model.GetPlanetStat(planetData.Id, eABILITY.DELIVERY_SPEED)} km/s " + DSBuff,
                //$"{Model.GetPlanetStat(planetData.Id, eABILITY.CARGO_SIZE)} ea" + CSBuff,
          //      "", "", "", "",
         //       $"DIST : \n{dist.ToString("0.00")}km AWAY", "");
       // }

    }
}