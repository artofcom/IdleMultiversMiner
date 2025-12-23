//////////////////////////////////////////////////////////
//
//////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine.Networking;
using System;
using System.Text;
using System.IO;

namespace Core.WWW
{
    public class WWWTextGet : MonoBehaviour
    {
        #region Constants
        public const float REQUEST_EXPIRATION_SEC = 3.0f;
        public const float REQUEST_CHECK_UPDATE_SEC = 0.2f;
        #endregion

        #region Static
        public static WWWTextGet instance;

        private static void CheckInstance()
        {
            if (WWWTextGet.instance == null)
            {
                GameObject go = new GameObject("Runtime+WWWTextGet");
                WWWTextGet.instance = go.AddComponent<WWWTextGet>();
            }
        }

        //TODO: Make these functions an interface, instead of static

        // Cancel a request that might be coming up in the future
        public static void CancelRequestForTextURL(string url)
        {
            WWWTextGet.instance.requestQueue.RemoveRequestFor(url);
        }

        // Get or schedule to get an image
        public static void GetDataForTextURL(string url, TextDataCallback callback, TextDataErrorCallback errorCallback)
        {
            WWWTextGet.CheckInstance();

            string cachedData = WWWTextGet.instance.DataForCachedText(url);

            if (!string.IsNullOrEmpty(cachedData))
            {
                callback(cachedData, url);
                Debug.Log("Get Text Data From Cache.... " + url);
            }
            else
            {
                TextRequest textReq = WWWTextGet.instance.requestQueue.FindRequest(url);

                if (textReq != null)
                {
                    textReq.successEvent += callback;
                    textReq.errorEvent += errorCallback;
                    WWWTextGet.instance.CheckRequestQueueAndCall();
                    Debug.Log("Get Text Data From URL. - Found duplicated req.. " + url);
                }
                else
                {
                    TextRequest reqObj = new TextRequest();
                    reqObj.url = url;
                    reqObj.successEvent += callback;
                    reqObj.errorEvent += errorCallback;

                    WWWTextGet.instance.requestQueue.AddRequestToQueue(reqObj);
                    WWWTextGet.instance.CheckRequestQueueAndCall();
                    Debug.Log("Get Text Data From URL. - Scheduled to request... " + url);
                }
            }
        }
        public static void GetDataForTextURLExt(string url, TextDataCallback callback, TextDataErrorCallback errorCallback,
                    bool useCache = true)
        {
            WWWTextGet.CheckInstance();

            string cachedData = useCache ? WWWTextGet.instance.DataForCachedText(url) : "";

            if (!string.IsNullOrEmpty(cachedData))
            {
                callback(cachedData, url);
                Debug.Log("Get Text Data From Cache.... " + url);
            }
            else
            {
                TextRequest textReq = WWWTextGet.instance.requestQueue.FindRequest(url);

                if (textReq != null)
                {
                    textReq.successEvent += callback;
                    textReq.errorEvent += errorCallback;
                    WWWTextGet.instance.CheckRequestQueueAndCall();
                    Debug.Log("Get Text Data From URL. - Found duplicated req.. " + url);
                }
                else
                {
                    TextRequest reqObj = new TextRequest();
                    reqObj.url = url;
                    reqObj.successEvent += callback;
                    reqObj.errorEvent += errorCallback;

                    WWWTextGet.instance.requestQueue.AddRequestToQueue(reqObj);
                    WWWTextGet.instance.CheckRequestQueueAndCall();
                    Debug.Log("Get Text Data From URL. - Scheduled to request... " + url);
                }
            }
        }
        public static void SetOfflineMode(bool offline)
        {
            WWWTextGet.CheckInstance();
            WWWTextGet.instance.OFFLINE_MODE = offline;
        }
        #endregion

        #region Instance Properties and Fields
        public delegate void TextDataCallback(string loadedText, string textUrl);
        public delegate void TextDataErrorCallback(string textUrl, string error);

        public TextRequestQueue requestQueue = new TextRequestQueue();

        public bool OFFLINE_MODE { set; private get; } = false;
        public bool requestInProgress = false;
        private SHA1CryptoServiceProvider sha1Service;
        #endregion

        #region Behaviour Overrides (Intialization)
        private void Awake()
        {
            sha1Service = new SHA1CryptoServiceProvider();
        }
        #endregion

        #region Helpers - Actual Web Requests!
        private void CheckRequestQueueAndCall()
        {
            if (OFFLINE_MODE == false && (
                Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork ||   // 3G
                Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork))      // WiFi 
            {
                if (this.requestQueue.NumberOfRequestsInQueue() > 0 && !this.requestInProgress)
                {
                    TextRequest reqObj = this.requestQueue.GetAPendingRequest();
                    if (reqObj != null)
                    {
                        // saftey first
                        this.requestInProgress = true;
                        StartCoroutine(this.SendRequest(reqObj));
                    }
                }
            }
            else
            {
                Debug.LogWarning("Unable to reach the internet. Queueing transaction.");
                // StartCoroutine(WaitForIntervalAndTryAgain());

                // Flush all queued req.
                TextRequest reqObj = this.requestQueue.GetAPendingRequest();
                while (reqObj != null)
                {
                    reqObj.CallErrorEvent("Network Unreachable!");
                    reqObj = this.requestQueue.GetAPendingRequest();
                }
                this.requestInProgress = false;
            }
        }

        private IEnumerator SendRequest(TextRequest textRequest)
        {
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>();
            using (UnityWebRequest webRequest = UnityWebRequest.Get(textRequest.url))
            {
                var requestRoutine = webRequest.SendWebRequest();

                var url = textRequest.url;
                var st = Time.realtimeSinceStartup;
                while (requestRoutine != null && !requestRoutine.isDone)
                {
                    // Debug.Log($"[Text Get TRANSACTION WAITING]: {url} + {Time.realtimeSinceStartup - st:#.##}");
                    if (Time.realtimeSinceStartup - st > REQUEST_EXPIRATION_SEC)
                    {
                        textRequest.CallErrorEvent("Request Time has been expried!");
                        this.requestInProgress = false;
                        this.CheckRequestQueueAndCall();
                        yield break;
                    }
                    yield return new WaitForSeconds(REQUEST_CHECK_UPDATE_SEC);
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    if (string.IsNullOrEmpty(webRequest.error))
                    {
                        textRequest.CallErrorEvent(webRequest.error);
                    }
                    else
                    {
                        string error_str = webRequest.result == UnityWebRequest.Result.ConnectionError ? "Connection Error" : "File Not Found";
                        textRequest.CallErrorEvent($"Image Request failed -- {error_str}");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    {

                        Debug.Log($"<color=#EF7070>>>> Downloaded Text Data from URL : {textRequest.url}....</color>");

                        CacheTextData(textRequest.url, webRequest.downloadHandler.text);
                        textRequest.CallSuccessEvent(webRequest.downloadHandler.text);
                    }
                    else
                    {
                        textRequest.CallErrorEvent($"Image Request failed -- DownloadHandlerTexture failed to return a valid texture");
                    }
                }
            }

            this.requestInProgress = false;
            this.CheckRequestQueueAndCall();
            yield return null;
        }
        #endregion

        #region Caching Logic
        private void CacheTextData(string url, string textData)
        {
            byte[] urlToEncode = Encoding.UTF8.GetBytes(url);
            string encodedKey = System.BitConverter.ToString(sha1Service.ComputeHash(urlToEncode)).Replace("-", string.Empty);
            string cacheFileName = $"{Application.persistentDataPath}/TextCache/{encodedKey}";

            try
            {
                if (!Directory.Exists($"{Application.persistentDataPath}/TextCache/"))
                {
                    Directory.CreateDirectory($"{Application.persistentDataPath}/TextCache/");
                }

                //File.WriteAllBytes(cacheFileName, textureData);
                File.WriteAllText(cacheFileName, textData);
                PlayerPrefs.SetString($"{encodedKey}_FilePath", cacheFileName);
                PlayerPrefs.SetString($"{encodedKey}_CacheDate", DateTimeOffset.UtcNow.ToString("o"));
            }
            catch
            {
                Debug.LogWarning($"Failed to Cache Text: {url}");
            }
        }

        private string DataForCachedText(string url)
        {
            string textData = "";

            if (!string.IsNullOrEmpty(url))
            {
                byte[] urlToEncode = Encoding.UTF8.GetBytes(url);
                string encodedKey = System.BitConverter.ToString(sha1Service.ComputeHash(urlToEncode)).Replace("-", string.Empty);
                string cachedFilePath = PlayerPrefs.GetString($"{encodedKey}_FilePath", string.Empty);
                string cacheRefreshDate = PlayerPrefs.GetString($"{encodedKey}_CacheDate", string.Empty);

                if (!string.IsNullOrEmpty(cachedFilePath) && !string.IsNullOrEmpty(cacheRefreshDate))
                {
                    DateTimeOffset lastCacheDate = default(DateTimeOffset);

                    if (DateTimeOffset.TryParse(cacheRefreshDate, out lastCacheDate))
                    {
                        double timeSinceCache = (DateTimeOffset.UtcNow - lastCacheDate).TotalSeconds;
                        if (true)   // timeSinceCache <= VALID_CACHE_SEC)
                        {
                            try
                            {
                                textData = File.ReadAllText(cachedFilePath);
                            }
                            catch
                            {
                                Debug.LogWarning($"Failed to Load Text Cache: {url}");
                            }
                        }
                    }
                }
            }

            return textData;
        }

        public void CleanupCache()
        {
            string cacheDirectory = $"{Application.persistentDataPath}/TextCache/";
            DirectoryInfo dir = new DirectoryInfo(cacheDirectory);
            FileInfo[] info = dir.GetFiles("*.*");

            foreach (FileInfo f in info)
            {
                string encodedKey = f.Name;
                string cachedFilePath = PlayerPrefs.GetString($"{encodedKey}_FilePath", string.Empty);
                string cacheRefreshDate = PlayerPrefs.GetString($"{encodedKey}_CacheDate", string.Empty);

                if (!string.IsNullOrEmpty(cachedFilePath) && !string.IsNullOrEmpty(cacheRefreshDate))
                {
                    DateTimeOffset lastCacheDate = default(DateTimeOffset);

                    if (DateTimeOffset.TryParse(cacheRefreshDate, out lastCacheDate))
                    {
                        double timeSinceCache = (DateTimeOffset.UtcNow - lastCacheDate).TotalSeconds;
                        if (timeSinceCache >= 0f)// VALID_CACHE_SEC)
                        {
                            try
                            {
                                File.Delete(f.FullName);
                                PlayerPrefs.DeleteKey($"{encodedKey}_FilePath");
                                PlayerPrefs.DeleteKey($"{encodedKey}_CacheDate");
                            }
                            catch
                            {
                                Debug.LogWarning($"Failed to Remove Text Cache: {encodedKey}");
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Supporting Classes
        public class TextRequest
        {
            public string url;
            public event TextDataCallback successEvent;
            public event TextDataErrorCallback errorEvent;

            public void CallSuccessEvent(string loadedTex)
            {
                if (successEvent != null)
                {
                    successEvent(loadedTex, url);
                }
            }

            public void CallErrorEvent(string error)
            {
                if (errorEvent != null)
                {
                    errorEvent(url, error);
                }
            }
        }

        public class TextRequestQueue
        {
            List<TextRequest> requests = new List<TextRequest>();

            public void AddRequestToQueue(TextRequest requestObj)
            {
                requests.Add(requestObj);
            }

            /**
             * Remove the request for the passed in url
             * 
             * @return - true if successfully removed, false if a matching request couldn't be found
             */
            public bool RemoveRequestFor(string url)
            {
                foreach (TextRequest request in this.requests)
                {
                    if (request.url == url)
                    {
                        this.requests.Remove(request);
                        return true;
                    }
                }

                return false;
            }

            /**
             * Check for if they queue already contains a request for this img url that way we don't duplicate
             */
            public TextRequest FindRequest(string url)
            {
                foreach (TextRequest request in this.requests)
                {
                    if (request.url == url)
                        return request;
                }
                return null;
            }

            public int NumberOfRequestsInQueue()
            {
                return requests.Count;
            }

            public TextRequest GetAPendingRequest()
            {
                if (NumberOfRequestsInQueue() > 0)
                {
                    TextRequest reqObj = requests[0] as TextRequest;
                    requests.RemoveAt(0);
                    return reqObj;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion
    }
}
