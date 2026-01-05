using IGCore.PlatformService.Cloud;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IGCore.PlatformService
{
    public interface IDataGatewayService 
    {
        string AccountId { get; set; }
        bool IsDirty { get; set; }

        void RegisterDataModel(IWritableModel model);
        void UnRegisterDataModel(IWritableModel model);
        void ClearModels();

        string GetData(string model_id);
    }


    public interface ILocalDataGatewayService : IDataGatewayService
    {
        Task<bool> WriteData(string dataKey, bool clearAll);
        Task<bool> ReadData(string dataKey);
    }

    public interface ICloudDataGatewayService : IDataGatewayService
    {
        Task<ICloudService.ResultType> WriteData(string dataKey, bool clearAll);
        Task<ICloudService.ResultType> ReadData(string dataKey);
    }


    namespace DataGateWay
    {
        [Serializable]
        public class DataPair
        {
            public DataPair() { }
            public DataPair(string key, string value) 
            {
                this.key = key;     this.value = value;
            }
            [SerializeField] string key;
            [SerializeField] string value;

            public string Key => key;
            public string Value => value;
        }

        [Serializable]
        public class  EnvironmentInfo
        {
            [SerializeField] string dataVersion;
            [SerializeField] string appVersion;
            [SerializeField] string deviceName;
            [SerializeField] long timeStamp;

            public EnvironmentInfo(string version, long timeStamp)
            {
                this.dataVersion = version;     
                this.appVersion = Application.version;    this.deviceName = SystemInfo.deviceName;
                this.timeStamp = timeStamp;// DateTime.UtcNow.Ticks;
            }
            public string DataVersion => dataVersion;
            public string DeviceName => deviceName;
            public long TimeStamp { get => timeStamp; set => timeStamp = value; }
            public string AppVersion => appVersion;
        }

        [Serializable]
        public class DataInService
        {
            [SerializeField] EnvironmentInfo environment;
            [SerializeField] List<DataPair> data;

            Dictionary<string, string> dictData;
            public DataInService()
            {
                data = new List<DataPair>();
                environment = new EnvironmentInfo("1.0", 0);
            }
            public List<DataPair> Data          {   get => data; set => data = value;}
            public EnvironmentInfo Environment  {   get => environment; set => environment = value; }
            public void Init()
            {
                if(data == null)    return;

                dictData = new Dictionary<string, string>();
                for(int q = 0; q < data.Count; q++) 
                    dictData.Add(data[q].Key, data[q].Value);
            }
            public string GetData(string key)
            {
                if(dictData!=null && dictData.ContainsKey(key))
                    return dictData[key];
                
                return string.Empty;
            }
            public void Clear()
            {
                Data?.Clear();
                dictData?.Clear();
            }
        }
    }
}