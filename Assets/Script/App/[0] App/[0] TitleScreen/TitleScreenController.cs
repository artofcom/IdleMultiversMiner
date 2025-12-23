using Core.Events;
using Core.Util;
using IGCore.MVCS;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UnityConsent;

public class TitleScreenController : IGCore.MVCS.AController
{
    const float WAIT_TIME_SEC = 1.5f;

    IdleMinerContext IMContext => (IdleMinerContext)context;

    public TitleScreenController(IGCore.MVCS.AView view, IGCore.MVCS.AModel model, IGCore.MVCS.AContext ctx)
        : base(view, model, ctx)
    {}

    public override void Init()
    {}

    protected override void OnViewEnable()
    {
        Debug.Log("============================= Title Enter ");

        //AsyncInitService();

        DelayedAction.TriggerActionWithDelay(IMContext.CoRunner, WAIT_TIME_SEC, () =>
        {
            OnEventClose?.Invoke("LobbyScreen");
                //"PlayScreen");
        });
    }
    
    protected override void OnViewDisable() { }

    public override void Resume(int awayTimeInSec) { }
    
    public override void Pump() { }
    
    public override void WriteData() { }







    IEnumerator coInit()
    {
        //yield return new WaitUntil(() => _context.IsInitialized == true);
        yield return new WaitForSeconds(1.0f);

        //bool bDone = false;

        /*_view.StartCoroutine(_context.GameCtrlManager.CoLoadControllerBundle(
            (successed) =>
            {
                UnityEngine.Assertions.Assert.IsTrue(successed == true);
                // IsConfigLoaded = true;

                bDone = true;

            }));
        */
       // yield return new WaitUntil(() => bDone == true);
    }

    IEnumerator coLoadGameBundle()
    {
        yield break;
        /*
        string bundleName = "G001_CobraHearts";
        string assetName = // "SlotMainPortrait.prefab";//
                           "SlotMain.prefab";
        string LOCAL_BUNDLE_PATH = "Assets/Bundles";
        string assetPathExt = $"{bundleName}/{assetName}";// + GetFileExtension(typeof(T));
        string externalizedAssetPath = $"{LOCAL_BUNDLE_PATH}/{assetPathExt}";
        Debug.Log("[Fetching] Loading locally..." + externalizedAssetPath);
        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(externalizedAssetPath);
        */

        /*
        const string gameMainPrefab = "G033_IdleMiner/GameMain";

        GameObject prefab = Resources.Load($"Bundles/{gameMainPrefab}") as GameObject;
        UnityEngine.Assertions.Assert.IsNotNull(prefab);

        _context.GamePrefab = prefab;

        string loadingText = "Initializing";
        float fCur = Time.time;
    //    (_view as TitleScreenView).Refresh(loadingText);
        while(Time.time - fCur <= WAIT_TIME_SEC)
        {
            yield return new WaitForSeconds(0.45f);

            loadingText += ".";
   //         (_view as TitleScreenView).Refresh(loadingText);
        }

        // Bypass Lobby View for now.
        EventSystem.DispatchEvent("LobbyScreenView_OnGamePrefabLoaded"); // "TitleScreenView_OnClickBtnStart"
        
        */
    }
}