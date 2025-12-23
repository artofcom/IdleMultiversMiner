using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Core.WWW;

namespace Core.AssetBundle
{
    public class AssetBundleManager
    {
        // [const values] ------------------------------
        //
        const string LOCAL_BUNDLE_PATH = "Assets/Bundles";
        const string LastValidURLKey = "LastValidBundleManifestURL";


        // [Members] -----------------------------------
        //
        ManifestInfo manifestInfo;
        Dictionary<string, UnityEngine.AssetBundle> BundleCache = new Dictionary<string, UnityEngine.AssetBundle>();
        bool mUseRemoteBundle = true;
        Queue<BundleRequest> bundleRequestQueue = new Queue<BundleRequest>();
        MonoBehaviour coroutineRunner;
        string mReqAbortBundleName = string.Empty;
        string mCDNHead = string.Empty;
        bool mAbortAll = false;

        // [Public functions] ---------------------
        //
        public IEnumerator Init(MonoBehaviour runner, bool useRemoteBundle, string CDNHead)
        {
            // Local 
            //string strJson = Resources.Load<TextAsset>("Data/assetManifest-IOS").text;
            //manifestInfo = JsonUtility.FromJson< ManifestInfo >(strJson);

            UnityEngine.Assertions.Assert.IsTrue(runner != null);
            coroutineRunner = runner;
            mCDNHead = CDNHead;

            mUseRemoteBundle = useRemoteBundle;
            if (!mUseRemoteBundle)
                yield break;

            // Remote.
            DateTime dateTime = DateTime.UtcNow;
            string cacheIgnoreKey = $"?cacheignorekey={dateTime.Year}{dateTime.Month}{dateTime.Day}{dateTime.Hour}{dateTime.Second}";
            string fileName = Application.platform==RuntimePlatform.Android ? $"assetManifest-android_{Application.version}.json" : $"assetManifest-ios_{Application.version}.json";
            string manifestURL = $"{CDNHead}/assetbundles/manifest/" + fileName + cacheIgnoreKey;
            bool fetching = true;
            WWWTextGet.GetDataForTextURLExt(manifestURL, (string loadedText, string textUrl) =>
            {
                if(!string.IsNullOrEmpty(loadedText))
                    manifestInfo = JsonUtility.FromJson<ManifestInfo>(loadedText);
                if (manifestInfo != null)
                    PlayerPrefs.SetString(LastValidURLKey, manifestURL);

                fetching = false;
                Debug.Log(textUrl + " Downloaded successfully.");
            },
            (string textUrl, string error) =>
            {
                Debug.LogWarning(textUrl + " Downloading has been failed... " + error);
                fetching = false;
            },
            useCache: false);

            while (fetching)
                yield return null;


            // Local Cache.
            if(manifestInfo == null)
            {
                manifestURL = PlayerPrefs.GetString(LastValidURLKey, "");
                if(!string.IsNullOrEmpty(manifestURL))
                {
                    fetching = true;
                    WWWTextGet.GetDataForTextURLExt(manifestURL, (string loadedText, string textUrl) =>
                    {
                        if (!string.IsNullOrEmpty(loadedText))
                            manifestInfo = JsonUtility.FromJson<ManifestInfo>(loadedText);

                        fetching = false;
                        if(manifestInfo != null)
                            Debug.Log(textUrl + " loaded from cache successfully.");
                    },
                    (string textUrl, string error) =>
                    {
                        Debug.LogWarning(textUrl + " has been failed to load from cache... " + error);
                        fetching = false;
                    },
                    useCache: true);

                    while (fetching)
                        yield return null;
                }
            }

            if (manifestInfo == null)
            {
                // In this case, the player doesn't have internet connection and local caches.
                // Unable to use asset-bundles.
                Debug.LogWarning("[AssetBundle] Failed to load manifest file. AssetBundle Manager will be disabled.");
            }
            else
                coroutineRunner.StartCoroutine(coUpdateRequestQueue());

            yield break;
        }

        // Fetch Bundle and then, load asset from the bundle.
        public IEnumerator LoadAssetBundle<T>(string bundleName, string assetName, Action<T> callbackDone, Action<float> callbackDownloadProgress) where T : UnityEngine.Object
        {
            bundleName = bundleName.ToLower();
            assetName = assetName.ToLower();

            UnityEngine.AssetBundle bundle = null;
            if (BundleCache.ContainsKey(bundleName))
            {
                Debug.Log("[Fetching] Loading Bundle From Runtime Cache..." + bundleName);
                bundle = BundleCache[bundleName];
                AssetBundleRequest bundleReq = bundle.LoadAssetAsync(assetName, typeof(T));
                yield return bundleReq;

                if (callbackDone != null)
                    callbackDone( bundleReq.asset as T );

                yield break;
            }

//#if UNITY_EDITOR
            // Local Mode.
            if(mUseRemoteBundle == false)
            {
                // Load Bundle from Local location.
                /*string assetPathExt = $"{bundleName}/{assetName}";// + GetFileExtension(typeof(T));
                string externalizedAssetPath = $"{LOCAL_BUNDLE_PATH}/{assetPathExt}";
                Debug.Log("[Fetching] Loading locally..." + externalizedAssetPath);
                T resource = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(externalizedAssetPath);
                */

                string assetPathExt = $"{bundleName}/{assetName}";// + GetFileExtension(typeof(T));
                string externalizedAssetPath = $"Bundles/{assetPathExt}";
                Debug.Log("[Fetching] Loading locally..." + externalizedAssetPath);
                T resource = Resources.Load(externalizedAssetPath) as T;
                if (callbackDone != null)
                    callbackDone( resource );
                yield break;
            }
//#endif

            // Try fetch data from Remote.
            if (manifestInfo == null || manifestInfo.bundles == null)
            {
                if (callbackDone != null)
                    callbackDone(null);
                yield break;
            }
            BundleInfo info = manifestInfo.GetBundleInfo(bundleName);
            if (info == null)
            {
                if (callbackDone != null)
                    callbackDone.Invoke(null);
                yield break;
            }

            if (IsCached(bundleName))
            {
                Debug.Log("[Fetching] Loading Bundle From Local Cache..." + bundleName);

                coroutineRunner.StartCoroutine(coLoadAssetWithBypassQueue(info, (loadedBundle) =>
                {
                    T loadAsset = loadedBundle==null ? null : loadedBundle.LoadAsset<T>(assetName);
                    if (callbackDone != null)
                        callbackDone.Invoke(loadAsset);
                }));
            }
            else
            {
                Debug.Log("[Fetching] Downloading Bundle From CDN..." + bundleName);

                AddBundleToDownloadQueue(info, bundleName, callbackDownloadProgress, (loadedBundle) =>
                {
                    T loadAsset = null;
                    if (loadedBundle != null)
                        loadAsset = loadedBundle.LoadAsset<T>(assetName);
                    
                    if (callbackDone != null)
                        callbackDone.Invoke(loadAsset);
                });
            }
        }

        // Load Asset from the input-Bundle.
        public IEnumerator LoadAssetFromBundle<T>(UnityEngine.AssetBundle bundle, string assetName, Action<T> callbackDone) where T : UnityEngine.Object
        {
            assetName = assetName.ToLower();

#if UNITY_EDITOR
            // Local Mode.
            if (mUseRemoteBundle == false)
            {
                // Should fail when local-mode.
                if (callbackDone != null)
                    callbackDone(null);
                yield break;
            }
#endif

            if (bundle == null)
            {
                if (callbackDone != null)
                    callbackDone(null);
                yield break;
            }
            
            AssetBundleRequest bundleReq = bundle.LoadAssetAsync(assetName, typeof(T));
            yield return bundleReq;

            if (callbackDone != null)
                callbackDone(bundleReq.asset as T);
        }

        // Fetch Bundle Only.
        public IEnumerator FetchBundle(string bundleName, Action<UnityEngine.AssetBundle> callbackDone, Action<float> callbackDownloadProgress) 
        {
            bundleName = bundleName.ToLower();

            UnityEngine.AssetBundle bundle = null;
            if (BundleCache.ContainsKey(bundleName))
            {
                Debug.Log("[Fetching] Loading Bundle From Runtime Cache..." + bundleName);
                bundle = BundleCache[bundleName];
                
                if (callbackDone != null)
                    callbackDone(bundle);

                yield break;
            }

#if UNITY_EDITOR
            // Local Mode.
            if (mUseRemoteBundle == false)
            {
                if (callbackDone != null)
                    callbackDone(null);
                yield break;
            }
#endif

            // Try fetch data from Remote.
            if (manifestInfo == null || manifestInfo.bundles == null)
            {
                if (callbackDone != null)
                    callbackDone(null);
                yield break;
            }
            BundleInfo info = manifestInfo.GetBundleInfo(bundleName);
            if (info == null)
            {
                if (callbackDone != null)
                    callbackDone.Invoke(null);
                yield break;
            }

            if (IsCached(bundleName))
            {
                Debug.Log("[Fetching] Loading Bundle From Local Cache..." + bundleName);

                coroutineRunner.StartCoroutine(coLoadAssetWithBypassQueue(info, (loadedBundle) =>
                {
                    if (callbackDone != null)
                        callbackDone.Invoke(loadedBundle);
                }));
            }
            else
            {
                Debug.Log("[Fetching] Adding Queue to download From CDN..." + bundleName);

                AddBundleToDownloadQueue(info, bundleName, callbackDownloadProgress, (loadedBundle) =>
                {
                    if (callbackDone != null)
                        callbackDone.Invoke(loadedBundle);
                });
            }
        }

        public void ClearCache()
        {
            //foreach (var key in BundleCache.Keys)
            //    BundleCache[key].Unload(true);
            BundleCache.Clear();
            foreach (var bundle in UnityEngine.AssetBundle.GetAllLoadedAssetBundles())
            {
                bundle.Unload(true);
            }
            Caching.ClearCache();
        }

        public long GetCachedSpaceSize()
        {
            long spaceSize = 0;
            for(int q = 0; q < Caching.cacheCount; ++q)
            {
                var cache = Caching.GetCacheAt(q);
                spaceSize += cache.spaceOccupied;
            }
            return spaceSize;
        }

        public void ClearCache(string bundleName)
        {
            bundleName = bundleName.ToLower();
            if (BundleCache.ContainsKey(bundleName))
                BundleCache.Remove(bundleName);
            
            foreach (var bundle in UnityEngine.AssetBundle.GetAllLoadedAssetBundles())
            {
                if(bundle.name.ToLower() == bundleName)
                {
                    bundle.Unload(true);
                    break;
                }
            }
            Caching.ClearAllCachedVersions(bundleName);
        }

        public bool IsCached(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (manifestInfo == null || manifestInfo.bundles == null)
                return false;

            BundleInfo info = manifestInfo.GetBundleInfo(name);
            if (info == null)
                return false;

            string URLHead = "https://storage.googleapis.com/";
            string path = URLHead + info.path;
            path += "/";
            path += info.name;

            return Caching.IsVersionCached(path, Hash128.Parse(info.hash));
        }

        public long GetBundleSize(string name)
        {
            if (string.IsNullOrEmpty(name))
                return 0;

            if (manifestInfo == null || manifestInfo.bundles == null)
                return 0;

            BundleInfo info = manifestInfo.GetBundleInfo(name);
            if (info == null)
                return 0;

            return info.fileSize;
        }

        // Fetch Bundle from Local Cached Map.
        public UnityEngine.AssetBundle GetAssetBundleFromCacheMap(string bundleName)
        {
            string key = bundleName.ToLower();
            if (BundleCache.ContainsKey(key))
                return BundleCache[key];

            return null;
        }


        public void AbortDownloading(string bundleName)
        {
            mReqAbortBundleName = bundleName.ToLower();
        }
        public void AbortAllDownloading()
        {
            mAbortAll = true;
        }


        // [Asset Download Queue] ---------------------
        //
        IEnumerator coUpdateRequestQueue()
        {
            var waitForSec = new WaitForSeconds(0.2f);
            while (true)
            {
                if (bundleRequestQueue.Count <= 0)
                {
                    if (!string.IsNullOrEmpty(mReqAbortBundleName))
                        mReqAbortBundleName = string.Empty;

                    mAbortAll = false;
                    yield return waitForSec;
                    continue;
                }

                var reqInfo = bundleRequestQueue.Dequeue();
                if(mAbortAll)
                {
                    if (reqInfo.endEvent != null)
                        reqInfo.endEvent.Invoke(null);
                    continue;
                }
                var www = UnityWebRequestAssetBundle.GetAssetBundle(reqInfo.remotePath, Hash128.Parse(reqInfo.hash));
                www.SendWebRequest();


                //------------------------------------------------
                //
                while (www.downloadProgress < 1.0f)
                {
                    Debug.Log(www.downloadProgress);
                    if(reqInfo.downloadProgressEvent != null)
                        reqInfo.downloadProgressEvent(www.downloadProgress);


                    // Processing Bundle Cancelation.
                    if( (!string.IsNullOrEmpty(mReqAbortBundleName) && reqInfo.bundleName.Equals(mReqAbortBundleName)) ||       // Aborted from outside.
                        (!IsCached(reqInfo.bundleName) && Application.internetReachability==NetworkReachability.NotReachable) ) // Network Unreachable. 
                    {
                        www.Abort();
                        Debug.Log("[ABM] Download has been canceled.");
                        mReqAbortBundleName = string.Empty;
                        break;
                    }

                    if(mAbortAll)
                    {
                        www.Abort();
                        Debug.Log("[ABM] All downloads have been canceled.!");
                        break;
                    }

                    yield return null;
                }
                //
                //------------------------------------------------

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                    if (reqInfo.endEvent != null)
                        reqInfo.endEvent.Invoke(null);
                    continue;
                }

                UnityEngine.AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                CacheBundleToMap(bundle, reqInfo.bundleName);
                if (reqInfo.endEvent != null)
                    reqInfo.endEvent.Invoke(bundle);
            }
        }

        void AddBundleToDownloadQueue(BundleInfo info, string bundleName, Action<float> callbackDownloadProgress, Action<UnityEngine.AssetBundle> callbackDone)
        {
            if (info == null)
            {
                if (callbackDone != null)
                    callbackDone.Invoke(null);
                return;
            }

            string path = $"{mCDNHead}/{info.path}/{info.name}";
            BundleRequest request = new BundleRequest();
            request.bundleName = bundleName;
            request.remotePath = path;
            request.hash = info.hash;
            request.downloadProgressEvent = callbackDownloadProgress;
            request.endEvent = (loadedBundle) =>
            {
                if (callbackDone != null)
                    callbackDone.Invoke(loadedBundle);
            };
            bundleRequestQueue.Enqueue(request);
        }

        IEnumerator coLoadAssetWithBypassQueue(BundleInfo info, Action<UnityEngine.AssetBundle> callbackDone)
        {
            string path = $"{mCDNHead}/{info.path}/{info.name}";
            var www = UnityWebRequestAssetBundle.GetAssetBundle(path, Hash128.Parse(info.hash));
            www.SendWebRequest();

            while (www.downloadProgress < 1.0f)
                yield return null;

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"[Cached Asset] Has some error... {www.error}");
                if (callbackDone != null)
                    callbackDone.Invoke(null);
            }
            else
            {
                UnityEngine.AssetBundle loadedBundle = DownloadHandlerAssetBundle.GetContent(www);
                CacheBundleToMap(loadedBundle, info.name);
                if (callbackDone != null)
                    callbackDone.Invoke(loadedBundle);
            }
        }


        void CacheBundleToMap(UnityEngine.AssetBundle bundle, string key)
        {
            if (bundle!=null && !BundleCache.ContainsKey(key))
                BundleCache[key] = bundle;
        }

        private static string GetFileExtension(Type t)
        {
            if (t == typeof(UnityEngine.GameObject))
                return ".prefab";
            else if (t == typeof(UnityEngine.Sprite))
                return ".png";
            else if (t == typeof(UnityEngine.Texture2D))
                return ".png";
            else if (t == typeof(UnityEngine.AudioClip))
                return ".mp3";
            else if (t == typeof(UnityEngine.Material))
                return ".mat";
            else if (t == typeof(UnityEngine.Shader))
                return ".shader";
            else if (t == typeof(UnityEngine.AnimationClip))
                return ".anim";
            else if (typeof(ScriptableObject).IsAssignableFrom(t))
                return ".asset";
            
            return default(string);
        }
    }

    public class BundleRequest
    {
        public string bundleName;
        public string remotePath;   // URL.
        public string hash;
        public Action<UnityEngine.AssetBundle> endEvent;
        public Action<float> downloadProgressEvent;
    }
}


/*
 *public IEnumerator GetAssetBundle(string name, string prefabName, Action<GameObject> callbackDone)
        {
            if (manifestInfo == null || manifestInfo.bundles == null)
            {
                if (callbackDone != null)
                    callbackDone.Invoke(null);
                yield break;
            }

            name = name.ToLower();


            UnityEngine.AssetBundle bundle = null;
            if (BundleCache.ContainsKey(name))
            {
                Debug.Log("[Fetching] Loading Bundle From Runtime Cache..." + name);
                bundle = BundleCache[name];
                var _loadAsset = bundle.LoadAssetAsync<GameObject>(prefabName);
                yield return _loadAsset;

                var _loadedObject = (GameObject)_loadAsset.asset;
                if (callbackDone != null)
                    callbackDone.Invoke(_loadedObject);

                yield break;
            }

            
            BundleInfo info = manifestInfo.GetBundleInfo(name);
            if (info == null)
            {
                if (callbackDone != null)
                    callbackDone.Invoke(null);
                yield break;
            }

            if (IsCached(name))
                Debug.Log("[Fetching] Loading Bundle From Local Cache..." + name);
            else
                Debug.Log("[Fetching] Downloading Bundle From CDN..." + name);


            string URLHead = "https://storage.googleapis.com/";
            string path = URLHead + info.path;
            path += "/";
            path += info.file;

            var www = UnityWebRequestAssetBundle.GetAssetBundle( path, Hash128.Parse(info.hash) );
            www.SendWebRequest();
            while (www.downloadProgress < 1.0f)
            {
                Debug.Log(www.downloadProgress);
                yield return null;
            }
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                if (callbackDone != null)
                    callbackDone.Invoke(null);
                yield break;
            }

            bundle = DownloadHandlerAssetBundle.GetContent(www);
            if (!BundleCache.ContainsKey(name))
                BundleCache[name] = bundle;

            var loadAsset = bundle.LoadAssetAsync<GameObject>(prefabName);
            yield return loadAsset;

            var loadedObject = (GameObject)loadAsset.asset;
            if (callbackDone != null)
                callbackDone.Invoke(loadedObject);
        }
 */