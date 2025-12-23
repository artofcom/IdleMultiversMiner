using Core.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using static IGCore.MVCS.Requestable;

namespace IGCore.MVCS
{
    public abstract class AContext
    {
        protected Dictionary<string, object> dataBuffer = new Dictionary<string, object>();
        Requestable Requestable = new Requestable();
        EventsGroup Events = new EventsGroup();


        public EventsGroup EventGroup => Events;

        abstract public void InitGame();
        abstract public void DisposeGame();

        public void AddData(string key, object data)
        {
            if(!dataBuffer.ContainsKey(key))
                dataBuffer[key] = data;
            else 
                UpdateData(key, data);
        }
        public void UpdateData(string key, object data)
        {
            DeleteData(key);
            AddData(key, data);
        }
        public object GetData(string key, object defaultValue=null)
        {
            if(dataBuffer.ContainsKey(key))
                return dataBuffer[key];

            Debug.LogWarning($"GetData : Couldn't find the key...[{key}]");
            return defaultValue!=null ? defaultValue : null;
        }
        public void DeleteData(string key)
        {
            if(dataBuffer.ContainsKey(key))
                dataBuffer.Remove(key);
        }
        public void ClearData()
        {
            dataBuffer.Clear();
        }

        public bool IsSimulationMode()
        {
#if UNITY_EDITOR
            return (bool)GetData("IsSimMode", false);
#else
            return false;
#endif
        }

        #region // Request Query Related. 
        public void RequestQuery(string unitName, string endPoint, Action<string, object> finishCallback, params object[] param)
        {
            Requestable.RequestQuery($"{unitName}/{endPoint}", finishCallback, param);
        }
        public object RequestQuery(string unitName, string endPoint, params object[] param)
        {
            object result = null;
            Requestable.RequestQuery($"{unitName}/{endPoint}", (errMsg, ret) =>
            {
                if(false == string.IsNullOrEmpty(errMsg))
                    Debug.LogWarning($"GetRequestQuery[{unitName}]/[{endPoint}] has some errors...{errMsg}");

                result = ret;
            }, param);

            return result;
        }
        public void AddRequestDelegate(string unitName, string endPoint, RequestDelegate queryDelegate)
        {
            Requestable.AddRequestDelegate($"{unitName}/{endPoint}", queryDelegate);
        }
        public void RemoveRequestDelegate(string unitName, string endPoint) 
        {
            Requestable.RemoveRequestDelegate($"{unitName}/{endPoint}");
        }
        #endregion
    }
}
