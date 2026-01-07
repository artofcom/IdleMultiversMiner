using Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;

namespace IGCore.PlatformService
{
    public class DataGatewayService : ILocalDataGatewayService
    {
        public bool IsDirty { get; set; }

        protected DataGateWay.DataInService serviceData = new DataGateWay.DataInService();
        protected List<IWritableModel> models = new List<IWritableModel>();

        public DataGateWay.DataInService ServiceData => serviceData;

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

        public virtual Task<bool> WriteData(string accountId, string dataKey, bool clearAll)
        {
            try
            {
                if(!IsDirty)        return Task.FromResult(false);

                IsDirty = false;
                UnityEngine.Assertions.Assert.IsTrue(!string.IsNullOrEmpty(accountId));

                if(serviceData.Data == null)
                    serviceData.Data = new List<DataGateWay.DataPair>();
            
                if(serviceData.Environment==null || serviceData.Environment.TimeStamp <= 0)
                    serviceData.Environment = new DataGateWay.EnvironmentInfo("1.0", DateTime.UtcNow.Ticks);

                serviceData.Environment.TimeStamp = DateTime.UtcNow.Ticks;
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
                            serviceData.Data.Add(new DataGateWay.DataPair(listDataSet[q].Item1, listDataSet[q].Item2));
                    }
                }
            
                string fullPath = Path.Combine(Application.persistentDataPath, accountId);
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                string fileName = Path.Combine(fullPath, dataKey + ".json");
                string jsonText = JsonUtility.ToJson(serviceData, prettyPrint:true);
                TextFileIO.WriteTextFile(fileName, jsonText);
                // Debug.Log($"Writing data...{filePath}, {jsonText}");
                return Task.FromResult(true);
            }
            catch (Exception ex) 
            {
                Debug.LogWarning(ex.Message);
                return Task.FromResult(false);
            }
        }

        public virtual Task<bool> ReadData(string accountId, string dataKey)
        {
            try
            {
                string fileNamePath = Path.Combine(Application.persistentDataPath, accountId, dataKey + ".json");
                string jsonString = TextFileIO.ReadTextFile(fileNamePath);

                serviceData = JsonUtility.FromJson<DataGateWay.DataInService>(jsonString);
                if(serviceData == null)
                    return Task.FromResult(false);  //serviceData = new DataInService();

                serviceData.Init();
                return Task.FromResult(true);
            }
            catch (Exception ex) 
            {
                Debug.LogWarning(ex.Message);
                return Task.FromResult(false);
            }
        }

        public virtual string GetData(string model_id)
        {
            if(serviceData == null)
                return string.Empty;

            return serviceData.GetData(model_id);
        }
    } 
}