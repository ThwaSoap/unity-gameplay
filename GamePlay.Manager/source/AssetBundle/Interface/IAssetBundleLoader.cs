using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlay
{
    public interface IAssetBundleLoader
    {
        AssetType LoadAsset<AssetType>(string _dir,string _packageName,string _assetName)where AssetType : UnityEngine.Object; 
        UnityEngine.Object LoadAsset(Type _type, string _dir, string _packageName, string _assetName);
        UResourceRequest LoadAssetAsync<AssetType>(string _dir, string _packageName, string _assetName, Action<AssetType> _finish) where AssetType : UnityEngine.Object;
        UResourceRequest LoadAssetAsync(Type _type, string _dir, string _packageName, string _assetName, Action<UnityEngine.Object> _finish);
        UAsyncOperation LoadLevel(string _dir, string _packageName, string _assetName, ELoadLevelMode _mode, Action _finish);
        UAsyncOperation LoadLevelAsync(string _dir, string _packageName, string _assetName, ELoadLevelMode _mode, Action _finish);
        void UnloadAsset(string _dir,string _packageName,bool _isDispose);
    }
}
