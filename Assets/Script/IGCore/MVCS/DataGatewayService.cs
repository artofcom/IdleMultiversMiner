using Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        DataInService serviceData = new DataInService();
        List<IWritableModel> models = new List<IWritableModel>();

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

        public void WriteData(string filePath, bool clearAll)
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
            
            string jsonText = JsonUtility.ToJson(serviceData, prettyPrint:true);
            TextFileIO.WriteTextFile(filePath, jsonText);
            // Debug.Log($"Writing data...{filePath}, {jsonText}");
        }

        public void ReadData(string filePath)
        {
            string jsonString = TextFileIO.ReadTextFile(filePath);

            serviceData = JsonUtility.FromJson<DataInService>(jsonString);
            if(serviceData == null)
                serviceData = new DataInService();

            serviceData.Init();

            //foreach (var model_id in models.Keys)
            //{
            //    models[model_id].LoadData( FetchData(model_id) ); 
            //}
        }

        public string GetData(string model_id)
        {
            if(serviceData == null)
                return string.Empty;

            return serviceData.GetData(model_id);
        }
    } 
}