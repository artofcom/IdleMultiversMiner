using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using App.GamePlay.IdleMiner;
using System;
using UnityEngine.Assertions;
using System.Threading.Tasks;
using System.Numerics;

public class ValueDataEditor : EditorWindow
{
    const int MAX_RSC = 3;
    const string STORE_SUB_PATH = "/Resources/Bundles/G033_IdleMiner/EditorData";

    // Test.
    string mTextField = "Type in";
    public AnimationCurve curveX = AnimationCurve.Linear(0, 0, 1.0f, 1.0f);
    int mDropDownSelected = 0;


    int mViewType = 0;  // 0 - ResourceView, 1 - PlanetView.
    //int mPlanetCount = 10;
    bool mToggleFlag = true;
    string mOutputPath = string.Empty;
    int mSelectedRscType = 0;
    bool IsReloadSettingFailed = true;
    //int mSelectedPlanetIdx = 0;

    public class EditorRscInfo
    {
        public EditorRscInfo( ResourceData rscData, CurveInfo _curveInfo) 
        {
            this.RscData = rscData; this.CurveInfo = _curveInfo;
        }
        public ResourceData RscData {  get; private set; }
        public CurveInfo CurveInfo { get; private set; }

        //public AnimationCurve PriceCurve = AnimationCurve.Linear(0, 0, 1.0f, 1.0f);
        //public BigInteger PriceScale = 1;
        //public int PriceStartOffset = 0;
    }

    List<EditorRscInfo> RscInfoList; 
    //bool IsCalcuatePlanetOpenCost = true;
    PlanetSetting planetSetting;
   // PlanetDataGroup srcPlanetDataGroup;
    CraftData craftCompData, craftItemData;

    [MenuItem("PlasticGames/Editor/ValueDataEditor")]
    public static void ShowWindow()
    {
        GetWindow<ValueDataEditor>().Init();
    }

    public void Init()
    {
        IsReloadSettingFailed = false;
        mSelectedRscType = 0;

        RefreshSettingData();
    }
    public void InitEditor(List<EditorRscInfo> _editorRscInfo, PlanetSetting setting, CraftData compData, CraftData itemData)
    {
        this.RscInfoList = _editorRscInfo;
        this.planetSetting = setting;

        this.craftCompData = compData;
        this.craftItemData = itemData;

      //  srcPlanetDataGroup = JsonUtility.FromJson<PlanetDataGroup>( Resources.Load<TextAsset>(planetSetting.PlanetDataPath).text );
      //  srcPlanetDataGroup.Convert();

        Assert.IsTrue(RscInfoList.Count == MAX_RSC);
    }

    public bool RefreshSettingData()
    {
        System.Object[] selection = Selection.GetFiltered(typeof(PlanetSetting), SelectionMode.Assets);
        if (selection.Length > 0)
        {
            PlanetSetting settingPnl = selection[0] as PlanetSetting;
            if (settingPnl != null)
            {
                List<EditorRscInfo> infoList = new List<EditorRscInfo>();

                ResourceData rscData = JsonUtility.FromJson<ResourceData>( Resources.Load<TextAsset>(settingPnl.RscLevel0.JsonFilePath).text );
                infoList.Add(new EditorRscInfo(rscData, settingPnl.RscLevel0.PriceCurve));
                
                rscData = JsonUtility.FromJson<ResourceData>(Resources.Load<TextAsset>(settingPnl.RscLevel1.JsonFilePath).text);
                infoList.Add(new EditorRscInfo(rscData, settingPnl.RscLevel1.PriceCurve));
                
                rscData = JsonUtility.FromJson<ResourceData>(Resources.Load<TextAsset>(settingPnl.RscLevel2.JsonFilePath).text);
                infoList.Add(new EditorRscInfo(rscData, settingPnl.RscLevel2.PriceCurve));
                
                // Craft-Recipe Data.
                CraftData compCraftData = JsonUtility.FromJson<CraftData>(Resources.Load<TextAsset>(settingPnl.CraftCompDataPath).text);
                CraftData itemCraftData = JsonUtility.FromJson<CraftData>(Resources.Load<TextAsset>(settingPnl.CraftItemDataPath).text);   
                this.InitEditor(infoList, settingPnl, compCraftData, itemCraftData);
                return true;
            }
        }
        return false;
    }

    public void OnGUI()
    {
        mOutputPath = Application.dataPath + STORE_SUB_PATH;

        GUILayout.Label("Value Data Editor");

        OnGUIBaseDataSetting(spacing:30);

        switch (mViewType)
        {
        case 0:
            OnGUIResourceSection(spacing: 30);
            break;
        case 1:
            OnGUIPlanetDataSection(spacing: 30);
            break;
        case 2:
            OnGUICraftDataSection(spacing : 30);
            break;
        default:
            break;
        }
    }

    
    void OnGUIBaseDataSetting(int spacing)
    {
        GUILayout.Space(spacing);
        GUILayout.Label("[Base Data Setting]");
        GUILayout.Label("Output Path : " + mOutputPath);
        GUILayout.Space(10);

        if (GUILayout.Button("RELOAD SETTINGS - (Click PlanetSettingData)", GUILayout.Width(400)))
        {
            IsReloadSettingFailed = RefreshSettingData()==false;
        }

        if(IsReloadSettingFailed)
        {
            GUILayout.Label("=== Please select the SETTING-ASSET First.");
            TriggerActionWithDelayAsync(1000, () => IsReloadSettingFailed = false);
        }

        GUILayout.Space(20);
        string[] viewTypes = new string[] { "[0]-ResourceView", "[1]-PlanetView", "[2]-CraftView" };
        mViewType = EditorGUILayout.Popup("View Type", mViewType, viewTypes);
    }

    void OnGUIResourceSection(int spacing=10)
    {
        GUILayout.Space(spacing);

        GUILayout.Label("[Resource View] - Edit PRICE data.");
        GUILayout.Space(10);

        if(RscInfoList==null || RscInfoList.Count!=MAX_RSC)
        {
            GUILayout.Label("[Error] : Please select the SettingData asset and click [RELOAD SETTING] Button.");
            return;
        }

        string[] rscTypes = new string[]  {"[0]-Material", "[1]-Component", "[2]-Item" };
        mSelectedRscType = EditorGUILayout.Popup("RSC Type", mSelectedRscType, rscTypes);

        EditorRscInfo editorRsc = RscInfoList[mSelectedRscType];
        GUILayout.Label("Resource Count : " + editorRsc.RscData.Data.Count);
        GUILayout.Label($"Price Range : [{editorRsc.CurveInfo.Min}] ~ [{editorRsc.CurveInfo.Max}]");
        // 
        // editorRsc.PriceCurve = EditorGUILayout.CurveField("Resource Cost Curve", editorRsc.PriceCurve);


        GUILayout.Space(30);
        GUILayout.BeginHorizontal();
        {   
            if (GUILayout.Button($"Apply & Save cost to {rscTypes[mSelectedRscType]} RSC", GUILayout.Height(40), GUILayout.Width(300)))
            {
                ResourceData target = editorRsc.RscData;
                int count = target.Data.Count;
                for(int q = 0; q < target.Data.Count; ++q)
                    target.Data[q].Price = CurveToString(editorRsc.CurveInfo, ((float)(q + 1)) / ((float)count) );

                string[] fileNameTypes = new string[] { "Mat", "Comp", "Item" };
                WriteFile(mOutputPath + $"/Resource_{fileNameTypes[mSelectedRscType]}.json", target);
            }
        }
        GUILayout.EndHorizontal();
    }


    void OnGUIPlanetDataSection(int spacing)
    {
        GUILayout.Space(spacing);

        GUILayout.Label("[Planet View] - Edit OpenCost / MiningRate / UpgradeCost");
        GUILayout.Space(10);

        #region DATA EXAMPLE - COMMENT
        /* Single Planet Data. ===
         * 
        id": 1,
            "name": "TownA",
            "openCost": "100",
            "obtainables": [
                {
                    "resourceId": "DUST",
                    "yield": 1.0
                }
            ],
            "miningRate": "base=1:incPercent=0:incBase=0",
            "shipSpeed": "base=0.1: incPercent=10:incBase=0.1",
            "cargoSize": "base=1: incPercent=10:incBase=0.5",
            "shotAccuracy": "base=50: incPercent=1:incBase=30",
            "shotInterval": "base=5: incPercent=1:incBase=-5",

            "miningCost": "base=10:incPercent=100:incBase=12",
            "shipCost": "base=10:incPercent=100:incBase=10",
            "cargoCost": "base=10:incPercent=100:incBase=12",
            "shotAccuracyCost": "base=10:incPercent=100:incBase=12",
            "shotIntervalCost": "base=10:incPercent=100:incBase=12",
        */
        #endregion

        /*if (planetSetting==null || srcPlanetDataGroup == null || srcPlanetDataGroup.Data == null)
        {
            GUILayout.Label("[Error] :Try clicking [RELOAD SETTING].");
            return;
        }*/

        GUILayout.Label("Input Data Path : " + "Resources/" + planetSetting.PlanetDataPath + ".json");
        //GUILayout.Label("Option");
        //bool toggleRet = GUILayout.Toggle(IsCalcuatePlanetOpenCost, "Reset Open Cost", GUILayout.Height(30));
        //if (IsCalcuatePlanetOpenCost != toggleRet)
        //    IsCalcuatePlanetOpenCost = toggleRet;

        /*

        mPlanetCount = srcPlanetDataGroup.Data.Count;
        if (GUILayout.Button($"BUILD Planet Data - Count : {mPlanetCount} ", GUILayout.Height(40), GUILayout.Width(300)) && mPlanetCount>0)
        {
            // Verify data first. 
            PlanetData firstData = srcPlanetDataGroup.Data[0];
            LevelBasedFloat mrRate = firstData.LBStat(eABILITY.MINING_RATE);
            LevelBasedFloat speed = firstData.LBStat(eABILITY.DELIVERY_SPEED);
            LevelBasedFloat cargo = firstData.LBStat(eABILITY.CARGO_SIZE);
            LevelBasedFloat sAcc = firstData.LBStat(eABILITY.SHOT_ACCURACY);
            LevelBasedFloat sInt = firstData.LBStat(eABILITY.SHOT_INTERVAL);
                
            LevelBasedBigInteger costMrRate = firstData.LBCost(eABILITY.MINING_RATE);
            LevelBasedBigInteger costSpeed = firstData.LBCost(eABILITY.DELIVERY_SPEED);
            LevelBasedBigInteger costCargo = firstData.LBCost(eABILITY.CARGO_SIZE);
            LevelBasedBigInteger costSAcc = firstData.LBCost(eABILITY.SHOT_ACCURACY);
            LevelBasedBigInteger costSInt = firstData.LBCost(eABILITY.SHOT_INTERVAL);


            List<PlanetData> planetsList = new List<PlanetData>();
            for (int q = 0; q < mPlanetCount; ++q)
            {
                // If we have source data, then utilize it as possible.
                PlanetData curPData = srcPlanetDataGroup.Data[q];
                
                //
                float curveTime = ((float)(q+1)) / ((float)mPlanetCount);
                BigInteger curveValue = GetCurveValue<BigInteger>(planetSetting.PlanetCostCurve, curveTime);
                string newOpenCost = IsCalcuatePlanetOpenCost ? curveValue.ToString() : curPData.OpenCost;

                PlanetData planetData = new PlanetData(curPData.Id, curPData.Name, newOpenCost, curPData.Obtainables,
                    miningRate:$"base={CurveToString(planetSetting.MiningRateCurve, curveTime)}:incPercent={mrRate.IncreasePercent}:incBase={mrRate.IncreaseBase}",
                    shipSpeed:$"base={CurveToString(planetSetting.ShipSpeedCurve, curveTime)}: incPercent={speed.IncreasePercent}:incBase={speed.IncreaseBase}",
                    cargoSize:$"base={CurveToString(planetSetting.CargoSizeCurve, curveTime)}: incPercent={cargo.IncreasePercent}:incBase={cargo.IncreaseBase}",
                    shotAccuracy:$"base={CurveToString(planetSetting.FireAccuracyCurve, curveTime)}: incPercent={sAcc.IncreasePercent}:incBase={sAcc.IncreaseBase}",
                    shotInterval: $"base={CurveToString(planetSetting.FireIntervalCurve, curveTime)}: incPercent={sInt.IncreasePercent}:incBase={sInt.IncreaseBase}",

                    miningCost:$"base={CurveToString(planetSetting.MiningRateCostCurve, curveTime)}:incPercent={costMrRate.IncreasePercent}:incBase={costMrRate.IncreaseBase}",
                    shipCost:$"base={CurveToString(planetSetting.ShipSpeedCostCurve, curveTime)}:incPercent={costSpeed.IncreasePercent}:incBase={costSpeed.IncreaseBase}",
                    cargoCost:$"base={CurveToString(planetSetting.CargoSizeCostCurve, curveTime)}:incPercent={costCargo.IncreasePercent}:incBase={costCargo.IncreaseBase}",
                    shotAccuracyCost:$"base={CurveToString(planetSetting.FireAccuracyCostCurve, curveTime)}:incPercent={costSAcc.IncreasePercent}:incBase={costSAcc.IncreaseBase}",
                    shotIntervalCost:$"base={CurveToString(planetSetting.FireIntervalCostCurve, curveTime)}:incPercent={costSInt.IncreasePercent}:incBase={costSInt.IncreaseBase}");

                planetsList.Add(planetData);

                Debug.Log(planetData.Id);
                Debug.Log(planetData.Name);
            }
            WriteFile(mOutputPath + "/PlanetData.json", new PlanetDataGroup(planetsList));
        }

        if(srcPlanetDataGroup != null && srcPlanetDataGroup.Data!=null && srcPlanetDataGroup.Data.Count>0)
        {
            GUILayout.Space(10);
            //GUILayout.BeginArea();
            GUILayout.Label("[Single Planet Section]");
        
            List<string> planetNames = new List<string>();
            for(int q = 0; q < srcPlanetDataGroup.Data.Count; ++q)
            {
                planetNames.Add($"{srcPlanetDataGroup.Data[q].Id}-{srcPlanetDataGroup.Data[q].Name}");
            }
            mSelectedPlanetIdx = EditorGUILayout.Popup("Selected Planet", mSelectedPlanetIdx, planetNames.ToArray());
            PlanetData curPlanetData = srcPlanetDataGroup.Data[mSelectedPlanetIdx];
            GUILayout.Label($"Id : {curPlanetData.Id}");
            GUILayout.Label($"Name : {curPlanetData.Name}");
            GUILayout.Label($"Open Cost : {curPlanetData.OpenCost}");
            GUILayout.Label($"Obtainable Count : {curPlanetData.Obtainables.Count}");
            for(int q = 0; q < curPlanetData.Obtainables.Count; ++q)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Rsc Id : {curPlanetData.Obtainables[q].ResourceId}");
                GUILayout.Label($"Rsc Yield : {curPlanetData.Obtainables[q].Yield}");
                GUILayout.EndHorizontal();
            }
        }*/
    }


    // CraftData -> cost
    // CraftData -> slotCosts 
    // CraftData -> duration
    void OnGUICraftDataSection(int spacing)
    {
        GUILayout.Space(spacing);

        if(this.craftCompData==null || this.craftItemData==null)
        {
            GUILayout.Label("Craft Data has NOT initialized!");
            return;
        }

        GUILayout.Label($"Craft Comp Recipe Count : {craftCompData.Recipes.Count}");
        GUILayout.Label($"Craft Item Recipe Count : {craftItemData.Recipes.Count}");

        GUILayout.BeginHorizontal();
        {   
            if (GUILayout.Button("COMP Recipe Data - Apply & Save", GUILayout.Height(40), GUILayout.Width(300)))
                SaveCraftData(eRscStageType.COMPONENT);

            if (GUILayout.Button("ITEM Recipe Data - Apply & Save", GUILayout.Height(40), GUILayout.Width(300)))
                SaveCraftData(eRscStageType.ITEM);   
        }
        GUILayout.EndHorizontal();
    }

    void SaveCraftData(eRscStageType type)
    {
        CraftData craftData;
        CurveInfo slotCostCurve, recipeCostCurve, recipeDurationCurve;
        string outputFileKey;
        switch(type)
        {
        case eRscStageType.COMPONENT:
            craftData = this.craftCompData;
            slotCostCurve = planetSetting.CraftCompSlotCostCurve;
            recipeCostCurve = planetSetting.CraftCompRecipeCostCurve;
            recipeDurationCurve = planetSetting.CraftCompRecipeDurationCurve;
            outputFileKey = "Comp";
            break;
        case eRscStageType.ITEM:
            craftData = this.craftItemData;
            slotCostCurve = planetSetting.CraftItemSlotCostCurve;
            recipeCostCurve = planetSetting.CraftItemRecipeCostCurve;
            recipeDurationCurve = planetSetting.CraftItemRecipeDurationCurve;
            outputFileKey = "Item";
            break;
        default:
            Assert.IsTrue(false, "Unsupported Type.." + type);
            return;
        }

        // Slot-Cost
        craftData.SlotCosts.Clear();
        for(int q = 0; q < CraftData.MAX_SLOT; ++q)
        {
            float curveTime = ((float)(q+1)) / ((float)CraftData.MAX_SLOT);
            craftData.SlotCosts.Add(CurveToString(slotCostCurve, curveTime));       //"100"); 
        }

        // Recipe Cost & Duration.
        for(int q = 0; q < craftData.Recipes.Count; ++q)
        {
            float curveTime = ((float)(q+1)) / ((float)craftData.Recipes.Count);
            craftData.Recipes[q].Cost = CurveToString(recipeCostCurve, curveTime); // "1000";
            craftData.Recipes[q].Duration = int.Parse(CurveToString(recipeDurationCurve, curveTime));
        }

        WriteFile(mOutputPath + $"/Craft_{outputFileKey}.json", craftData);
    }

    #region ### TESTS
    void AllTests()
    {

        //if (RscDataLV0 != null)
        //    GUILayout.Label("Rsc-Count : " + RscDataLV0.Data.Count);

        bool toggleRet = GUILayout.Toggle(mToggleFlag, "Comment", GUILayout.Height(30));
        if (mToggleFlag != toggleRet)
        {
            Debug.Log("Toggle Updated");
            mToggleFlag = toggleRet;
        }


        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("CCC");
            GUILayout.Label("DDD");

            if (GUILayout.Button("Build Data", GUILayout.Width(200)))
            {
                Debug.Log("Button Clicked");
                // DoSomeWork();
            }
        }
        GUILayout.EndHorizontal();




        GUILayout.Space(10);

        // This is the dropdown button.
        string[] options = new string[]
        {
            "Option1", "Option2", "Option3",
        };
        mDropDownSelected = EditorGUILayout.Popup("Label", mDropDownSelected, options);

        curveX = EditorGUILayout.CurveField("AAA", curveX);

        string strTextInput = GUILayout.TextField(mTextField, GUILayout.Width(300));
        if (mTextField != strTextInput)
            mTextField = strTextInput;


        if (GUILayout.Button("Open File", GUILayout.Width(200)))
        {
            string filePath = EditorUtility.OpenFilePanel("Load data", "", "json");
            //FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            string data = File.ReadAllText(filePath);
            GUILayout.Label(data);
            Debug.Log(data);
        }

    }
    #endregion

    #region ### Utils
    bool WriteFile(string filePath, object data)
    { 
        StreamWriter streamWriter = new StreamWriter(filePath);
        if(streamWriter == null)
            return false;

        string jsonStr = JsonUtility.ToJson(data, prettyPrint: true);
        streamWriter.Write(jsonStr);
        streamWriter.Close();
        streamWriter.Dispose();

        Debug.Log(jsonStr);
        return true;
    }

    string CurveToString(CurveInfo curve, float time)
    {
        if (curve == null)
            return string.Empty;

        if (curve.IsFloatCast)
            return GetCurveValue<float>(curve, time).ToString();
        
        return GetCurveValue<BigInteger>(curve, time).ToString();
    }

    T GetCurveValue<T>(CurveInfo curve, float time)
    {
        if (curve == null)
            return default(T);

        return GetCurveValue<T>(curve.Min, curve.Max, curve.Curve, time, curve.Isx1000Value);
    }

    T GetCurveValue<T>(BigInteger min, BigInteger max, AnimationCurve curve, float time, bool isX100Value = false)
    {
        if (curve == null)
            return default(T);

        BigInteger diff = max - min;
        if (isX100Value && diff <= BigInteger.One && diff >= -BigInteger.One)   // meaning T is float.
        {
            float ret = ((float)min) * 0.001f;
            return (T)((object)ret);    //(T)Convert.ChangeType(min, typeof(T));
        }

        int curveValuex1000 = (int)(1000.0f * curve.Evaluate(time));
        BigInteger biValue;
        biValue = (max - min) * curveValuex1000;
        biValue /= 1000;
        biValue += min;

        Type inputType = typeof(T);
        if(inputType == typeof(int) || inputType == typeof(BigInteger))
        {
            biValue = isX100Value ? biValue / 1000 : biValue;
            biValue = RoundValue(biValue);
            return (T)Convert.ChangeType(biValue, typeof(T));
        }

        if(inputType == typeof(float))
        {
            float fValue = isX100Value ? ((float)biValue) * 0.001f : (float)biValue;
            return (T)Convert.ChangeType(fValue, typeof(T));
        }

        Assert.IsTrue(false, "Unsupported Curve Type !!! : " + inputType.ToString());
        return default(T);
    }

    BigInteger RoundValue(BigInteger biValue)
    {
        if(biValue < 10)        return biValue;
        if(biValue < 100)       // 10 ~ 99
        {
            return biValue - (biValue%10);
        }
        if(biValue < 1000)      // 100 ~ 999
        {
            return biValue - (biValue%100);
        }
        return biValue - (biValue%100);
    }

    /*
    BigInteger GetCurveValue(BigInteger min, BigInteger max, AnimationCurve curve, float time, bool isX100Value=false)
    {
        if (curve == null)
            return BigInteger.Zero;

        int curveValuex1000 = (int)(1000.0f * curve.Evaluate(time));
        
        
        BigInteger biValue = (max-min) * curveValuex1000;
        biValue /= 1000;
        biValue += min;

        return biValue;
    }
    */

    async void TriggerActionWithDelayAsync(int milliSec, Action action)
    {
        await Task.Delay(milliSec);

        action?.Invoke();
    }
    #endregion
}


#region ### BackUp Codes.
/*GUILayout.Label("[Minable Resources]");
  //GUILayout.Space(10);
  ResourceData rscMatData = RscInfoList[0].RscData;
  for(int q = 0; q < rscMatData.Data.Count; ++q)
  {
      GUILayout.Button($"[{q}]-[{rscMatData.Data[q].Id}]", GUILayout.Width(200));
  }*/
/*
 * void DoSomeWork()
    {
        //string jsonText = "{\"id\": 1,\r\n            \"name\": \"TownA\",\r\n            \"openCost\": \"100\",\r\n            \"obtainables\": [\r\n                {\r\n                    \"resourceId\": \"DUST\",\r\n                    \"yield\": 1.0\r\n                }\r\n            ],\r\n            \"miningRate\": \"base=1:incPercent=0:incBase=0\",\r\n            \"shipSpeed\": \"base=0.1: incPercent=10:incBase=0.1\",\r\n            \"cargoSize\": \"base=1: incPercent=10:incBase=0.5\",\r\n            \"shotAccuracy\": \"base=50: incPercent=1:incBase=30\",\r\n            \"shotInterval\": \"base=5: incPercent=1:incBase=-5\",\r\n            \"shotCount\": \"base=1:incPercent=10:incBase=1\",\r\n\r\n            \"miningCost\": \"base=10:incPercent=100:incBase=12\",\r\n            \"shipCost\": \"base=10:incPercent=100:incBase=10\",\r\n            \"cargoCost\": \"base=10:incPercent=100:incBase=12\",\r\n            \"shotAccuracyCost\": \"base=10:incPercent=100:incBase=12\",\r\n            \"shotIntervalCost\": \"base=10:incPercent=100:incBase=12\",\r\n            \"shotCountCost\": \"base=10:incPercent=100:incBase=12\"}";
        int count = 100;
        List<PlanetData> planetsList = new List<PlanetData>();
        
        
        for (int q = 0; q < count; ++q)
        {
           // PlanetData planetData = new PlanetData();
           // planetData = JsonUtility.FromJson<PlanetData>(jsonText);

            float curveValue = curveX.Evaluate( ((float)q) / ((float)count) );
            Debug.Log( $"{q} curve value.... : {curveValue}" );

            List<ObtainStat> obtainables = new List<ObtainStat>();
            for (int z = 0; z < 1; ++z)
            {
                ObtainStat rsc = new ObtainStat("rsc_" + q, 1.0f);
                obtainables.Add(rsc);
            }
            //PlanetData planetData = new PlanetData($"{q+1}", $"AAA{q}", curveValue);
            PlanetData planetData = new PlanetData(q+1, "Name"+q, "100", obtainables,
                "base=1:incPercent=0:incBase=0",
                "base=0.1: incPercent=10:incBase=0.1",
                "base=1: incPercent=10:incBase=0.5",
                "base=50: incPercent=1:incBase=30",
                "base=5: incPercent=1:incBase=-5",

                "base=10:incPercent=100:incBase=12",
                "base=10:incPercent=100:incBase=10",
                "base=10:incPercent=100:incBase=12",
                "base=10:incPercent=100:incBase=12",
                "base=10:incPercent=100:incBase=12");


            planetsList.Add(planetData);

            Debug.Log(planetData.Id);
            Debug.Log(planetData.Name);
        }

        WriteFile(mOutputPath + "/p1.json", new PlanetDataGroup(planetsList));
    }*/


//toggleRet = GUILayout.Toggle(IsResetObtainables, "Reset Obtainables", GUILayout.Height(30));
//if (IsResetObtainables != toggleRet)
//     IsResetObtainables = toggleRet;


/*GUILayout.BeginHorizontal();
{
    GUILayout.Label("Planet Count : ");
    string strCurCount = mPlanetCount.ToString();
    string strTextInput = GUILayout.TextField(strCurCount, GUILayout.Width(300));
    if (strCurCount != strTextInput)
    {
        int retParse;
        if(int.TryParse(strTextInput, out retParse))
            mPlanetCount = retParse;
    }
}
GUILayout.EndHorizontal();
*/

#endregion