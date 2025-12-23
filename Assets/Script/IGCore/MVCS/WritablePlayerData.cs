using UnityEngine;
using System;
using UnityEngine.Assertions;
using Core.Utils;

namespace IGCore.MVCS
{
    public class WritablePlayerModel : APlayerModel
    {
        public WritablePlayerModel(AContext ctx) : base(ctx) { }

        protected static void WriteFileInternal(string fileName, object objData, bool convertToJson=true)
        {
            if(string.IsNullOrEmpty(fileName) || objData == null)
                return;

            string content = convertToJson ? JsonUtility.ToJson(objData, prettyPrint:true) : objData.ToString();
            TextFileIO.WriteTextFile(Application.persistentDataPath + "/" + fileName + ".json", content);
        }

        protected static void ReadFileInternal<T>(string fileName, out T data, T fallback)
        {
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

            string strData = TextFileIO.ReadTextFile(Application.persistentDataPath + "/" + fileName + ".json");
            parseData<T>(strData, out data, fallback);
        }

        public override void Init() {}
    }
}
