using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    public enum EUIExitMode
    {
        DelayDestroy,
        JustHidden
    }

    [System.Serializable]
    public abstract class UUIInfo
    {
        public bool IsSceneObject = false;
        public abstract string GetName();
    }

    [System.Serializable]
    public class UUIInfoResource : UUIInfo
    {
        public string Name;
        public string DIR;
        public override string GetName()
        {
            return Name;
        }
    }

    [System.Serializable]
    public class UUIInfoAssetBundle : UUIInfo
    {
        public string LoaderName = UAssetBundleManager.NAME_PATH;
        public string UIName;
        public string ProxyName;
        public string PackageName;
        public string AssetName;
        public override string GetName()
        {
            if (string.IsNullOrEmpty(UIName))
                return AssetName;
            else
                return UIName;
        }

        public string GetLoaderName() 
        {
            if (string.IsNullOrEmpty(LoaderName)) return UAssetBundleManager.NAME_PATH;
            return LoaderName;
        }
    }

    [System.Serializable]
    public class UUIInfoStaticSingle : UUIInfo
    {
        public string UIName;
        public GameObject Instance; 
        public override string GetName()
        {
            if (string.IsNullOrEmpty(UIName))
                return Instance?.name;
            else
                return UIName;
        }
    }

    [System.Serializable]
    public class UUIInfoStaticTemplate : UUIInfo
    {
        public string UIName;
        public GameObject Instance; 
        public override string GetName()
        {
            if (string.IsNullOrEmpty(UIName))
                return Instance?.name;
            else
                return UIName;
        }
    }
}

