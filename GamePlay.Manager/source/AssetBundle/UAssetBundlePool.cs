using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    using Internal;
    public sealed class UAssetBundlePool
    {
        internal class UAssetBundleInstance
        {
            public string Path { get; private set; }
            public AssetBundle Instance;
            public int ReferenceCount; 
            public UAssetBundleInstance(string _path) 
            {
                Path = _path;
            }
        }

        Dictionary<string, UAssetBundleInstance> Pool = new Dictionary<string, UAssetBundleInstance>();
        Dictionary<string, UAssetBundleCreateRequest> WokerRequest = new Dictionary<string, UAssetBundleCreateRequest>();

        #region 内部接口
        private UAssetBundlePool(){}

        static UAssetBundlePool Instance;
        static UAssetBundlePool GetInstance() 
        {
            if (Instance == null) 
            {
                Instance = new UAssetBundlePool();
            }
            return Instance;
        }
        #endregion

        public static UAssetBundleManager CreateManager() 
        {
            return new UAssetBundleManager(GetInstance());
        }

        internal AssetBundle ApplyPackage(string _path) 
        {
            UAssetBundleInstance outPackage;
            if (!Pool.TryGetValue(_path, out outPackage))
            {
                outPackage = new UAssetBundleInstance(_path);
                Pool.Add(_path, outPackage);
            }
            outPackage.ReferenceCount += 1;
            return outPackage.Instance;
        }

        internal void AddPackage(string _path, AssetBundle _instance)
        {
            UAssetBundleInstance outPackage;
            if (Pool.TryGetValue(_path, out outPackage))
            {
                outPackage.Instance = _instance; 
            }
            else 
            {
                //> 说明这个AB在异步加载期间已经被移除掉了
                _instance.Unload(true);
                Debug.LogErrorFormat("AssetBundle加载异常: [{0}]可能在加载期间被释放.", _path); 
            }
        }

        internal bool RemPackage(string _path,int _count, bool _isRelease)
        {
            bool removed = false;
            UAssetBundleInstance outPackage;
            if (Pool.TryGetValue(_path, out outPackage))
            {
                outPackage.ReferenceCount -= _count;
                if (outPackage.ReferenceCount <= 0)
                {
                    removed = true;
                    if (outPackage.Instance != null)
                    {
                        outPackage.Instance.Unload(_isRelease);
                        outPackage.Instance = null;
                    }
                    Pool.Remove(_path);
                }
            }
            return removed;
        }

        #region 处理请求
        internal void AddRequest(string _path,UAssetBundleCreateRequest _request)
        {
            if (WokerRequest.ContainsKey(_path)) return;

            WokerRequest.Add(_path, _request); 
        }

        internal void CompleteRequest(string _path, AssetBundle _value) 
        {
            AddPackage(_path, _value); 
            WokerRequest.Remove(_path);
        }

        internal bool TryGetRequest(string _path, out UAssetBundleCreateRequest _outRequest) 
        {
            return WokerRequest.TryGetValue(_path,out _outRequest);
        }
        #endregion
    }
}