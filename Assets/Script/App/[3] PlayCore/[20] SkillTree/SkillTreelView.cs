using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameCore.UI;
using UnityEngine.Assertions;
using Core.Events;
using UnityEngine.Events;
using Core.Utils;
using System;
using App.GamePlay.IdleMiner.Resouces;
using App.GamePlay.IdleMiner.Common;
using IGCore.Components;

namespace App.GamePlay.IdleMiner.SkillTree
{
    public class SkillTreeView : IGCore.MVCS.AView// ARefreshable
    {
        // Const Definitions.
        public const string EVENT_ONENABLED = "SkillTreeDialog_OnEnabled";
        public const string EVENT_ONDISABLED = "SkillTreeDialog_OnDisabled";

        public Action<string> EventOnSkillItemClicked;

        // Serialize Fields.
        [SerializeField] TabButtons MainMenuTabs;
        [SerializeField] SkillItemBundleComp skillItemBundle;
        
        [ImplementsInterface(typeof(INotificator))] 
        [SerializeField] MonoBehaviour notificator;


        [Header("Debugging Section")]
        [SerializeField] bool isCheatMode = false;
        public bool IsCheatMode => isCheatMode;


        // Accessor.
        public INotificator Notificator => notificator as INotificator;



        // Members.
        bool IsStarted = false;

        public class PresentInfo : APresentor
        {
            public PresentInfo(SkillItemBundleComp.PresentInfo skill_item_bundle_presentor)
            {
                this.bundlePresentInfo =  skill_item_bundle_presentor;
            }
            public SkillItemBundleComp.PresentInfo bundlePresentInfo { get; private set; }    
        }


        // Start is called before the first frame update
        private void Awake()
        {
            Assert.IsNotNull(skillItemBundle);
         //   Assert.IsNotNull(notificator);
        }
        void Start() { }

        protected override void OnEnable()
        {
            if (!IsStarted) Start();

            base.OnEnable();
            SkillItemComp.EventOnIconClicked += OnSkillItemClicked;

       //     SkillTabBtns.SelectedIndex = 0;
        }

        protected override void OnDisable()
        {
            SkillItemComp.EventOnIconClicked -= OnSkillItemClicked;

         //   base.OnDisable();
        }

        private void OnDestroy()
        {
            SkillItemComp.EventOnIconClicked -= OnSkillItemClicked;
        }

        public override void Refresh(APresentor presentor)
        {
            if (presentor == null)
                return;

            var info = (PresentInfo)presentor;
            if (info == null)
                return;

            skillItemBundle.Refresh(info.bundlePresentInfo);
        }

        public Sprite GetSkillIcon(string category_id, string skill_id)
        {
            return skillItemBundle?.GetSkillIcon(category_id, skill_id);
        }

        // Events Handler.
        //
        void OnSkillItemClicked(string skill_id)
        {
            EventOnSkillItemClicked?.Invoke(skill_id);
        }
        
        public void OnBtnCloseClicked()
        {
            MainMenuTabs.CloseAll();
            gameObject.SetActive(false);
        }

    }
}
