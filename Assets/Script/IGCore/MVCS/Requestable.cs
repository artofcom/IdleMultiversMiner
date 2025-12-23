using System.Collections.Generic;
using System;

namespace IGCore.MVCS
{
    public class Requestable
    {
        public delegate object RequestDelegate(params object[] param);
    
        protected Dictionary<string, RequestDelegate> dictRequest = new Dictionary<string, RequestDelegate>();

        public void AddRequestDelegate(string path, RequestDelegate queryDelegate)
        {
            path = path.ToLower();

            if(dictRequest.ContainsKey(path))
                dictRequest.Remove(path);

            dictRequest.Add(path, queryDelegate);
        }
        
        public void RequestQuery(string path, Action<string, object> finishCallback, params object[] param)
        {
            path = path.ToLower();

            string errorMsg = string.Empty;
            object ret = default(object);
            if(dictRequest.ContainsKey(path))                
                ret = dictRequest[path].Invoke(param);
            else
                errorMsg = $"[Requestables] : Counldn't find [{path}].";


            finishCallback(errorMsg, ret);
        }

        public void RemoveRequestDelegate(string path)
        {
            path = path.ToLower();

            if(dictRequest.ContainsKey(path))
                dictRequest.Remove(path);
        }
    }
}
