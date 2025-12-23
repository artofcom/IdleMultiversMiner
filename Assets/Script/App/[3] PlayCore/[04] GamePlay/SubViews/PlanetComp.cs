using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Core.Events;
using System.Numerics;
using UnityEngine.Assertions;
using UnityEngine.Events;
using System;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner.GamePlay
{
    public class PlanetComp : PlanetBaseComp
    {
        public Action<int, int> EventOnArrivalAtTown;
        public Action<int, int> EventOnArrivalAtStation;
        public Action<int, int> EventOnBoosterClicked;

        [Header("Planet")]
        [SerializeField] TMP_Text OpenPrice;
        [SerializeField] BoosterComp boosterComp;

#if UNITY_EDITOR
        [Header("--- Editor Area ---")]
        [SerializeField] protected PlanetData planetData;
        public PlanetData PlanetData => planetData;
#endif


        public TwoSpotRunner Deliverer => mFlyMonObj;
        
        GameObject mMainCharObj;
        TwoSpotRunner mFlyMonObj;

        public override string Type => "Planet";

        public class PresentInfo : APresentor
        {
            // Closed.
            public PresentInfo(string _openCost)
            {
                IsOpened = false;
                OpenCost = _openCost;
            }

            // Opened.
            public PresentInfo(string _name, float _speed, Sprite _sprManager, BoosterComp.PresentInfo boostPresentInfo)
            {
                IsOpened = true;
                Name = _name;
                SpriteManager = _sprManager;
                DeliverySpeed = _speed;
                boostCompPresentInfo = boostPresentInfo;
            }


            public string Name { get; private set; }
            public bool IsOpened { get; private set; }
            public float DeliverySpeed { get; private set; }
            public string OpenCost { get; private set; }
            public Sprite SpriteManager { get; private set; }
            
            public BoosterComp.PresentInfo boostCompPresentInfo { get; private set; }
        }

        //===================================================================//
        //
        // Initialization
        //
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(OpenPrice); 
            Assert.IsNotNull(boosterComp);

            boosterComp.EventOnBoosterClicked += OnBoosterClicked;
        }

        public void Init(bool isOpened, float deliverySpeed, GameObject mainCharObj, GameObject flyMonObjCache)
        {
            Assert.IsNotNull(mainCharObj);
            Assert.IsNotNull(flyMonObjCache);

            //const string KEY = "GRAVE";
            //base.Register($"{KEY}-{planetId}");

            mMainCharObj = mainCharObj;
            var obj = Instantiate(flyMonObjCache, flyMonObjCache.transform.parent);
            mFlyMonObj = obj.GetComponent<TwoSpotRunner>();
            Assert.IsNotNull(mFlyMonObj);

            //
            // MainDemon ------------------------- Town
            //    |      <------ fly demon ------>  |
            //
            mFlyMonObj.Init(mMainCharObj.transform, transform);
            mFlyMonObj.OnArrivalAtTarget1.AddListener(OnArrivalAtMainChar);
            mFlyMonObj.OnArrivalAtTarget2.AddListener(OnArrivalAtTown);
            mFlyMonObj.gameObject.SetActive(isOpened);
            if(isOpened)
                mFlyMonObj.SetVelocity(deliverySpeed);
            
            base.Init();
        }

        public override void CleanUp()
        {
            Debug.Log($"<color=green>[Planet-CleanUp] Id:[{this.planetId}] clean up in progress..</color>");

            mMainCharObj = null;

            mFlyMonObj.Stop();
            mFlyMonObj.OnArrivalAtTarget1.RemoveAllListeners();
            mFlyMonObj.OnArrivalAtTarget2.RemoveAllListeners();
            Destroy(mFlyMonObj.gameObject);
            mFlyMonObj = null;
            openedRoot.gameObject.SetActive(false);
            closedRoot.gameObject.SetActive(true);

            base.CleanUp();
        }

        //===================================================================//
        //
        // UI Interfactions / Updates.
        //
        public override void Refresh(APresentor presentor)
        {
            Assert.IsTrue(planetId > 0);
            // Assert.IsTrue(!string.IsNullOrEmpty(base.Id));

            if (presentor == null)
                return;

            var info = (PresentInfo)presentor;

            bool IsOpenedBefore = openedRoot.gameObject.activeSelf;

            openedRoot.gameObject.SetActive(info.IsOpened);
            closedRoot.gameObject.SetActive(!info.IsOpened);

            if (info.IsOpened)
            {
                planetName.text = info.Name;
                imgMain.color = Color.white;

                if(mFlyMonObj != null)
                {
                    // Is this Opened on this update?
                    if (!IsOpenedBefore)
                        mFlyMonObj.gameObject.SetActive(true);
                
                    mFlyMonObj.SetVelocity(info.DeliverySpeed);
                }

                managerRoot.SetActive(info.SpriteManager != null);
                imgManager.sprite = info.SpriteManager;

                boosterComp.gameObject.SetActive(info.boostCompPresentInfo!=null);
                if(info.boostCompPresentInfo != null)
                    boosterComp.Refresh(info.boostCompPresentInfo);
            }
            else
            {
                boosterComp.gameObject.SetActive(false);
                OpenPrice.text = // "<sprite name=\"Rune\">" +
                                 "$" + info.OpenCost;
            }
        }

        void OnBoosterClicked()
        {
            EventOnBoosterClicked?.Invoke(ZoneId, planetId);
        }

        


        //===================================================================//
        //
        // Mining Core Logic.
        //
        void OnArrivalAtTown()
        {
            EventOnArrivalAtTown?.Invoke(ZoneId, planetId);
        }

        void OnArrivalAtMainChar()
        {
            EventOnArrivalAtStation?.Invoke(ZoneId, planetId);
        }
    }
}