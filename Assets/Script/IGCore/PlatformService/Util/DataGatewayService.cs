using Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;

namespace IGCore.MVCS
{
    public class DataGatewayService : IDataGatewayService
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

            public EnvironmentInfo(string version)
            {
                this.dataVersion = version;     
                this.appVersion = Application.version;    this.deviceName = SystemInfo.deviceName;
                this.timeStamp = DateTime.UtcNow.Ticks;
            }
            public string DataVersion => dataVersion;
            public string DeviceName => deviceName;
            public long TimeStamp => timeStamp;
            public string AppVersion => appVersion;
        }

        [Serializable]
        public class DataInService
        {
            [SerializeField] EnvironmentInfo environment;
            [SerializeField] List<DataPair> data;

            Dictionary<string, string> dictData;
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


        public string AccountId { get; set; } = string.Empty;

        protected DataInService serviceData = new DataInService();
        protected List<IWritableModel> models = new List<IWritableModel>();

        public DataInService ServiceData => serviceData;

        public void RegisterDataModel(IWritableModel model)
        {
            models.Add(model);
        }
        public void UnRegisterDataModel(IWritableModel model)
        {
            models.Remove(model);
        }
        public void ClearModels()
        {
            models.Clear();
        }

        public virtual async Task<bool> WriteData(string dataKey, bool clearAll)
        {
            UnityEngine.Assertions.Assert.IsTrue(!string.IsNullOrEmpty(AccountId));

            if(serviceData.Data == null)
                serviceData.Data = new List<DataPair>();
            
            if(serviceData.Environment==null || serviceData.Environment.TimeStamp <= 0)
                serviceData.Environment = new EnvironmentInfo("1.0");

            serviceData.Clear();

            if(!clearAll)
            {
                // Poll Data from Models.
                for(int k = 0; k < models.Count; ++k)
                {
                    List<Tuple<string, string>> listDataSet  = models[k].GetSaveDataWithKeys();
                    if(listDataSet == null)
                        continue;

                    for(int q = 0; q < listDataSet.Count; q++)
                        serviceData.Data.Add(new DataPair(listDataSet[q].Item1, listDataSet[q].Item2));
                }
            }
            
            string fullPath = Path.Combine(Application.persistentDataPath, AccountId);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            string fileName = Path.Combine(fullPath, dataKey + ".json");
            string jsonText = JsonUtility.ToJson(serviceData, prettyPrint:true);
            TextFileIO.WriteTextFile(fileName, jsonText);
            // Debug.Log($"Writing data...{filePath}, {jsonText}");
            return true;
        }

        public virtual async Task<bool> ReadData(string dataKey)
        {
            string fileNamePath = Path.Combine(Application.persistentDataPath, AccountId, dataKey + ".json");
            string jsonString = TextFileIO.ReadTextFile(fileNamePath);

            serviceData = JsonUtility.FromJson<DataInService>(jsonString);
            if(serviceData == null)
                serviceData = new DataInService();

            serviceData.Init();
            return true;
        }

        public virtual string GetData(string model_id)
        {
            if(serviceData == null)
                return string.Empty;

            return serviceData.GetData(model_id);
        }
    } 
}