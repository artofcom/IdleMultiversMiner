using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameCore.UI;
using Core.Utils;
using Core.Events;
using UnityEngine.Assertions;
using Core.Tween;
using System;
using UI.Scroller;
using IGCore.MVCS;
using Core.Util;

namespace App.GamePlay.IdleMiner.GamePlay
{
    public class GamePlayView : IGCore.MVCS.AView
    {
        //=============================================================================
        //
        #region ===> Instance Properties
        
        [SerializeField] MonoBehaviour CoroutineRunner;
     
        [SerializeField] GameObject MainCharObj;
        [SerializeField] PlanetControllerComp planetsComp;
        [SerializeField] GameObject FlyMonObjCache;

        [SerializeField] CurveTweener arrowFirer;
        [SerializeField] LineTweener magicBallFirer;
        [SerializeField] Transform transformPlayField;
        [SerializeField] float speedVisualAdjustmentRate = 1.0f;
        
        [SerializeField] GameObject btnGameReset;
     
        public PlanetControllerComp PlanetsComp => planetsComp;
        public float SpeedVisualAdjustmentRate => speedVisualAdjustmentRate;

        public Action EventOnBtnGameResetClicked;

        public class ViewIniter : AIniter
        {
            public ViewIniter(Dictionary<int, List<MiningZoneComp.BaseInfo>> dictAreaComp)
            {
                this.dictAreaComp = dictAreaComp;
            }
            public Dictionary<int, List<MiningZoneComp.BaseInfo>> dictAreaComp { get; protected set; }
        }

        #endregion ===> Instance Properties

        
        //=============================================================================
        //
        #region ===> Unity Callbacks

        // Start is called before the first frame update
        protected void Awake()
        {
            Assert.IsNotNull(CoroutineRunner);
            Assert.IsNotNull(MainCharObj);
            Assert.IsNotNull(planetsComp);
            Assert.IsNotNull(FlyMonObjCache);
            Assert.IsNotNull(arrowFirer);
            Assert.IsNotNull(magicBallFirer);
            Assert.IsNotNull(transformPlayField);
            Assert.IsNotNull(btnGameReset);

            btnGameReset.SetActive(false);
            
       //   StartMeteor();
        }

        #endregion ===> Unity Callbacks


        //=============================================================================
        //
        #region ===> Interfaces

        public override void Init(AIniter initer)
        {
            planetsComp.Init(CoroutineRunner, MainCharObj, FlyMonObjCache, (initer as ViewIniter).dictAreaComp);
        }

        public override void Refresh(APresentor presentData)
        {}

        internal void TriggerArcher(Vector3 vWorldTo, object data, Action<object> onFinish)
        {
            //StartCoroutine(TriggerActionWithDelay(0.1f, () =>
            //{
                arrowFirer.Trigger(vWorldTo, isWorldPos:true, duration:-1.0f, data, onFinish);
            //}));
        }
        internal void TriggerMagicBall(Vector3 vWorldTo, float velocity, object data, Action<object> onFinish)
        {
            float duration = (magicBallFirer.GetDistance(vWorldTo, isWorldPos:true)) / velocity;
            magicBallFirer.Trigger(vWorldTo, isWorldPos:true, duration, data, onFinish);
        }

        internal float GetDistanceToPlanet(int zoneId, int planetId)
        {
            Vector2 vPlanet = PlanetsComp.GetPlanetPos(zoneId, planetId);// Vector2.one;//  TownManager.GetPlanetPos(planetId);
            Vector2 vMainChar = MainCharObj.transform.position;

            return (vPlanet - vMainChar).magnitude;// * PlanetData.WORLD_DIST_ADJUSTMENTS;
        }

        public List<float> GetDistanceToPlanet(int zoneId)
        {
            List<float> distanceList = new List<float>();
            List<Vector2> vPoses = PlanetsComp.GetPlanetsPos(zoneId);
            Vector2 vMainChar = MainCharObj==null ? Vector2.zero : MainCharObj.transform.position;
            for(int q = 0; q < vPoses.Count; q++)  
            {
                distanceList.Add((vPoses[q] - vMainChar).magnitude);
            }
            return distanceList;
        }

        internal TwoSpotRunner GetDeliverer(int zoneId, int planetId)
        {
            return planetsComp.GetDeliverer(zoneId, planetId);
        }

        internal IEnumerator coTriggerActionWithDelay(float delay, Action onFinish)
        {
            yield return new WaitForSeconds(delay);

            onFinish?.Invoke();
        }

        public void EnableGameResetButton(bool enabled)
        {
            btnGameReset.gameObject.SetActive(enabled);
        }

        #endregion ===> Interfaces


        //=============================================================================
        //
        #region ===> Event Handlers

        public void OnClickZoomIn()
        {
            transformPlayField.localScale *= 1.1f;
        }

        public void OnClickZoomOut()
        {
            transformPlayField.localScale *= 0.95f;
        }

        public void OnBtnGameResetClicked()
        {
            EventOnBtnGameResetClicked?.Invoke();
        }

        #endregion Event Handlers
 


        public Sprite GetPlanetSprite(int zoneId, int planetId)
        {
            return planetsComp.GetPlanetSprite(zoneId, planetId);
        }
    }
}