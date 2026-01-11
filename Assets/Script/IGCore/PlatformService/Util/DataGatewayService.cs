using Core.Utils;
using IGCore.PlatformService.Cloud;
using NUnit.Framework.Internal.Filters;
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
        public long TimeStamp => serviceData!=null ? serviceData.TimeStamp : 0;

        public void RegisterDataModel(IWritableModel model)
        {
            models.Add(model);
            //Debug.Log($"[LocalGateWay] : Adding Model [{model.GetType().Name}].");
        }
        public void UnRegisterDataModel(IWritableModel model)
        {
            models.Remove(model);
            //Debug.Log($"[LocalGateWay] : Removing Model [{model.GetType().Name}].");
        }
        public void ClearModels()
        {
            models.Clear();
            //Debug.Log("[LocalGateWay] : Clearing All Models.");
        }

        public virtual Task<bool> WriteData(string accountId, string dataKey, bool clearAll)
        {
            try
            {
                if(IsLocked || !IsDirty || models==null || models.Count==0 || string.IsNullOrEmpty(accountId))
                {
                    Debug.Log("<color=orange>[GateWay] : Serivce is Not Ready for Write...</color>");
                    return Task.FromResult(false);
                }

                IsDirty = false;

                if(serviceData == null)
                    serviceData = new DataGateWay.DataInService();
                
                serviceData.ReadyForWrite(DateTime.UtcNow.Ticks);

                if(clearAll)
                    serviceData.Clear();
                else
                {
                    // Poll Data from Models.
                    for (int k = 0; k < models.Count; ++k)
                    {
                        List<Tuple<string, string>> listDataSet  = models[k].GetSaveDataWithKeys();
                        if (listDataSet == null)
                            continue;

                        for (int q = 0; q < listDataSet.Count; q++)
                        {
                            serviceData.UpdateData(new DataGateWay.DataPair(listDataSet[q].Item1, listDataSet[q].Item2));
                            //Debug.Log($"[LocalDataGateway] {listDataSet[q].Item1} data has been collected for writing.");
                        }
                    }
                }
                serviceData.InitBuffer();
            
                string fullPath = Path.Combine(Application.persistentDataPath, accountId);
                //if (!Directory.Exists(fullPath))
                //    Directory.CreateDirectory(fullPath);

                string fileName = Path.Combine(fullPath, dataKey + ".json");
                string jsonText = JsonUtility.ToJson(serviceData, prettyPrint:true);
                Debug.Log($"<color=yellow>[LocalDataGateway][Writing] : {fileName} \nTotal Data Size : {serviceData.DataCount} \n{jsonText}</color>");
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

                Debug.Log($"<color=green>[LocalDataGateWay][Reading] : {fileNamePath} \n{jsonString} </color>");

                serviceData.InitBuffer();
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