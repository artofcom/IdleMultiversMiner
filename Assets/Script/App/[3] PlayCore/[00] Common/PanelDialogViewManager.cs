using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace App.GamePlay.IdleMiner
{
    public class PanelDialogViewManager : MonoBehaviour
    {
        /*
        Dictionary<string, ARefreshable> DictPnlDialogs = new Dictionary<string, ARefreshable>();

        // Start is called before the first frame update
        void Start()
        {
            for (int q = 0; q < transform.childCount; ++q)
            {
                var child = transform.GetChild(q).GetComponent<ARefreshable>();
                if (child != null)
                {
                    Assert.IsTrue(!DictPnlDialogs.ContainsKey(child.gameObject.name));

                    DictPnlDialogs.Add(child.gameObject.name, child);
                    Debug.Log($"[PanelDialogView] : [{child.gameObject.name}] has been added.");
                }
            }
        }

        public ARefreshable GetPanelView(string key, bool enableLooseSearch=true)
        {
            if (DictPnlDialogs.ContainsKey(key))
                return DictPnlDialogs[key];


            if(enableLooseSearch)
            {
                foreach(string dictKey in DictPnlDialogs.Keys)
                {
                    if (dictKey.Contains(key))
                        return DictPnlDialogs[dictKey];

                    if (dictKey.ToLower().Contains(key.ToLower()))
                        return DictPnlDialogs[dictKey];
                }
            }

            return null;
        }

        public void CloseAllPanels()
        {
            ClosePanel(key : string.Empty);
        }

        public void ClosePanel(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                foreach (string dictKey in DictPnlDialogs.Keys)
                {
                    DictPnlDialogs[dictKey].gameObject.SetActive(false);
                }
            }
            else
            {
                var panel = GetPanelView(key);
                if (panel != null)
                    panel.gameObject.SetActive(false);
            }

        }*/
    }



}
