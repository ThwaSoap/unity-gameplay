using System;
using UnityEngine;

namespace GamePlay.Internal
{
    class UResourceLoader : UAssetLoader
    {
        ULevelManager LevelManager;
        public void Init(ULevelManager _manger) 
        {
            LevelManager = _manger;
        }

        public override AssetType LoadAsset<AssetType>(string _path)
        {
            return Resources.Load<AssetType>(_path);
        }

        public override UnityEngine.Object LoadAsset(Type _type, string _assetName)
        {
            return Resources.Load(_assetName,_type);
        }

        public override UResourceRequest LoadAssetAsync<AssetType>(string _path, Action<AssetType> _finish)
        {
            return new UResourceRequestUnity<AssetType>(_path,_finish);
        }

        public override UResourceRequest LoadAssetAsync(Type _type, string _path, Action<UnityEngine.Object> _finish)
        {
            return new UResourceRequestUnity(_path,_type,_finish);
        }

        public override UAsyncOperation LoadLevel(string _assetName, ELoadLevelMode _mode, Action _finish)
        {
            return new UUnityLevelRequest(LevelManager.Container,LevelManager.Container.CreateInstance(_assetName,_mode),_finish);
        }

        public override UAsyncOperation LoadLevelAsync(string _assetName, ELoadLevelMode _mode, Action _finish)
        {
            return new UUnityLevelRequest(LevelManager.Container, LevelManager.Container.CreateInstance(_assetName, _mode), _finish);
        }

        public override void UnloadAsset(bool _isDispose)
        {
        }
    }
}