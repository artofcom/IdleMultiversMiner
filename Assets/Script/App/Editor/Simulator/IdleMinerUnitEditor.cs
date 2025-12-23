using App.GamePlay.IdleMiner;
using Core.Events;
using Core.Utils;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IdleMinerUnit))]
public class IdleMinerUnitEditor : Editor
{
    IdleMinerUnit IMU;
    IdleMinerContext context = null;

    EditorCoroutine mainCoroutine = null;
    
    public override void OnInspectorGUI()
    {
        if(IMU == null)
            IMU = (IdleMinerUnit)target;

        DrawDefaultInspector(); 

        GUILayout.Label("");
        GUILayout.Label("========== Editor Area ============");
        GUILayout.Label("");
        
        if (GUILayout.Button("<< Run Simulator ! >>", GUILayout.Height(50.0f)))
        {
            if(mainCoroutine != null)
                EditorCoroutineUtility.StopCoroutine(mainCoroutine);
            mainCoroutine = EditorCoroutineUtility.StartCoroutine(SimulateGame(), this);
            // IdleMinerUnitEditorWindow.ShowWindow(IMU);
        }
    }



    IEnumerator SimulateGame()
    {
        if(context == null) 
            context = new IdleMinerContext();

        context.ClearData();
        context.AddData("IsSimMode", true);
        context.AddData("gamePath", IMU.SimulationPath);

        context.AddData("ShouldSellItem", IMU.ShouldSellItem);
        context.AddData("DesiredPDR", IMU.DesiredPDR);
        context.AddData("CraftSlotCount", IMU.CraftSlotCount);

        // Initialize simulation logger
        //SimulationLogger.Initialize(IMU.SimulationPath);

        IMU.Init(context);
        IMU.Resume(1);

        int cnt = 0;
        int loopTimeInSec = IMU.SimulatorFrameUpdateSec;
        int elTimeSec = loopTimeInSec;
        while(cnt <= IMU.SimulatorCount)
        {
            string strElTime = TimeExt.ToTimeString(elTimeSec+loopTimeInSec, TimeExt.UnitOption.FULL_NAME, TimeExt.TimeOption.DAY);
            Debug.Log($"<color=#FF4500>[SIM] ======================================== Running {cnt}th loop...{strElTime} =============================== </color>");
            context.UpdateData("SimTime", strElTime);

            // GamePlay - Mining, Shipping, OpenPlanet, UpgradeMiningStat
            // Resource - Sell
            // Craft - AssignRecipe ( GetResourceReqFromSkill -> FreeUp_UnusedRecipe_FromSlot -> Assign_Recipe )
            // SkillTree 
            IMU.Resume(loopTimeInSec);

            yield return new EditorWaitForSeconds(IMU.SimulatorFrameSec);
            
            elTimeSec += loopTimeInSec;
            ++cnt;
        }

        mainCoroutine = null;
        Debug.Log("[SIM] : Finished.");
        //SimulationLogger.Close();
    }
}
