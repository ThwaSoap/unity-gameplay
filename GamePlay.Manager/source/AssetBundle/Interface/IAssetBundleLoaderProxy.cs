using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlay.Internal
{
    interface IAssetBundleLoaderProxy
    {
        AssetType LoadAsset<AssetType>(string _packgeName, string _assetName) where AssetType : UnityEngine.Object;
        UnityEngine.Object LoadAsset(Type _type, string _packgeName, string _assetName);

        UResourceRequest LoadAssetAsync<AssetType>(string _packgeName, string _assetName, Action<AssetType> _finish) where AssetType : UnityEngine.Object;

        UResourceRequest LoadAssetAsync(Type _type, string _packgeName, string _assetName, Action<UnityEngine.Object> _finish);

        UAsyncOperation LoadLevel(string _packgeName, string _assetName, ELoadLevelMode _mode, Action _finish);
        UAsyncOperation LoadLevelAsync(string _packgeName, string _assetName, ELoadLevelMode _mode, Action _finish);

        void UnloadAsset(string _packageName, bool _isDispose);
    }
}
