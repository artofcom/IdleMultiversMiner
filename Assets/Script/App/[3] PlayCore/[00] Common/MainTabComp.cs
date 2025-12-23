using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    [Serializable]
    public class FeatureBtnInfo
    {
        [SerializeField] string strName;
        [SerializeField] Button btnOn;
        [SerializeField] Button btnOff;
        [SerializeField] Image imageIcon;

        public string Name => strName;
        public Button OnButton => btnOn;
        public Button OffButton => btnOff;
        public Image ImageIcon => imageIcon;
    }


    // planet, resource, craft, skillTree, manager, booster.
    //
    public class MainTabComp : IGCore.MVCS.AView
    {
        [SerializeField] List<FeatureBtnInfo> featureButtons;


        public class PresentInfo : APresentor
        {
            public PresentInfo(List<string> listActivatedFeatureBtnNames)
            {
                ListActivatedBtnNames = listActivatedFeatureBtnNames;
            }   

            public List<string> ListActivatedBtnNames { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(featureButtons);
        }

        public override void Refresh(APresentor presentor)
        {
            PresentInfo info = presentor as PresentInfo;
            if(info == null)     return;

            for(int q = 0; q < featureButtons.Count; q++)
            {
                FeatureBtnInfo btnInfo = featureButtons[q];

                // search key.
                bool found = false;
                for(int i = 0; i < info.ListActivatedBtnNames.Count; i++)
                {
                    if(btnInfo.Name == info.ListActivatedBtnNames[i]) { found = true; break; }
                }

                btnInfo.OnButton.gameObject.SetActive(true);
                btnInfo.OffButton.gameObject.SetActive(false);
                btnInfo.OnButton.interactable = found;
                
                if(btnInfo.ImageIcon != null)
                    btnInfo.ImageIcon.color = found ? Color.white : Color.gray; 

                Debug.Log($"[MainTab]: [{btnInfo.Name}] has been set to [{found}].");
            }
        }


        //  Events ----------------------------------------
        //
    }
}