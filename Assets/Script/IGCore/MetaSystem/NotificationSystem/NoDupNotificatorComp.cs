using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

namespace IGCore.Components
{
    // Powered by Gemini & artofcom.
    public class NoDupNotificatorComp : MonoBehaviour, INotificator
    {
        private const string NOTI_KEYS = "NOTIFICATION_KEYS";

        enum STATUS { ON, OFF };

        // [SerializeField] string id;
        [SerializeField] GameObject objectNotifier;
        
        // ReasonKey, status.
        Dictionary<string, STATUS> dictReasonStatus = new Dictionary<string, STATUS>();

        // static HashSet<string> sIdListCache = new HashSet<string>();    // All Id List.

        private void Awake()
        {
            // Assert.IsTrue(!string.IsNullOrEmpty(id), "Id should not be empty!!!");

            if(objectNotifier != null) 
                objectNotifier.SetActive(false);

            // LoadCacheFromPlayerPrefs();
        }

        // returns if the new reason is valid.
        public bool EnableNotification(string reason_key)
        {
            bool ret = SetStatus(STATUS.ON, reason_key);

            if(ret && objectNotifier != null) 
                objectNotifier.SetActive(true);

            return ret;
        }

        // When player opens the UI - notifier needs to be cleared.
        public void DisableNotification()
        {
            if(objectNotifier != null) 
                objectNotifier.SetActive(false);

            SetStatus(STATUS.OFF);
        }






        bool SetStatus(STATUS eStatus, string reason_key="") 
        {
            switch(eStatus) 
            {
            case STATUS.ON:
                Assert.IsTrue(!string.IsNullOrEmpty(reason_key));

                if (dictReasonStatus.ContainsKey(reason_key))    
                    return false;
                 
                dictReasonStatus.Add(reason_key, eStatus);
                
                Debug.Log($"[Noti] : Turnning ON. [{reason_key}]");
                break;

            case STATUS.OFF:
                {
                    if(dictReasonStatus.Count > 0)
                    {
                        foreach(string key in dictReasonStatus.Keys.ToList()) 
                            dictReasonStatus[key] = eStatus;
                    }
                    Debug.Log($"[Noti] : Turnning OFF.. [{reason_key}]");
                    break;
                }
            default:
                // Un recognized type.
                return false;
            }

            
            // Re-Write all Reason Data.
            /*string strData = string.Empty;
            int cnt = 0;
            foreach(var key in dictReasonStatus.Keys)
            {
                // ReasonKey0=>status:ReasonKey1=>status:ReasonKey2=>status
                //
                strData += $"{key}=>{dictReasonStatus[key]}";
                ++cnt;
                if(cnt < dictReasonStatus.Count)
                    strData += ":";
            }
            if(!string.IsNullOrEmpty(strData))
            {
                Debug.Log($"[Noti] : Writing data... [{id}] => [{strData}]");
                PlayerPrefs.SetString($"{NOTI_KEYS}-{id}", strData);
            }*/
            return true;
        }


        public void Reset()
        {
            dictReasonStatus.Clear();
        }

        void LoadCacheFromPlayerPrefs()
        {
            /*
            if(sIdListCache.Count == 0)
            {
                string curData = PlayerPrefs.GetString(NOTI_KEYS, string.Empty);
                if(string.IsNullOrEmpty(curData))
                    Debug.Log($"[Noti] : No Key Found...");
                else
                    Debug.Log($"[Noti] : Key_list => [{curData}]");

                if (!string.IsNullOrEmpty(curData)) 
                {
                    // NotiA:NotiB:NotiC
                    string[] keys = curData.Split(':');
                    for(int i = 0; i < keys.Length; i++) 
                        sIdListCache.Add(keys[i]);
                }
            }


            dictReasonStatus.Clear();
            if(sIdListCache.Contains(id))
            {
                // Found Id ? => Find Sub Reason Keys.
                string id_key = $"{NOTI_KEYS}-{id}";
                string stateData = PlayerPrefs.GetString(id_key, string.Empty);
                Debug.Log($"[Noti] : [{id_key}] => [{stateData}]");
                
                // => ReasonKey0=>status:ReasonKey1=>status:ReasonKey2=>status
                if (!string.IsNullOrEmpty(stateData))
                {
                    string[] statuses = stateData.Split(":");
                    for (int q = 0; q < statuses.Length; q++)
                    {
                        // ReasonKeyA=>status
                        string keyState = statuses[q];
                        string[] reasonState = keyState.Split("=>");
                        Assert.IsTrue(reasonState.Length == 2);
                        Assert.IsTrue(!dictReasonStatus.ContainsKey(reasonState[0]), "Found conflicted key..." + reasonState[0]);

                        int reason_status = 0;
                        if (int.TryParse(reasonState[1], out reason_status))
                            dictReasonStatus.Add(reasonState[0], (STATUS)reason_status);
                        else
                        {
                            Assert.IsTrue(false, "Parse Failed !");
                        }
                    }
                }
            }
            else
            {
                // This is new Id ? => Update Noti_Keys PlayerPrefs.
                sIdListCache.Add(id);
                Debug.Log($"[Noti] : Adding new Noti Id.... [{id}]");

                // NotiA:NotiB:NotiC
                int cnt = 0;
                string strKeyData = string.Empty;
                foreach(string key in sIdListCache) 
                {
                    strKeyData += key;
                    ++cnt;
                    if(cnt < sIdListCache.Count)
                        strKeyData += ":";
                }
                PlayerPrefs.SetString(NOTI_KEYS, strKeyData);
            }*/
        }

#if UNITY_EDITOR
        //==========================================================================
        //
        // Editor - Reset Data Prefab
        //
        [UnityEditor.MenuItem("PlasticGames/Notificator Data/Clear All")]
        public static void ClearNotifierDataData()
        {
            /*string curData = PlayerPrefs.GetString(NOTI_KEYS, string.Empty);
            if (!string.IsNullOrEmpty(curData)) 
            {
                Debug.Log($"[Noti] : Id Keys have been cleared..... [{curData}]");
                string[] keys = curData.Split(':');
                for(int i = 0; i < keys.Length; i++)                 
                    PlayerPrefs.SetString($"{NOTI_KEYS}-{keys[i]}", string.Empty);
                
                PlayerPrefs.SetString(NOTI_KEYS, string.Empty);
            }*/
        }
#endif
    }
}
