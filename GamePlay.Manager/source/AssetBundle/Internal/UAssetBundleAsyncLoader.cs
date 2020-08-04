using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Internal
{
    class UAssetBundleAsyncLoader : UAssetBundleSyncLoader
    {
        public UAssetBundleAsyncLoader(UAssetBundleManager _manager, UAssetBundlePool _packageManager, UAssetBundleManifestManager _manifestManager)
            : base(_manager, _packageManager, _manifestManager) { }

        public UResourceRequest LoadAssetFromPathAsync<Asset>(string _dir, string _packageName, string _assetName, Action<Asset> _finished) where Asset : UnityEngine.Object
        {
            List<UAssetBundleCreateRequest> requests = new List<UAssetBundleCreateRequest>();
            LoadAssetBundle(_dir, _packageName, requests, LoadFromFile);
            return new UAssetBundleAssetRequest<Asset>(_assetName,requests,_finished);
        }

        public UResourceRequest LoadAssetFromPathAsync(Type _type, string _dir, string _packageName, string _assetName, Action<UnityEngine.Object> _finished)
        {
            List<UAssetBundleCreateRequest> requests = new List<UAssetBundleCreateRequest>();
            LoadAssetBundle(_dir, _packageName, requests, LoadFromFile);
            return new UAssetBundleAssetRequest(_assetName, _type,requests, _finished);
        }

        public UAsyncOperation LoadLevelFromPathAsync(string _dir, string _packageName, string _assetName, ELoadLevelMode _loadMode, Action _finished)
        {
            List<UAssetBundleCreateRequest> requests = new List<UAssetBundleCreateRequest>();
            LoadAssetBundle(_dir, _packageName, requests, LoadFromFile);
            return new UAssetBundleLevelRequest(LevelManager.Container, LevelManager.Container.CreateInstance(_assetName, _loadMode),requests, _finished);
        }

        public UAsyncOperation LoadLevelFromResourceAsync(string _dir, string _packageName, string _assetName, ELoadLevelMode _loadMode, Action _finished) 
        {
            List<UAssetBundleCreateRequest> requests = new List<UAssetBundleCreateRequest>();
            LoadAssetBundle(_dir, _packageName, requests, LoadFromResource);
            return new UAssetBundleLevelRequest(LevelManager.Container, LevelManager.Container.CreateInstance(_assetName, _loadMode), requests, _finished);
        }

        public UResourceRequest LoadAssetFromResourceAsync<Asset>(string _dir, string _packageName, string _assetName, Action<Asset> _finished) where Asset : UnityEngine.Object 
        {
            List<UAssetBundleCreateRequest> requests = new List<UAssetBundleCreateRequest>();
            LoadAssetBundle(_dir, _packageName, requests, LoadFromResource);
            return new UAssetBundleAssetRequest<Asset>(_assetName, requests, _finished); 
        }

        public UResourceRequest LoadAssetFromResourceAsync(Type _type, string _dir, string _packageName, string _assetName, Action<UnityEngine.Object> _finished) 
        {
            List<UAssetBundleCreateRequest> requests = new List<UAssetBundleCreateRequest>();
            LoadAssetBundle(_dir, _packageName, requests, LoadFromResource);
            return new UAssetBundleAssetRequest(_assetName, _type, requests, _finished);
        }

        #region 内部维护函数
        AssetBundleCreateRequest LoadFromFile(string _path) 
        {
            return AssetBundle.LoadFromFileAsync(_path + "."+ Manager.PackageExtension);
        }

        AssetBundleCreateRequest LoadFromResource(string _path) 
        {
            TextAsset file = Resources.Load<TextAsset>(_path);
            if (file == null) return null;
            return AssetBundle.LoadFromMemoryAsync(file.bytes);
        }

        void LoadAssetBundle(string _dir, string _packageName, List<UAssetBundleCreateRequest> _requests, Func<string, AssetBundleCreateRequest> _getCreateRequest)
        {
            //> 加载这个AB并返回这个文件是否加载过
            bool isLoaded = LoadAssetBundleInternal(_dir, _packageName, _requests, _getCreateRequest);

            //> 如果这个文件没有加载过，我们才加载这个文件的依赖文件
            if (!isLoaded)
                LoadDependencies(_dir, _packageName, _requests, _getCreateRequest);
        }

        bool LoadAssetBundleInternal(string _dir, string _packageName,List<UAssetBundleCreateRequest> _requests, Func<string, AssetBundleCreateRequest> _getCreateRequest)
        {
            //> 检查并加载这个模块的关系依赖文件
            string moduleName = UAssetBundleManifestManager.ParseModuleName(_packageName);
            string fullModuleName = GetAssetBundlePath(_dir, moduleName, moduleName);
            string fullPackgeName = GetAssetBundlePath(_dir, moduleName, _packageName);
            if (false == ManifestManager.Contains(fullModuleName))
            {
                //> 加载依赖关系文件
                string content = LoadPathText(fullModuleName);
                ManifestManager.AddManifest(fullModuleName, content);
            }

            //> 如果他已经在完成列表中，那么将他的引用++
            if (PackageManager.ApplyPackage(fullPackgeName) != null) 
            {
                //> 已经加载完成了
                return true;
            }

            UAssetBundleCreateRequest outRequest;
            if (PackageManager.TryGetRequest(fullModuleName, out outRequest)) 
            {
                //> 已经处于加载状态了
                _requests.Add(outRequest);
                return true;
            }

            string path = fullPackgeName + "." + Manager.PackageExtension;
            AssetBundleCreateRequest createRequest = _getCreateRequest(path); 
            if (createRequest == null || createRequest.isDone && createRequest.assetBundle == null)
            {
                //> 执行加载操作失败了
                PackageManager.RemPackage(fullPackgeName,1,true);
                return true;
            }
            else 
            {
                var request = new UAssetBundleCreateRequestUnity(createRequest, fullPackgeName, (NAME, VALUE) =>
                {
                    PackageManager.CompleteRequest(fullPackgeName, VALUE);
                });
                //> 成功执行了加载
                PackageManager.AddRequest(fullPackgeName, request);
                _requests.Add(request);
                return false;
            }
        }

        void LoadDependencies(string _dir, string _packageName, List<UAssetBundleCreateRequest> _requests, Func<string, AssetBundleCreateRequest> _getCreateRequest)
        {
            //> 获取某个模块下某个包的关系依赖文件
            string moduleName = UAssetBundleManifestManager.ParseModuleName(_packageName);
            string fullModuleName = GetAssetBundlePath(_dir,moduleName,moduleName);
            List<string> dependValue = ManifestManager.GetDependence(fullModuleName, _packageName);
            if (dependValue.Count == 0)
                return;

            //> 记录和加载所有依赖文件
            for (int i = 0; i < dependValue.Count; i++)
                LoadAssetBundleInternal(_dir, dependValue[i], _requests, _getCreateRequest);
        }
        #endregion
    }
}