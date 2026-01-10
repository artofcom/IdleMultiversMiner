using Core.Utils;
using IGCore.PlatformService.Cloud;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace IGCore.PlatformService
{
    public class DataGatewayService : ILocalDataGatewayService
    {
        public bool IsDirty { get; set; }
        public bool IsLocked { get; set; } = false;

        protected DataGateWay.DataInService serviceData = new DataGateWay.DataInService();
        protected List<IWritableModel> models = new List<IWritableModel>();

        public DataGateWay.DataInService ServiceData => serviceData;

        public void RegisterDataModel(IWritableModel model)
        {
            models.Add(model);
            Debug.Log($"[LocalGateWay] : Adding Model [{model.GetType().Name}].");
        }
        public void UnRegisterDataModel(IWritableModel model)
        {
            models.Remove(model);
            Debug.Log($"[LocalGateWay] : Removing Model [{model.GetType().Name}].");
        }
        public void ClearModels()
        {
            models.Clear();
            Debug.Log("[LocalGateWay] : Clearing All Models.");
        }

        public virtual Task<bool> WriteData(string accountId, string dataKey, bool clearAll)
        {
            try
            {
                if(IsLocked)
                {
                    Debug.Log("<color=red>[GateWay] : Serivce is Locked !!!</color>");
                    return Task.FromResult(false);
                }

                
                if(!IsDirty)        return Task.FromResult(false);
                IsDirty = false;

                Assert.IsTrue(!string.IsNullOrEmpty(accountId));

                if(serviceData == null)
                    serviceData = new DataGateWay.DataInService();
                else
                {
                    if (serviceData.Data == null)
                        serviceData.Data = new List<DataGateWay.DataPair>();
            
                    if(serviceData.Environment==null || serviceData.Environment.TimeStamp <= 0)
                        serviceData.Environment = new DataGateWay.EnvironmentInfo(DateTime.UtcNow.Ticks);
                }

                serviceData.Clear();
                serviceData.Environment.Update(DateTime.UtcNow.Ticks);

                if(!clearAll)
                {
                    Assert.IsTrue(models.Count > 0, "Model count should be greater than 0 !");
                    Assert.IsTrue(serviceData.Data.Count == 0);

                    // Poll Data from Models.
                    for(int k = 0; k < models.Count; ++k)
                    {
                        List<Tuple<string, string>> listDataSet  = models[k].GetSaveDataWithKeys();
                        if(listDataSet == null)
                            continue;

                        for(int q = 0; q < listDataSet.Count; q++)
                        {
                            serviceData.Data.Add(new DataGateWay.DataPair(listDataSet[q].Item1, listDataSet[q].Item2));
                            Debug.Log($"[LocalDataGateway] {listDataSet[q].Item1} data has been collected for writing.");
                        }
                    }
                    Debug.Log($"[LocalDataGateway] Total Data Size : {serviceData.Data.Count}");
                }
                serviceData.Init();
            
                string fullPath = Path.Combine(Application.persistentDataPath, accountId);
                //if (!Directory.Exists(fullPath))
                //    Directory.CreateDirectory(fullPath);

                string fileName = Path.Combine(fullPath, dataKey + ".json");
                string jsonText = JsonUtility.ToJson(serviceData, prettyPrint:true);
                return Task.FromResult( TextFileIO.WriteTextFile(fileName, jsonText) );
            }
            catch (Exception ex) 
            {
                Debug.LogWarning(ex.Message);
                return Task.FromResult(false);
            }
        }

        public virtual Task<bool> ReadData(string accountId, string dataKey)
        {
            serviceData = null;

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