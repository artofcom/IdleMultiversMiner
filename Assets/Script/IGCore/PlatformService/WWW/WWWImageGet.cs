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

    public class WWWImageGet : MonoBehaviour
    {
        #region Constants
        public const float VALID_CACHE_SEC = 60f * 60f * 24f * 3f; // 60 seconds, 60 minutes, 24 hours, 3 days
        public const float REQUEST_EXPIRATION_SEC = 3.0f;
        public const float REQUEST_CHECK_UPDATE_SEC = 0.2f;
        #endregion

        #region Static
        public static WWWImageGet instance;

        private static void CheckInstance()
        {
            if (WWWImageGet.instance == null)
            {
                GameObject go = new GameObject("Runtime+WWWImageGet");
                WWWImageGet.instance = go.AddComponent<WWWImageGet>();
            }
        }

        //TODO: Make these functions an interface, instead of static

        // Cancel a request that might be coming up in the future
        public static void CancelRequestForImageURL(string url)
        {
            WWWImageGet.instance.requestQueue.RemoveRequestFor(url);
        }

        // Get or schedule to get an image
        public static void GetDataForImageURL(string url, ImageDataCallback callback, ImageDataErrorCallback errorCallback, bool AddToViewDependentList = false)
        {
            WWWImageGet.CheckInstance();

            byte[] cachedData = WWWImageGet.instance.DataForCachedImage(url);

            if (cachedData != null && cachedData.Length > 0)
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(cachedData, false);
                tex.Apply(false, false);
                callback(tex, url);

                Debug.Log("Get Image Data From Cache.... " + url);
            }
            else
            {
                ImageRequest imgReq = WWWImageGet.instance.requestQueue.FindRequest(url);

                if (imgReq != null)
                {
                    imgReq.successEvent += callback;
                    imgReq.errorEvent += errorCallback;
                    WWWImageGet.instance.CheckRequestQueueAndCall();
                    Debug.Log("Get Image Data From URL. - Found duplicated req.. " + url);
                }
                else
                {
                    ImageRequest reqObj = new ImageRequest();
                    reqObj.url = url;
                    reqObj.successEvent += callback;
                    reqObj.errorEvent += errorCallback;

                    WWWImageGet.instance.requestQueue.AddRequestToQueue(reqObj);
                    WWWImageGet.instance.CheckRequestQueueAndCall();
                    Debug.Log("Get Image Data From URL. - Scheduled to request... " + url);
                }
            }
        }
        public static void SetOfflineMode(bool offline)
        {
            WWWImageGet.CheckInstance();
            WWWImageGet.instance.OFFLINE_MODE = offline;
        }
        #endregion

        #region Instance Properties and Fields
        public delegate void ImageDataCallback(Texture2D loadedTexture, string imageUrl);
        public delegate void ImageDataErrorCallback(string imageUrl, string error);

        public ImageRequestQueue requestQueue = new ImageRequestQueue();

        public bool OFFLINE_MODE { set; private get; } = false;
        public bool requestInProgress = false;
        private SHA1CryptoServiceProvider sha1Service;
        #endregion

        #region Behaviour Overrides (Intialization)
        private void Awake()
        {
            sha1Service = new SHA1CryptoServiceProvider();
            // CleanupCache();
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
                    ImageRequest reqObj = this.requestQueue.GetAPendingRequest();
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
                ImageRequest reqObj = this.requestQueue.GetAPendingRequest();
                while (reqObj != null)
                {
                    reqObj.CallErrorEvent("Network Unreachable!");
                    reqObj = this.requestQueue.GetAPendingRequest();
                }
                this.requestInProgress = false;
            }
        }

        private IEnumerator SendRequest(ImageRequest imageRequest)
        {
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>();
            byte[] textureData = null;

            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageRequest.url))
            {
                var requestRoutine = webRequest.SendWebRequest();

                var url = imageRequest.url;
                var st = Time.realtimeSinceStartup;
                while (requestRoutine != null && !requestRoutine.isDone)
                {
                    // Debug.Log($"[Image Get TRANSACTION WAITING]: {url} + {Time.realtimeSinceStartup - st:#.##}");
                    if (Time.realtimeSinceStartup - st > REQUEST_EXPIRATION_SEC)
                    {
                        imageRequest.CallErrorEvent("Request Time has been expried!");
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
                        imageRequest.CallErrorEvent(webRequest.error);
                    }
                    else
                    {
                        string error_str = webRequest.result == UnityWebRequest.Result.ConnectionError ? "Connection Error" : "File Not Found";
                        imageRequest.CallErrorEvent($"Image Request failed -- {error_str}");
                    }
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                    if (texture != null)
                    {
                        Debug.Log($"<color=#56F8C4>>>> Downloaded Image Data from URL : {imageRequest.url}....</color>");

                        texture.Apply();
                        textureData = webRequest.downloadHandler.data;
                        if (textureData != null)
                        {
                            CacheImageData(imageRequest.url, textureData);
                        }

                        imageRequest.CallSuccessEvent(texture);
                    }
                    else
                    {
                        imageRequest.CallErrorEvent($"Image Request failed -- DownloadHandlerTexture failed to return a valid texture");
                    }
                }
            }

            this.requestInProgress = false;
            this.CheckRequestQueueAndCall();
            yield return null;
        }
        #endregion

        #region Caching Logic
        private void CacheImageData(string url, byte[] textureData)
        {
            byte[] urlToEncode = Encoding.UTF8.GetBytes(url);
            string encodedKey = System.BitConverter.ToString(sha1Service.ComputeHash(urlToEncode)).Replace("-", string.Empty);
            string cacheFileName = $"{Application.persistentDataPath}/ImageCache/{encodedKey}";

            try
            {
                if (!Directory.Exists($"{Application.persistentDataPath}/ImageCache/"))
                {
                    Directory.CreateDirectory($"{Application.persistentDataPath}/ImageCache/");
                }

                File.WriteAllBytes(cacheFileName, textureData);

                PlayerPrefs.SetString($"{encodedKey}_FilePath", cacheFileName);
                PlayerPrefs.SetString($"{encodedKey}_CacheDate", DateTimeOffset.UtcNow.ToString("o"));
            }
            catch
            {
                Debug.LogWarning($"Failed to Cache Image: {url}");
            }
        }

        private byte[] DataForCachedImage(string url)
        {
            byte[] textureData = null;

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
                                textureData = File.ReadAllBytes(cachedFilePath);
                            }
                            catch
                            {
                                Debug.LogWarning($"Failed to Load Image Cache: {url}");
                            }
                        }
                    }
                }
            }

            return textureData;
        }

        public void CleanupCache()
        {
            string cacheDirectory = $"{Application.persistentDataPath}/ImageCache/";
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
                                Debug.LogWarning($"Failed to Remove Image Cache: {encodedKey}");
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Supporting Classes
        public class ImageRequest
        {
            public string url;
            public event ImageDataCallback successEvent;
            public event ImageDataErrorCallback errorEvent;

            public void CallSuccessEvent(Texture2D loadedTexture)
            {
                if (successEvent != null)
                {
                    successEvent(loadedTexture, url);
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

        public class ImageRequestQueue
        {
            List<ImageRequest> requests = new List<ImageRequest>();

            public void AddRequestToQueue(ImageRequest requestObj)
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
                foreach (ImageRequest request in this.requests)
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
            public ImageRequest FindRequest(string url)
            {
                foreach (ImageRequest request in this.requests)
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

            public ImageRequest GetAPendingRequest()
            {
                if (NumberOfRequestsInQueue() > 0)
                {
                    ImageRequest reqObj = requests[0] as ImageRequest;
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