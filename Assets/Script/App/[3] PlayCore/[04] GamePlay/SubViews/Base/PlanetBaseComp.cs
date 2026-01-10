using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;
using App.GamePlay.IdleMiner.Common.Model;

namespace App.GamePlay.IdleMiner.GamePlay
{
    public class PlanetBaseComp : IGCore.MVCS.AView// MonoBehaviour// ARefreshable
    {
        public Action<int, int> EventOnClicked;


        [SerializeField] protected Image imgMain;
        [SerializeField] protected Button btnMain;
        [SerializeField] protected GameObject openedRoot;
        [SerializeField] protected GameObject closedRoot;
        [SerializeField] protected TMP_Text planetName;
        [SerializeField] protected GameObject managerRoot;
        [SerializeField] protected Image imgManager;

        [Header("=== Data Link===")]
        [SerializeField] protected int planetId;

        public virtual string Type => "Base";

        protected bool mIsInitialized = false;

        // Accessor.
        public int ZoneId => transform.parent.GetComponent<MiningZoneComp>().ZoneId;
        public int PlanetId => planetId;
        public bool IsInitialized => mIsInitialized;

        public class BasePresentInfo : APresentor
        {
            public string Name { get; private set; }
        }

        protected virtual void Awake()
        {
            Assert.IsNotNull(imgMain);
            Assert.IsNotNull(btnMain);
            Assert.IsNotNull(openedRoot);
            Assert.IsNotNull(closedRoot);
            Assert.IsNotNull(planetName);
            Assert.IsNotNull(managerRoot);
            Assert.IsNotNull(imgManager);
        }

        protected virtual void Start() 
        {
            btnMain.onClick.AddListener(OnBtnClicked);
        }

        public virtual void Init()
        {
            mIsInitialized = true;
        }

        public override void Refresh(APresentor presentor)
        {
            var presentInfo = (BasePresentInfo)presentor;

            planetName.text = presentInfo.Name;
        }

        // Util.
        public Sprite GetIcon()
        {
            var icon = imgMain;
            Assert.IsNotNull(icon);
            return icon.sprite;
        }

        public void OnBtnClicked()
        {
            // Debug.Log("Btn Clicked ! " + base.Id);
            EventOnClicked?.Invoke(ZoneId, planetId);
        }
    }
}
