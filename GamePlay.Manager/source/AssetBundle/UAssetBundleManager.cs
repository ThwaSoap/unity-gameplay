namespace GamePlay
{
    using Internal;
    using System.Collections.Generic;

    public class UAssetBundleManager 
    { 
        /// <summary>
        /// 存储AssetBundle的池，全局唯一
        /// </summary>
        UAssetBundlePool Pool;
        /// <summary>
        /// 模块的依赖关系管理器
        /// </summary>
        UAssetBundleManifestManager ManifestManager;

        UAssetBundleAsyncLoader AsyncLoader;

        public UAssetBundleLoaderProxyManager PathLoader { get; private set; }
        public UAssetBundleLoaderProxyManager ResLoader { get; private set; }

        Dictionary<string, UAssetBundleLoaderProxyManager> ProxyManagers = new Dictionary<string, UAssetBundleLoaderProxyManager>();

        internal string PackageExtension = "bytes";
        internal const string NAME_PATH = "PATH";
        internal const string NAME_RES  = "RES";
        internal UAssetBundleManager(UAssetBundlePool _instance) 
        {
            Pool = _instance;
            ManifestManager = new UAssetBundleManifestManager();
            AsyncLoader = new UAssetBundleAsyncLoader(this,Pool,ManifestManager); 
            PathLoader = new UAssetBundleLoaderProxyManager(new UAssetBundleLoaderFromPathAdapter(AsyncLoader));
            ResLoader = new UAssetBundleLoaderProxyManager(new UAssetBundleLoaderFromResourceAdapter(AsyncLoader));
            ProxyManagers.Add(NAME_PATH, PathLoader);
            ProxyManagers.Add(NAME_RES, ResLoader);
        }
        public void Initialized(ULevelManager _levelManager, string _assetBundleExtension="bytes") 
        {
            AsyncLoader.SetLevelManager(_levelManager);
        }

        public void AddProxyManager(string _name, UAssetBundleLoaderProxyManager _manager) 
        {
            if (_name == NAME_PATH || _name == NAME_RES || _manager == null) return;
            ProxyManagers[_name] = _manager;
        }

        public void RemProxyManager(string _name) 
        {
            if (_name == NAME_PATH || _name == NAME_RES) return;
            ProxyManagers.Remove(_name);
        }

        public UAssetBundleLoaderProxyManager GetProxyManager(string _name) 
        {
            return ProxyManagers[_name];
        }

        public UAssetBundleLoaderProxyManager this[string _name] 
        {
            get 
            {
                return ProxyManagers[_name];
            }
        }

        public void Dispose(bool _isDispose) 
        {
            AsyncLoader.Dispose(_isDispose);
            ResLoader.Clear();
            PathLoader.Clear();
            ProxyManagers.Clear();
        }
    }
}