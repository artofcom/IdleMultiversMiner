using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Numerics;
using UnityEngine.Assertions;
using System;
using Core.Utils;

namespace App.GamePlay.IdleMiner.GamePlay
{
    public class BoosterComp :IGCore.MVCS.AView
    {
        public Action EventOnBoosterClicked;


        [SerializeField] GameObject runButton;
        [SerializeField] GameObject runningProgressRoot;
        [SerializeField] Image runningProgressImage;

        [SerializeField] Image cooltimeProgressImage;

        [SerializeField] TMP_Text txtStatus;

        public class PresentInfo : APresentor
        {
            // Ready
            public PresentInfo()
            {
                IsReady = true;
            }

            // Boosting
            // isBoosting ( false : coolTime )
            public PresentInfo(float remainSec, float fullSec, bool isBoosting)
            {
                IsReady = false;
                RemainSec = remainSec;
                FullDurationSec = fullSec;
                IsBoosting = isBoosting;
            }

            public bool IsReady { get;  private set; }
            public float RemainSec { get; private set; }
            public float FullDurationSec { get; private set; }
            public bool IsBoosting { get; private set; }    // boosting or cooling ?
        }

        //===================================================================//
        //
        // Initialization
        //
        protected void Awake()
        {
            //base.Awake();
            //Assert.IsNotNull(OpenPrice);            
        }

        
        //===================================================================//
        //
        // UI Interfactions / Updates.
        //
        public override void Refresh(APresentor presentor)
        {
            
            if (presentor == null)
                return;

            var info = (PresentInfo)presentor;
            bool isCoolTime = !info.IsReady && !info.IsBoosting;

            runButton.SetActive(info.IsReady || isCoolTime);
            if(runButton.activeSelf)
                runButton.GetComponent<Button>().interactable = !isCoolTime;

            runningProgressRoot.SetActive(false);
            txtStatus.gameObject.SetActive(!info.IsReady);
            cooltimeProgressImage.gameObject.SetActive(!info.IsReady);

            if(false == info.IsReady)   // cooltime or boosting.
            {
                runningProgressRoot.SetActive(info.IsBoosting);
                cooltimeProgressImage.gameObject.SetActive(isCoolTime);

                if(info.IsBoosting)
                    runningProgressImage.fillAmount = info.RemainSec / info.FullDurationSec;
                
                else
                    cooltimeProgressImage.fillAmount = 1.0f - info.RemainSec / info.FullDurationSec;;
                
                txtStatus.text = TimeExt.ToTimeString((long)info.RemainSec, TimeExt.UnitOption.NO_USE, TimeExt.TimeOption.MIN);
            }
        }

        public void OnManualBuffClicked()
        {
            EventOnBoosterClicked?.Invoke();
        }
    }
}