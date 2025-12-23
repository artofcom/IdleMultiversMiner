using System;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Analytics;
using UnityEngine.UnityConsent;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace IGCore.SubSystem.Analytics
{
    [CreateAssetMenu(fileName = "UnityAanlyticsService", menuName = "ScriptableObjects/UnityAnalyticsService", order = 1)]
    public class UnityAnalyticsService : ScriptableObject, IAnalyticsService
    {
        class GameEvent : Unity.Services.Analytics.Event
        {
            public GameEvent(string eventName) : base(eventName) { }

            public void SetEvent<T>(string key, T value)
            {
                switch(value)
                {
                case bool b:    SetParameter(key, b);   break;
                case string s:  SetParameter(key, s);   break;
                case int i:     SetParameter(key, i);   break;
                case double d:  SetParameter(key, d);   break;
                case float f:   SetParameter(key, f);   break;
                case long l:    SetParameter(key, l);   break;
                default:
                    Assert.IsTrue(false, "Unsupported Event Type !");
                    break;
                }
            }
        }

        private void OnEnable() {}

        public async void Init()
        {
            try
            {
                await UnityServices.InitializeAsync();

                EndUserConsent.SetConsentState(new ConsentState
                {
                    AdsIntent = ConsentStatus.Granted,
                    AnalyticsIntent = ConsentStatus.Granted,
                });
            }
            catch(Exception ex) 
            {
                // ignore and go.
                Debug.LogWarning("Unity Service Init has been failed. : " + ex.Message);
            }
        }


        // Note : eventName should be assigned and enabled from the Unity Dash.
        //
        public void SendEvent(string eventName)
        {
            AnalyticsService.Instance.RecordEvent(eventName);
            Debug.Log($"[GameEvent] : {eventName} has been sent!");
        }
        public void SendEvent<T>(string eventName, Dictionary<string, T> dictSubData)
        {
            var newEvent = new GameEvent(eventName);
            foreach(var dataName in dictSubData.Keys)
                newEvent.SetEvent<T>(dataName, dictSubData[dataName]);
            
            AnalyticsService.Instance.RecordEvent(newEvent);
        }
    }
}