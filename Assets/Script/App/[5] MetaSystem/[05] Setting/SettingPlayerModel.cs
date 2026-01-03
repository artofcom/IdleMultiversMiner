using App.GamePlay.Common;
using App.GamePlay.IdleMiner.Common.PlayerModel;
using IGCore.MVCS;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class  SettingData
{
    [SerializeField] bool isSoundFXOn = true;
    [SerializeField] bool isBGMOn = true;

    public bool IsSoundFXOn { get =>  isSoundFXOn; set => isSoundFXOn = value; }
    public bool IsBGMOn { get => isBGMOn; set => isBGMOn = value; }
}

public class SettingPlayerModel : MultiGatewayWritablePlayerModel
{
    SettingData settingData;

    public SettingPlayerModel(AContext ctx, List<IDataGatewayService> gatewayServices) : base(ctx, gatewayServices) { }

    string DataKey => "SettingData";

    public override void Init()
    {
        base.Init();

        LoadSettingData();

        RegisterRequestables();

        IsInitialized = true;
    }

    public override void Dispose()
    {
        base.Dispose();

        UnregisterRequestables();

        IsInitialized = false;
    }

    void LoadSettingData()
    {
        FetchData((context as IdleMinerContext).ValidGatewayServiceIndex, DataKey, out settingData, new SettingData());
    }

    public override List<Tuple<string, string>> GetSaveDataWithKeys()
    {
        List<Tuple<string, string>> listDataSet = new List<Tuple<string, string>>();
        if(settingData != null) 
            listDataSet.Add(new Tuple<string, string>(DataKey, JsonUtility.ToJson(settingData)));
        return listDataSet;
    }

    void RegisterRequestables()
    {
        context.AddRequestDelegate("AppPlayerModel", "SetBGM", setBGM);
        context.AddRequestDelegate("AppPlayerModel", "IsBGMOn", IsBGMOn);
        context.AddRequestDelegate("AppPlayerModel", "SetSoundFX", setSoundFX);
        context.AddRequestDelegate("AppPlayerModel", "IsSoundFXOn", IsSoundFXOn);
    }

    void UnregisterRequestables()
    {
        context.RemoveRequestDelegate("AppPlayerModel", "SetBGM");
        context.RemoveRequestDelegate("AppPlayerModel", "IsBGMOn");
        context.RemoveRequestDelegate("AppPlayerModel", "SetSoundFX");
        context.RemoveRequestDelegate("AppPlayerModel", "IsSoundFXOn");
    }

    object setBGM(params object[] data)
    {
        if(data.Length < 1)
            return null;

        bool isOn = (bool)data[0];
        if(settingData.IsBGMOn == isOn)
            return settingData.IsBGMOn;

        settingData.IsBGMOn = isOn;
        (context as IdleMinerContext).SaveMetaData();
        return settingData.IsBGMOn;
    }
    object IsBGMOn(params object[] data)
    {
        return settingData.IsBGMOn;
    }
    object setSoundFX(params object[] data)
    {
        if(data.Length < 1)
            return null;

        bool isOn = (bool)data[0];
        if(settingData.IsSoundFXOn == isOn)
            return settingData.IsSoundFXOn;

        settingData.IsSoundFXOn = isOn;
        (context as IdleMinerContext).SaveMetaData();
        return settingData.IsSoundFXOn;
    }
    object IsSoundFXOn(params object[] data)
    {
        return settingData.IsSoundFXOn;
    }
}
