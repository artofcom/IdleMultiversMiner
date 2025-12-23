using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner.MiningStat
{
    public class PlanetManagerCardComp : IGCore.MVCS.AView
    {
        [SerializeField] GameObject OnRoot;
        [SerializeField] GameObject OffRoot;

        [SerializeField] Image imageManager;
        [SerializeField] TMP_Text textName;
        [SerializeField] TMP_Text textGrade;
        [SerializeField] TMP_Text textStat;

        //[Header("=== Data Link===")]

        // public int TownId { get; private set; } = -1;

        public class PresentInfo : APresentor
        {
            public PresentInfo() { IsAssigned = false; }
            public PresentInfo(Sprite _icon, string _name, string _stat, string _grade)
            {
                IsAssigned = true; Icon = _icon; Name = _name; Stats = _stat; Grade = _grade;
            }
            public bool IsAssigned { get; private set; }
            public Sprite Icon { get; private set; }
            public string Name { get; private set; }
            public string Stats { get; private set; }
            public string Grade { get; private set; }
        }

        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(OnRoot);
            Assert.IsNotNull(OffRoot);

            Assert.IsNotNull(imageManager);
            Assert.IsNotNull(textName);
            Assert.IsNotNull(textGrade);
            Assert.IsNotNull(textStat);

            // GetComponent<Button>().onClick.AddListener( OnBtnClicked );
        }

        


        public void Init()
        {
            //const string KEY = "TOWN";
            //base.Init($"{KEY}-{TownId}");
        }

        public void OnBtnClicked()
        {
            //Debug.Log("Btn Clicked ! " + base.Id);
            //EventSystem.DispatchEvent("TownComponent_OnBtnClicked", this);
        }

        public override void Refresh(APresentor presentor)
        {
            //UnityEngine.Assertions.Assert.IsTrue(TownId > 0);
            
            if (presentor == null)
                return;

            var info = (PresentInfo)presentor;

            OnRoot.SetActive(info.IsAssigned);
            OffRoot.SetActive(!info.IsAssigned);

            if(info.IsAssigned)
            {
                imageManager.sprite = info.Icon;
                textName.text = info.Name;
                textGrade.text = info.Grade;
                textStat.text = info.Stats;
            }
        }
    }
}