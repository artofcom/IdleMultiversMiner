using Core.Utils;
using IGCore.PlatformService.Cloud;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace IGCore.PlatformService
{
    public class DataCloudGatewayService : ICloudDataGatewayService
    {
        protected ICloudService cloudService;

        public bool IsDirty { get; set; }
        public bool IsLocked { get; set; } = false;

        protected DataGateWay.DataInService serviceData = new DataGateWay.DataInService();
        protected List<IWritableModel> models = new List<IWritableModel>();

        public DataGateWay.DataInService ServiceData => serviceData;

        public void RegisterDataModel(IWritableModel model)
        {
            models.Add(model);
            //Debug.Log($"[CloudGateWay] : Adding Model [{model.GetType().Name}].");
        }
        public void UnRegisterDataModel(IWritableModel model)
        {
            models.Remove(model);
            //Debug.Log($"[CloudGateWay] : Removing Model [{model.GetType().Name}].");
        }
        public void ClearModels()
        {
            models.Clear();
            //Debug.Log("[CloudGateWay] : Clearing All Models.");
        }

        public DataCloudGatewayService(ICloudService cloudService)
        {
            this.cloudService = cloudService;
        }

        public async Task<ICloudService.ResultType> WriteData(string dataKey, bool clearAll)
        {
            try
            {
                if(!IsDirty)        return ICloudService.ResultType.eNoneToUpdate;
                IsDirty = false;

                if(IsLocked)
                {
                    Debug.Log("<color=red>[CloudGateWay] : Serivce is Locked !!!</color>");
                    return ICloudService.ResultType.eLocked;
                }

                Assert.IsNotNull(cloudService);

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
                            //Debug.Log($"[CloudDataGateway] {listDataSet[q].Item1} data has been collected for writing.");
                        }
                    }
                    Debug.Log($"[CloudDataGateway] Total Data Size : {serviceData.Data.Count}");
                }
            
                string jsonText = JsonUtility.ToJson(serviceData, prettyPrint:true);

                Tuple<ICloudService.ResultType, string> cloudResult = await cloudService.SaveUserData(dataKey, jsonText);
                if(cloudResult.Item1 != ICloudService.ResultType.eSuccessed)
                    Debug.Log($"<color=red>[CloudGateWay] : SaveData [{dataKey}] has been failed.., {cloudResult.Item2}</color>");
                else 
                    Debug.Log($"<color=green>[CloudGateWay] : SaveData [{dataKey}] has been successed.</color>");

                return cloudResult.Item1;
            }
            catch (Exception ex) 
            {
                Debug.LogWarning(ex.Message);
                return ICloudService.ResultType.eUnknownError;
            }

        }

        public async Task<ICloudService.ResultType> ReadData(string dataKey)
        {   
            serviceData = null;

            try
            {
                Tuple<ICloudService.ResultType, string> cloudResult = await cloudService.LoadUserData(dataKey);
                if(ICloudService.ResultType.eSuccessed != cloudResult.Item1)
                {
                    Debug.Log($"<color=red>[CloudGateWay] : ReadData data from Cloud has been failed. [{dataKey}] </color>");
                    return cloudResult.Item1;
                }
            
                Debug.Log($"<color=green>[CloudGateWay] : ReadData data from Cloud has been done. - [{dataKey}]</color>");
                Debug.Log($"<color=green>[CloudGateWay] : [{cloudResult.Item2}]</color>");
                serviceData = JsonUtility.FromJson<DataGateWay.DataInService>(cloudResult.Item2);
            
                if(serviceData == null)
                    return ICloudService.ResultType.eUnknownError; 

                serviceData.Init();
                return ICloudService.ResultType.eSuccessed;
            }
            catch (Exception ex) 
            {
                Debug.LogWarning(ex.Message);
                return ICloudService.ResultType.eUnknownError;
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