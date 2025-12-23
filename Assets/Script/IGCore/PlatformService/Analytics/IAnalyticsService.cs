using System.Collections.Generic;
using UnityEngine;

namespace IGCore.SubSystem.Analytics
{
    public interface IAnalyticsService 
    {
        void Init();
        void SendEvent(string eventName);
        void SendEvent<T>(string eventName, Dictionary<string, T> dictData);
    }
}
