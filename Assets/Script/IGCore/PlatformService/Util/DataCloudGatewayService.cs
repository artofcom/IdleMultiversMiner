using Core.Utils;
using IGCore.PlatformService.Cloud;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace IGCore.MVCS
{
    public class DataCloudGatewayService : DataGatewayService
    {
        ICloudService cloudService;

        public DataCloudGatewayService(ICloudService cloudService)
        {
            this.cloudService = cloudService;
        }

        public override async Task WriteData(string dataKey, bool clearAll)
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

            // Writing it in local.            
            string fileName = Application.persistentDataPath + "/" + dataKey + ".json";
            TextFileIO.WriteTextFile(fileName, jsonText);

            string errMessage = await cloudService.SaveUserData(dataKey, jsonText);
            if(false == string.IsNullOrEmpty(errMessage))
                Debug.Log($"<color=red>[CloudGateWay] : SaveData [{dataKey}] has been failed.... => {errMessage}</color>");
            else 
                Debug.Log($"<color=green>[CloudGateWay] : SaveData [{dataKey}] has been successed.</color>");
        }

        public override async Task ReadData(string dataKey)
        {
            string jsonString = await cloudService.LoadUserData(dataKey);
            if(string.IsNullOrEmpty(jsonString))
            {
                Debug.Log($"<color=red>[CloudGateWay] : ReadData [{dataKey}] has been failed. Try reading it in local.. </color>");
                
                string fileName = Application.persistentDataPath + "/" + dataKey + ".json";
                jsonString = TextFileIO.ReadTextFile(fileName);
                if(string.IsNullOrEmpty(jsonString))
                {
                    Debug.LogError($"[CloudGateWay] : ReadData [{dataKey}] in local has been failed !");
                    return;
                }
            }
            else 
                Debug.Log($"<color=green>[CloudGateWay] : ReadData [{dataKey}] has been successed</color>");

            serviceData = JsonUtility.FromJson<DataInService>(jsonString);
            if(serviceData == null)
                serviceData = new DataInService();

            serviceData.Init();
        }

        public override string GetData(string model_id)
        {
            if(serviceData == null)
                return string.Empty;

            return serviceData.GetData(model_id);
        }
    } 
}