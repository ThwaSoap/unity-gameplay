using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GamePlay 
{
    public abstract class UAssetLoader : ILevelLoader
    {
        public abstract UAsyncOperation LoadLevel(string _assetName, ELoadLevelMode _mode = ELoadLevelMode.SingleMainLevel, Action _finish=null);
        public abstract UAsyncOperation LoadLevelAsync(string _assetName, ELoadLevelMode _mode = ELoadLevelMode.SingleMainLevel, Action _finish=null);
        public abstract AssetType LoadAsset<AssetType>(string _assetName) where AssetType : UnityEngine.Object;
        public abstract UnityEngine.Object LoadAsset(System.Type _type, string _assetName);
        public abstract UResourceRequest LoadAssetAsync<AssetType>(string _assetName, Action<AssetType> _finish=null) where AssetType : UnityEngine.Object;
        public abstract UResourceRequest LoadAssetAsync(System.Type _type, string _assetName, Action<UnityEngine.Object> _finish = null);
        public abstract void UnloadAsset(bool _isDispose);

        UAsyncOperation ILevelLoader.LoadLevel(string _levelName, ELoadLevelMode _mode, Action _finish) { return LoadLevel(_levelName,_mode,_finish); }
        UAsyncOperation ILevelLoader.LoadLevelAsync(string _levelName, ELoadLevelMode _mode, Action _finish) { return LoadLevelAsync(_levelName,_mode,_finish); }
        void ILevelLoader.UnloadAsset(bool _isDispose) { UnloadAsset(_isDispose); }
    }
}
