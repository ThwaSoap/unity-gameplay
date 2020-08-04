using System;
using System.Collections.Generic;

namespace GamePlay
{
    using Internal;
    /// <summary>
    /// 资源包加载器的代理对象
    /// 代理对象负责桥接 应用客户 与 实际服务产出
    /// </summary>
    public class UAssetBundleLoaderProxy : IAssetBundleLoaderProxy
    {
        string Path;
        IAssetBundleLoader Interface;
        Dictionary<string, UAssetLoader> Loaders = new Dictionary<string, UAssetLoader>();

        #region 获取加载器

        public UAssetLoader this[string _packageName]
        {
            get 
            {
                return  GetLoader(_packageName);
            }
        }

        public UAssetLoader GetLoader(string _packageName) 
        {
            UAssetLoader outLoader;
            if (Loaders.TryGetValue(_packageName, out outLoader) == false) 
            {
                outLoader = new UAssetBundleLoaderImp(this,_packageName);
                Loaders.Add(_packageName,outLoader);
            }
            return outLoader;
        }
        #endregion

        internal UAssetBundleLoaderProxy(string path, IAssetBundleLoader _interface) 
        {
            Interface = _interface;
            Path = path;
        }

        public void Clear() 
        {
            Loaders.Clear();
        }
        #region 实现代理接口
        AssetType IAssetBundleLoaderProxy.LoadAsset<AssetType>(string _packgeName,string _assetName)
        {
            return Interface.LoadAsset<AssetType>(Path,_packgeName,_assetName);
        }

        UnityEngine.Object IAssetBundleLoaderProxy.LoadAsset(Type _type, string _packgeName, string _assetName)
        {
            return Interface.LoadAsset(_type,Path,_packgeName,_assetName);
        }

        UResourceRequest IAssetBundleLoaderProxy.LoadAssetAsync<AssetType>(string _packgeName, string _assetName, Action<AssetType> _finish)
        {
            return Interface.LoadAssetAsync<AssetType>(Path, _packgeName, _assetName,_finish);
        }

        UResourceRequest IAssetBundleLoaderProxy.LoadAssetAsync(Type _type, string _packgeName, string _assetName, Action<UnityEngine.Object> _finish)
        {
            return Interface.LoadAssetAsync(_type,Path, _packgeName, _assetName, _finish);
        }

        UAsyncOperation IAssetBundleLoaderProxy.LoadLevel(string _packgeName, string _assetName, ELoadLevelMode _mode, Action _finish)
        {
            return Interface.LoadLevel(Path, _packgeName, _assetName, _mode, _finish);
        }

        UAsyncOperation IAssetBundleLoaderProxy.LoadLevelAsync(string _packgeName, string _assetName, ELoadLevelMode _mode, Action _finish)
        {
            return Interface.LoadLevelAsync(Path, _packgeName, _assetName, _mode, _finish);
        }

        void IAssetBundleLoaderProxy.UnloadAsset(string _packageName,bool _isDispose)
        {
            Interface.UnloadAsset(Path,_packageName, _isDispose);
        }
        #endregion

        #region 加载器实现
        class UAssetBundleLoaderImp : UAssetLoader
        {
            IAssetBundleLoaderProxy Proxy;
            string PackageName;
            public UAssetBundleLoaderImp(IAssetBundleLoaderProxy _proxy, string _packgeName)
            {
                Proxy = _proxy;
                PackageName = _packgeName;
            }

            public override AssetType LoadAsset<AssetType>(string _assetName)
            {
                return Proxy.LoadAsset<AssetType>(PackageName, _assetName);
            }

            public override UnityEngine.Object LoadAsset(Type _type, string _assetName)
            {
                return Proxy.LoadAsset(_type, PackageName, _assetName);
            }

            public override UResourceRequest LoadAssetAsync<AssetType>(string _assetName, Action<AssetType> _finish)
            {
                return Proxy.LoadAssetAsync<AssetType>(PackageName, _assetName, _finish);
            }

            public override UResourceRequest LoadAssetAsync(Type _type, string _assetName, Action<UnityEngine.Object> _finish)
            {
                return Proxy.LoadAssetAsync(_type, PackageName, _assetName, _finish);
            }

            public override UAsyncOperation LoadLevel(string _assetName, ELoadLevelMode _mode, Action _finish)
            {
                return Proxy.LoadLevel(PackageName, _assetName, _mode, _finish);
            }

            public override UAsyncOperation LoadLevelAsync(string _assetName, ELoadLevelMode _mode, Action _finish)
            {
                return Proxy.LoadLevelAsync(PackageName, _assetName, _mode, _finish);
            }

            public override void UnloadAsset(bool _isDispose)
            {
                Proxy.UnloadAsset(PackageName, _isDispose);
            }
        }
        #endregion
    }
}
