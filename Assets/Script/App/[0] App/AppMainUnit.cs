using App.GamePlay.IdleMiner;
using Core.Events;
using IGCore.MVCS;
using IGCore.PlatformService.Cloud;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AppMainUnit : AUnit
{
    [SerializeField] UnitSwitcherComp unitSwitcher;

    // MetaSystems.
    [SerializeField] List<AUnit> metaSystems;

    [ImplementsInterface(typeof(ICloudService))]
    [SerializeField] MonoBehaviour cloudService;

    AContext _minerContext = null;

    protected override void Awake() 
    { 
        base.Awake();
        
        //_minerContext = new IdleMinerContext();
        _minerContext = new IdleMinerContext(cloudService as ICloudService);
        
        ((IdleMinerContext)_minerContext).Init(this);

        Init(_minerContext);

        var playerModel = new AppPlayerModel(_minerContext, ((IdleMinerContext)_minerContext).MetaDataGatewayService);
        model = new AppModel(_minerContext, playerModel);
        controller = new AppController(this, view, model, _minerContext);

        playerModel.Init();
        model.Init();
        controller.Init();

        unitSwitcher.Init(_minerContext);

        if(metaSystems != null)
        {
            for(int q = 0; q < metaSystems.Count; q++) 
                metaSystems[q].Init(_minerContext);
        }
    }

    protected void Start()
    {
        Application.targetFrameRate = 61;
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application Quit.");
    }


#if UNITY_EDITOR
    [UnityEditor.MenuItem("PlasticGames/Clear PlayerData/All")]
    private static void ClearPlayerPrefab()
    {
        AppPlayerModel.ClearAllData();
        IdleMinerPlayerModel.ClearAllData();
        PlayerPrefs.DeleteAll();

        Debug.Log("Deleting All PlayerPrefab...");
    }
#endif
}
