using Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace IGCore.MVCS
{
    public abstract class MultiGatewayWritablePlayerModel : APlayerModel, IWritableModel
    {
        List<IDataGatewayService> gatewayServices;
        public MultiGatewayWritablePlayerModel(AContext ctx, List<IDataGatewayService> gatewayServices) : base(ctx)
        { 
            this.gatewayServices = new List<IDataGatewayService>( gatewayServices );
        }

        public abstract List<Tuple<string, string>> GetSaveDataWithKeys();

        protected void FetchData<T>(int idxGatewayService, string data_key, out T data, T fallback)
        {
            Assert.IsNotNull(gatewayServices);

            if(idxGatewayService<0 || idxGatewayService>=gatewayServices.Count)
            {
                Debug.Log($"<color=red>Invalid gateway serice index. {idxGatewayService} </color>");
                data = fallback;
                return;
            }

            IDataGatewayService gatewayService = gatewayServices[idxGatewayService];

            // Internal func.
            bool parseData<Q>(string textData, out Q parseData, Q fallback_q)
            {
                if(string.IsNullOrEmpty(textData))
                {
                    parseData = fallback_q;
                    return false;
                }

                if (typeof(Q) == typeof(string))
                    parseData = (Q)(object)textData;
                else if (typeof(Q) == typeof(int))
                {
                    int value;
                    bool ret = int.TryParse(textData, out value);
                    Assert.IsTrue(ret);
                    parseData = (Q)(object)value;
                }
                else if (typeof(Q) == typeof(float))
                {
                    float value;
                    bool ret = float.TryParse(textData, out value);
                    Assert.IsTrue(ret);
                    parseData = (Q)(object)value;
                }
                else if (typeof(Q)==typeof(double) || typeof(Q)==typeof(long))
                {
                    parseData = (Q)(object)0; 
                }
                else if (typeof(Q) == typeof(bool))
                {
                    parseData = (Q)(object)((bool) (textData=="1" || textData.ToLower()=="true"));
                }
                else
                { 
                    parseData = JsonUtility.FromJson<Q>(textData);
                }
                return true;
            }

            string jsonString = gatewayService.GetData(data_key);
            parseData<T>(jsonString, out data, fallback);
        }

        public override void Init()
        {
            Assert.IsNotNull(gatewayServices);
            for(int q = 0; q < gatewayServices.Count; q++) 
                gatewayServices[q].RegisterDataModel(this);
        }

        public override void Dispose()
        {
            base.Dispose();

            for(int q = 0; q < gatewayServices.Count; q++) 
                gatewayServices[q].UnRegisterDataModel(this);
        }

        public void SetDirty()
        {
            for(int q = 0; q < gatewayServices.Count; q++) 
                gatewayServices[q].IsDirty = true;
        }
    }
}
