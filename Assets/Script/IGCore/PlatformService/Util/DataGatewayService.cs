using Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

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
        public class DataInService
        {
            [SerializeField] List<DataPair> data;

            Dictionary<string, string> dictData;
            public List<DataPair> Data
            {
                get => data; set => data = value;
            }
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

        protected DataInService serviceData = new DataInService();
        protected List<IWritableModel> models = new List<IWritableModel>();

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

        public virtual async Task WriteData(string dataKey, bool clearAll)
        {
            if(serviceData.Data == null)
                serviceData.Data = new List<DataPair>();

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
            
            string fileName = Application.persistentDataPath + "/" + dataKey + ".json";
            string jsonText = JsonUtility.ToJson(serviceData, prettyPrint:true);
            TextFileIO.WriteTextFile(fileName, jsonText);
            // Debug.Log($"Writing data...{filePath}, {jsonText}");
        }

        public virtual async Task ReadData(string dataKey)
        {
            string fileName = Application.persistentDataPath + "/" + dataKey + ".json";
            string jsonString = TextFileIO.ReadTextFile(fileName);

            serviceData = JsonUtility.FromJson<DataInService>(jsonString);
            if(serviceData == null)
                serviceData = new DataInService();

            serviceData.Init();
        }

        public virtual string GetData(string model_id)
        {
            if(serviceData == null)
                return string.Empty;

            return serviceData.GetData(model_id);
        }
    } 
}