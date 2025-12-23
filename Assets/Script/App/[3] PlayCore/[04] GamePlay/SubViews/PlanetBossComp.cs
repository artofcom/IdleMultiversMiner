using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Core.Events;
using System.Numerics;
using UnityEngine.Assertions;
using UnityEngine.Events;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner.GamePlay
{
    public class PlanetBossComp : PlanetBaseComp
    {
        [Header("BossBattle")]
        [SerializeField] TMP_Text txtOpenCost;
        [SerializeField] TMP_Text txtTime;
        [SerializeField] Slider SliderLife;
        [SerializeField] GameObject clearedRoot;


#if UNITY_EDITOR
        [Header("--- Editor Area ---")]
        [SerializeField] protected PlanetBossData planetBossData;
        public PlanetBossData PlanetBossData => planetBossData;
#endif

        public override string Type => App.GamePlay.IdleMiner.Common.Model.PlanetBossData.KEY;

        public class PresentInfo : APresentor
        {
            public enum STATE {  CLOSED, OPENED, CLEARED };

            // Closed.
            public PresentInfo(string _openCost)
            {
                State = STATE.CLOSED;
                OpenCost = _openCost;
            }

            // Opened.
            public PresentInfo(string _name, string time, float lifeRate, Sprite _sprManager)
            {
                State = STATE.OPENED;
                Name = _name;

                Time = time;
                LifeRate = lifeRate;
                SprManager = _sprManager;
            }

            // Cleared.
            public PresentInfo(string _name, bool dontCare)
            {
                State = STATE.CLEARED;
                Name = _name;
            }


            public string Name { get; private set; }
            public STATE State { get; private set; }
            public string OpenCost { get; private set; }

            public string Time { get; private set; }
            public float LifeRate { get; private set; }
            public Sprite SprManager { get; private set; }
        }

        //===================================================================//
        //
        // Initialization
        //
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(txtOpenCost);
            Assert.IsNotNull(txtTime);
            Assert.IsNotNull(SliderLife);
            Assert.IsNotNull(clearedRoot);
        }

        public void Init(bool isOpened)//, string txtTime, float lifeRate)
        {
            base.Init();
            //const string KEY = "BOSS";
            //base.Register($"{KEY}-{planetId}");
        }



        //===================================================================//
        //
        // UI Interfactions / Updates.
        //
        public override void Refresh(APresentor presentor)
        {
            Assert.IsTrue(planetId > 0);
            //Assert.IsTrue(!string.IsNullOrEmpty(base.Id));

            if (presentor == null)
                return;

            var info = (PresentInfo)presentor;

            bool IsOpenedBefore = openedRoot.gameObject.activeSelf;

            openedRoot.gameObject.SetActive(info.State == PresentInfo.STATE.OPENED);
            closedRoot.gameObject.SetActive(info.State == PresentInfo.STATE.CLOSED);
            clearedRoot.SetActive(info.State == PresentInfo.STATE.CLEARED);

            managerRoot.SetActive(info.SprManager != null);

            switch(info.State)
            {
                case PresentInfo.STATE.OPENED:
                    planetName.text = info.Name;
                    imgMain.color = Color.white;
                    txtTime.text = info.Time;
                    SliderLife.value = info.LifeRate;
                    imgManager.sprite = info.SprManager;
                    break;
                case PresentInfo.STATE.CLOSED:
                    txtOpenCost.text = info.OpenCost;
                    break;
                case PresentInfo.STATE.CLEARED:
                    planetName.text = info.Name;
                    imgMain.color = Color.gray;
                    break;
            }
        }


    }
}