using Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace IGCore.MVCS
{
    public abstract class GatewayWritablePlayerModel : APlayerModel, IWritableModel
    {
        IDataGatewayService gatewayService;
        public GatewayWritablePlayerModel(AContext ctx, IDataGatewayService gatewayService) : base(ctx)
        { 
            this.gatewayService = gatewayService;
        }

        public abstract List<Tuple<string, string>> GetSaveDataWithKeys();

        protected void FetchData<T>(string data_key, out T data, T fallback)
        {
            Assert.IsNotNull(gatewayService);

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
            Assert.IsNotNull(gatewayService);
            gatewayService.RegisterDataModel(this);
        }

        public override void Dispose()
        {
            base.Dispose();
            gatewayService.UnRegisterDataModel(this);
        }
    }
}
