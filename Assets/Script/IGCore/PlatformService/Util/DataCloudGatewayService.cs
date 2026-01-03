using Core.Utils;
using IGCore.PlatformService.Cloud;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace IGCore.MVCS
{
    public class DataCloudGatewayService : DataGatewayService
    {
        protected ICloudService cloudService;

        public DataCloudGatewayService(ICloudService cloudService)
        {
            this.cloudService = cloudService;
        }

        public override async Task<bool> WriteData(string dataKey, bool clearAll)
        {
            if(!IsDirty)        return false;

            IsDirty = false;

            Assert.IsNotNull(cloudService);

            if(serviceData.Data == null)
                serviceData.Data = new List<DataPair>();

            if(serviceData.Environment==null || serviceData.Environment.TimeStamp <= 0)
                serviceData.Environment = new EnvironmentInfo("1.0", DateTime.UtcNow.Ticks);

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

            Tuple<bool, string> cloudResult = await cloudService.SaveUserData(dataKey, jsonText);
            if(false == cloudResult.Item1)
                Debug.Log($"<color=red>[CloudGateWay] : SaveData [{dataKey}] has been failed.., {cloudResult.Item2}</color>");
            else 
                Debug.Log($"<color=green>[CloudGateWay] : SaveData [{dataKey}] has been successed.</color>");

            return cloudResult.Item1;
        }

        public override async Task<bool> ReadData(string dataKey)
        {   
            serviceData = null;

            Tuple<bool, string> cloudResult = await cloudService.LoadUserData(dataKey);
            if(false == cloudResult.Item1)
            {
                Debug.Log($"<color=red>[CloudGateWay] : ReadData data from Cloud has been failed. [{dataKey}] </color>");
                return false;
            }
            else
            {
                Debug.Log($"<color=green>[CloudGateWay] : ReadData data from Cloud has been done. - [{dataKey}]</color>");
                Debug.Log($"<color=green>[CloudGateWay] : [{cloudResult.Item2}]</color>");
                serviceData = JsonUtility.FromJson<DataInService>(cloudResult.Item2);
            }            
            
            if(serviceData == null)
                serviceData = new DataInService();

            serviceData.Init();
            return true;
        }
    } 
}