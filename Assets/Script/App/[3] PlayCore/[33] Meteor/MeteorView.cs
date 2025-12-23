using UnityEngine;
using UnityEngine.Assertions;
using System;

namespace App.GamePlay.IdleMiner
{
    public class MeteorView : IGCore.MVCS.AView
    {
        public static Action EventOnBtnMeteorClicked;

        [SerializeField] GameObject meteorObject;
        
        public GameObject MeteorObject => meteorObject;

        public override void Refresh(APresentor presentData)
        {}

        void StartMeteor()
        {
            Assert.IsNotNull(meteorObject);
        }




        public void ShowMeteor(bool show)
        {
            meteorObject.SetActive(show);
        }


        public void OnMeteorObjectClicked()
        {
            EventOnBtnMeteorClicked?.Invoke();
        }
    }
}