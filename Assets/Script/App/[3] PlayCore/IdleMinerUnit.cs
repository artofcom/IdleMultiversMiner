using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using App.MetaSystem.Bonus;
using Core.Events;
using IGCore.Components;
using IGCore.MVCS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    public class IdleMinerUnit : AUnit
    {
#if UNITY_EDITOR
        const int PLAYER_DATA_WRITE_INTERVAL = 5;   // in sec.
#else
        const int PLAYER_DATA_WRITE_INTERVAL = 30;  // in sec.
#endif

        [SerializeField] List<AUnit> subUnits;
        [SerializeField] AUnit startUnit;
        [SerializeField] GameConfig gameConfig;
        [SerializeField] AUnit popupDialog;

        [SerializeField] TimedBonus timedBonus;
        
#if UNITY_EDITOR
        [Header("Simulator Settins")]
        [SerializeField] string simulationPath = "Bundles/G010_Graves/Variations/Standard";
        [SerializeField] int simFrameUpdateSec = 30;
        [SerializeField] int simCount = 10;
        [SerializeField] float simFrameSec = 1.0f;
        [SerializeField] bool shouldSellItem;
        [SerializeField] float desiredPDR = 0.1f;   // sell until production rate. 
        [SerializeField] int craftSlotCount = 3;

        public string SimulationPath => simulationPath;
        public int SimulatorFrameUpdateSec => simFrameUpdateSec;
        public int SimulatorCount => simCount;
        public float SimulatorFrameSec => simFrameSec;
        public bool ShouldSellItem => shouldSellItem;
        public float DesiredPDR => desiredPDR;
        public int CraftSlotCount => craftSlotCount;    
#endif

        EventsGroup Events = new EventsGroup();
        int curTabIndex = -1;
        IdleMinerModel model;

        AUnit StartUnit => startUnit==null ? ((subUnits!=null && subUnits.Count>0) ? subUnits[0] : null) : startUnit; 
        

        // [ImplementsInterface(typeof(IDamageable))]
        // [SerializeField] MonoBehaviour targetMono; 

        protected override void Awake()
        {
            base.Awake();
            Assert.IsTrue(subUnits!=null && subUnits.Count>0);
            Assert.IsNotNull(timedBonus);

            timedBonus.awardingIntervalInMin = gameConfig.TimedBonusIntervalInMin;
        }

        public override void Init(AContext ctx)
        {
            base.Init(ctx);

            context.AddData("GameConfig", gameConfig);
            if(context.GetData("IsSimMode") == null)
                context.AddData("IsSimMode", false);

            context.InitGame();
            foreach(var module in subUnits)
                module.Init(ctx);
            
            var playerModel = new IdleMinerPlayerModel(context, (ctx as IdleMinerContext).GameCoreGatewayService);
            model = new IdleMinerModel(context, playerModel);
            controller = new IdleMinerController(view, model, context);

            playerModel.Init();
            model.Init();
            controller.Init();  

            Events.RegisterEvent(EventID.SKILL_RESET_GAME_INIT, Event_ResetGamePlay_InitGame);
            Events.RegisterEvent(EventID.RESOURCE_UPDATED, Event_Resource_Updated);            

            if(!ctx.IsSimulationMode())
            {
                popupDialog.Init(ctx);
                ((IdleMinerView)view).EventOnTabBtnChanged += OnTabBtnChanged;

                context.AddData(KeySets.CTX_KEYS.GAME_DLG_KEY, ((PopupDialogUnit)popupDialog).DialogKey);
            }
        }

        public override void Resume(int awayTimeInSec)
        {
            base.Resume(awayTimeInSec);

            // GamePlay, MiningStat, Resource, Craft, SkillTree.. ::Resume()
            foreach(var module in subUnits)
                module.Resume(awayTimeInSec);
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach(var module in subUnits)
                module.Dispose();

            popupDialog.Dispose();

            model.Dispose();

            context.DisposeGame();

            Events.UnRegisterAll();
        }

        public override void Attach()
        {
            Assert.IsNotNull(context);

            if(startUnit == null)
                startUnit = subUnits[0];

            base.Attach();
            StartUnit.Attach();
            popupDialog.Attach();

            StartCoroutine(coUpdateMainPump());
            StartCoroutine(coUpdateWriteData());
        }


        #region ===> Helpers

        void Event_ResetGamePlay_InitGame(object data)
        {
            timedBonus.SetEnable(enable:false);
            Debug.Log("<color=green>TimeBons has been Disabled.!</color>");
        }

        void Event_Resource_Updated(object data)
        {
            if(!timedBonus.IsEnabled)
            {
                timedBonus.SetEnable(enable:true);
                Debug.Log("<color=green>TimeBons has been Actived.!</color>");
            }
        }

        IEnumerator coUpdateMainPump()
        {
            // Wait for init Up.: 3Sec is good enough for now.
            yield return new WaitForSeconds(3.0f);

            var waitASec = new WaitForSeconds(1.0f);

            int interval =  (int)model.PlayerData.FlushAwayTime();
            Debug.Log("[InitSeq]:[Resume-Unit] Away Idle Time in Sec " + interval.ToString());
            foreach(var module in subUnits)
                module.Resume(interval);

            while (true)
            {
                Pump();

                foreach(var module in subUnits)
                    module.Pump();

                yield return waitASec;
            }
        }

        IEnumerator coUpdateWriteData()
        {
            // Wait for init Up.
            yield return new WaitForSeconds(1.0f);

            var waitASec = new WaitForSeconds(PLAYER_DATA_WRITE_INTERVAL);
            
            while (true)
            {            
                WriteAllData();
                yield return waitASec;
            }
        }

        void WriteAllData()
        {
            WriteData();
            foreach(var module in subUnits)
                module.WriteData();
        }

        // For App on Device.
        private void OnApplicationFocus(bool focus)
        {
#if !UNITY_EDITOR
            if(!focus)
                WriteAllData();
#endif
        }




        // For Editor.
        private void OnApplicationPause(bool pause)
        {
#if UNITY_EDITOR
            if(pause)
                WriteAllData();
#endif
        }

        private void OnApplicationQuit()
        {
            WriteAllData();
        }

        void OnTabBtnChanged(int index)
        {
            Debug.Log("Tab Changed..."+index);
            
            // 1 ~ subUnits.Count : Do NOT detaching GamePlay.
            //
            if(curTabIndex>=1 && curTabIndex<subUnits.Count)
                subUnits[curTabIndex].Detach();

            curTabIndex = -1;
            ++index;
            if(index>=0 && index<subUnits.Count)
            {
                subUnits[index].Attach();
                curTabIndex = index;
            }
        }

#endregion
    }
}
