using App.GamePlay.IdleMiner;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Numerics;

[Serializable]
public class CurveInfo : ISerializationCallbackReceiver
{
    [SerializeField] AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1.0f, 1.0f);
    [SerializeField] string min = "0", max = "100";

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        if (min.Contains(".") || max.Contains("."))
        {
            IsFloatCast = true; 
            Isx1000Value = true;
            float fTemp;
            if (float.TryParse(min, out fTemp))
                _min = (int)(fTemp * 1000.0f);
            if (float.TryParse(max, out fTemp))
                _max = (int)(fTemp * 1000.0f);
        }
        else
        {
            IsFloatCast = false;
            Isx1000Value = false;
            BigInteger biTemp = new BigInteger(1);
            if (BigInteger.TryParse(min, out biTemp))
                _min = biTemp;
            if (BigInteger.TryParse(max, out biTemp))
                _max = biTemp;
        }
    }

    BigInteger _min, _max;

    public bool Isx1000Value { get; private set; }  
    public bool IsFloatCast { get; private set; }   // otherwise BigInt ?

    public BigInteger Scale = 1;
    public BigInteger Min => _min;
    public BigInteger Max => _max;
    public AnimationCurve Curve => curve;
}

[Serializable]
public class ResourceSetting
{
    public CurveInfo PriceCurve;
    [SerializeField] string JsonPath;

    public string JsonFilePath => JsonPath;
}


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlanetSetting", order = 1)]
public class PlanetSetting : ScriptableObject
{
    [Header("=== Resource Area - Sell Price")]
    [SerializeField] ResourceSetting rscLevel0;
    [SerializeField] ResourceSetting rscLevel1;
    [SerializeField] ResourceSetting rscLevel2;

    [Header("=== Planet Area - Open Cost")]
    [SerializeField] string planetDataPath;
    [SerializeField] CurveInfo planetCostCurve;

    [Header("=== Planet Area - Base Stat")]
    [SerializeField] CurveInfo miningRateCurve;
    [SerializeField] CurveInfo cargoSizeCurve;
    [SerializeField] CurveInfo shipSpeedCurve;
    [SerializeField] CurveInfo fireIntervalCurve;
    [SerializeField] CurveInfo fireAccuracyCurve;

    [Header("=== Planet Area - Upgrade Cost")]
    [SerializeField] CurveInfo miningRateCostCurve;
    [SerializeField] CurveInfo cargoSizeCostCurve;
    [SerializeField] CurveInfo shipSpeedCostCurve;
    [SerializeField] CurveInfo fireIntervalCostCurve;
    [SerializeField] CurveInfo fireAccuracyCostCurve;


    [Header("=== RecipeData Area - Upgrade Cost")]
    [SerializeField] string craftCompDataPath;
    [SerializeField] CurveInfo craftCompRecipeCostCurve;
    [SerializeField] CurveInfo craftCompRecipeDurationCurve;
    [SerializeField] CurveInfo craftCompSlotCostCurve;
    [Header("")]
    [SerializeField] string craftItemDataPath;
    [SerializeField] CurveInfo craftItemRecipeCostCurve;
    [SerializeField] CurveInfo craftItemRecipeDurationCurve;
    [SerializeField] CurveInfo craftItemSlotCostCurve;


    public ResourceSetting RscLevel0 => rscLevel0;
    public ResourceSetting RscLevel1 => rscLevel1;
    public ResourceSetting RscLevel2 => rscLevel2;
    
    public CurveInfo PlanetCostCurve => planetCostCurve;
    
    public string PlanetDataPath => planetDataPath;

    public CurveInfo MiningRateCurve => miningRateCurve;
    public CurveInfo CargoSizeCurve => cargoSizeCurve;
    public CurveInfo ShipSpeedCurve => shipSpeedCurve;
    public CurveInfo FireIntervalCurve => fireIntervalCurve;
    public CurveInfo FireAccuracyCurve => fireAccuracyCurve;

    public CurveInfo MiningRateCostCurve => miningRateCostCurve;
    public CurveInfo CargoSizeCostCurve => cargoSizeCostCurve;
    public CurveInfo ShipSpeedCostCurve => shipSpeedCostCurve;
    public CurveInfo FireIntervalCostCurve => fireIntervalCostCurve;
    public CurveInfo FireAccuracyCostCurve => fireAccuracyCostCurve;

    public string CraftCompDataPath => craftCompDataPath;
    public CurveInfo CraftCompRecipeCostCurve => craftCompRecipeCostCurve;
    public CurveInfo CraftCompRecipeDurationCurve => craftCompRecipeDurationCurve;
    public CurveInfo CraftCompSlotCostCurve => craftCompSlotCostCurve;

    public string CraftItemDataPath => craftItemDataPath;
    public CurveInfo CraftItemRecipeCostCurve => craftItemRecipeCostCurve;
    public CurveInfo CraftItemRecipeDurationCurve => craftItemRecipeDurationCurve;
    public CurveInfo CraftItemSlotCostCurve => craftItemSlotCostCurve;


    private void OnEnable()
    {}

    private void OnValidate()
    {
        /*
        rscLevel0.Convert();
        rscLevel1.Convert();
        rscLevel2.Convert();

        planetCostCurve.Convert();

        miningRateCurve.Convert();
        cargoSizeCurve.Convert();
        shipSpeedCurve.Convert();
        fireIntervalCurve.Convert();
        fireAccuracyCurve.Convert();

        miningRateCostCurve.Convert();
        cargoSizeCostCurve.Convert();
        shipSpeedCostCurve.Convert();
        fireIntervalCostCurve.Convert();
        fireAccuracyCostCurve.Convert();

        craftCompRecipeCostCurve.Convert();
        craftCompRecipeDurationCurve.Convert();
        craftCompSlotCostCurve.Convert();

        craftItemRecipeCostCurve.Convert();
        craftItemRecipeDurationCurve.Convert();
        craftItemSlotCostCurve.Convert();
        */

        Debug.Log("OnValidate called...");
    }

    
}
