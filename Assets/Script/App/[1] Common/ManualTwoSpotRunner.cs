using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    public class ManualTwoSpotRunner : TwoSpotRunner
    {
        [HideInInspector] public bool IsManualUpdateMode { get; set; } = false;


        override protected void Update()
        {
            if (!IsManualUpdateMode)
                base.Update();
        }
        
        public void ManualUpdate(float deltaTime)
        {
            if (IsManualUpdateMode)
            {
                _internalUpdate(deltaTime);
            }
        }
    }
}