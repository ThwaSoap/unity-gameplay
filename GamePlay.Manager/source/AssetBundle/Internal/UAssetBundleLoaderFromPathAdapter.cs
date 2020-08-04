using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlay.Internal
{
    class UAssetBundleLoaderFromPathAdapter : IAssetBundleLoader
    {
        UAssetBundleAsyncLoader Loader;
        public UAssetBundleLoaderFromPathAdapter(UAssetBundleAsyncLoader _loader) 
        {
            Loader = _loader;
        }

        #region 从路径加载AssetBundle
        AssetType IAssetBundleLoader.LoadAsset<AssetType>(string _dir, string _packageName, string _assetName)
        {
            return Loader.LoadAssetFromPath<AssetType>(_dir, _packageName, _assetName);
        }

        UnityEngine.Object IAssetBundleLoader.LoadAsset(Type _type, string _dir, string _packageName, string _assetName)
        {
            return Loader.LoadAssetFromPath(_type, _dir, _packageName, _assetName);
        }

        UResourceRequest IAssetBundleLoader.LoadAssetAsync<AssetType>(string _dir, string _packageName, string _assetName, Action<AssetType> _finish)
        {
            return Loader.LoadAssetFromPathAsync<AssetType>(_dir, _packageName, _assetName, _finish);
        }

        UResourceRequest IAssetBundleLoader.LoadAssetAsync(Type _type, string _dir, string _packageName, string _assetName, Action<UnityEngine.Object> _finish)
        {
            return Loader.LoadAssetFromPathAsync(_type, _dir, _packageName, _assetName, _finish);
        }

        UAsyncOperation IAssetBundleLoader.LoadLevel(string _dir, string _packageName, string _assetName, ELoadLevelMode _mode, Action _finish)
        {
            return Loader.LoadLevelFromPath(_dir, _packageName, _assetName, _mode, _finish);
        }

        UAsyncOperation IAssetBundleLoader.LoadLevelAsync(string _dir, string _packageName, string _assetName, ELoadLevelMode _mode, Action _finish)
        {
            return Loader.LoadLevelFromPathAsync(_dir, _packageName, _assetName, _mode, _finish);
        }
        void IAssetBundleLoader.UnloadAsset(string _dir, string _packageName, bool _isDispose)
        {
            Loader.Unload(_dir, _packageName, _isDispose);
        }
        #endregion
    }
}
