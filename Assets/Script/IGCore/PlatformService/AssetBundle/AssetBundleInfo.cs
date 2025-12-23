using System.Collections.Generic;
using System;

namespace Core.AssetBundle
{
    [Serializable]
    public class HeaderInfo
    {
        public string date;
    }

    [Serializable]
    public class BundleInfo
    {
        public string name, path, hash;
        public long fileSize;
    }

    [Serializable]
    public class ManifestInfo
    {
        public HeaderInfo header;
        public List<BundleInfo> bundles;

        public BundleInfo GetBundleInfo(string strName)
        {
            for(int k = 0; k < bundles.Count; ++k)
            {
                if (bundles[k].name.ToLower() == strName.ToLower())
                    return bundles[k];
            }
            return null;
        }
    }
}
