using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlay.Internal
{
    class UAssetBundleLoaderFromResourceAdapter : IAssetBundleLoader
    {
        UAssetBundleAsyncLoader Loader;
        public UAssetBundleLoaderFromResourceAdapter(UAssetBundleAsyncLoader _loader) 
        {
            Loader = _loader;
        }
        AssetType IAssetBundleLoader.LoadAsset<AssetType>(string _dir, string _packageName, string _assetName)
        {
            return Loader.LoadAssetFromResource<AssetType>(_dir,_packageName,_assetName);
        }

        UnityEngine.Object IAssetBundleLoader.LoadAsset(Type _type, string _dir, string _packageName, string _assetName)
        {
            return Loader.LoadAssetFromResource(_type,_dir,_packageName,_assetName);
        }

        UResourceRequest IAssetBundleLoader.LoadAssetAsync<AssetType>(string _dir, string _packageName, string _assetName, Action<AssetType> _finish)
        {
            return Loader.LoadAssetFromResourceAsync<AssetType>(_dir,_packageName,_assetName,_finish);
        }

        UResourceRequest IAssetBundleLoader.LoadAssetAsync(Type _type, string _dir, string _packageName, string _assetName, Action<UnityEngine.Object> _finish)
        {
            return Loader.LoadAssetFromResourceAsync(_type,_dir,_packageName,_assetName,_finish);
        }

        UAsyncOperation IAssetBundleLoader.LoadLevel(string _dir, string _packageName, string _assetName, ELoadLevelMode _mode, Action _finish)
        {
            return Loader.LoadLevelFromResource(_dir,_packageName, _assetName,_mode,_finish); 
        }

        UAsyncOperation IAssetBundleLoader.LoadLevelAsync(string _dir, string _packageName, string _assetName, ELoadLevelMode _mode, Action _finish)
        {
            return Loader.LoadLevelFromResourceAsync(_dir,_packageName,_assetName,_mode,_finish);
        }

        void IAssetBundleLoader.UnloadAsset(string _dir, string _packageName, bool _isDispose)
        {
            Loader.Unload(_dir,_packageName,_isDispose);
        }
    }
}
