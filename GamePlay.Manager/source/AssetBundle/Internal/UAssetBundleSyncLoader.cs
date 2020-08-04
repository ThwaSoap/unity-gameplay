using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePlay.Internal
{
    class UAssetBundleSyncLoader
    {
        protected UAssetBundleManager Manager;
        protected UAssetBundlePool PackageManager;
        protected UAssetBundleManifestManager ManifestManager;
        protected ULevelManager LevelManager;
        class Note
        {
            public string Dir { get; private set; }
            public string PackageName { get; private set; }
            public int Count;

            public Note(string _dir, string _packageName) 
            {
                Dir = _dir;
                PackageName = _packageName;
                Count = 1;
            }
        }

        Dictionary<string, Note> Notes = new Dictionary<string, Note>();

        /// <summary>
        /// 是否使用模块文件夹，如果使用的话，每个模块的AB文件应该放在单独的文件夹内，文件夹的名称应该是模块名称的小写
        /// </summary>
        bool IsUseModuleFolder = false;
        public UAssetBundleSyncLoader(UAssetBundleManager _manager, UAssetBundlePool _packageManager, UAssetBundleManifestManager _manifestManager)
        {
            Manager = _manager;
            PackageManager = _packageManager;
            ManifestManager = _manifestManager;
        }

        internal void SetUseModuleFolder(bool _isUseModuleFolder)
        {
            IsUseModuleFolder = _isUseModuleFolder;
        }

        internal void SetLevelManager(ULevelManager _manager) 
        {
            LevelManager = _manager;
        }


        internal UnityEngine.Object LoadAssetFromResource(Type _type, string _dir, string _packageName, string _assetName)
        {
            AssetBundle package = LoadAssetBundle(_dir, _packageName, LoadResourceText, LoadResourcePackage);

            if (package == null) return null;

            return package.LoadAsset(_assetName, _type);
        }

        /// <summary>
        /// 从资源文件夹加载一个AB资源
        /// </summary>
        /// <typeparam name="Asset"></typeparam>
        /// <param name="_dir"></param>
        /// <param name="_packageName"></param>
        /// <param name="_assetName"></param>
        /// <returns></returns>
        internal Asset LoadAssetFromResource<Asset>(string _dir, string _packageName, string _assetName) where Asset : UnityEngine.Object
        {
            AssetBundle package = LoadAssetBundle(_dir, _packageName, LoadResourceText, LoadResourcePackage);

            if (package == null) return default(Asset);

            return package.LoadAsset<Asset>(_assetName);
        }

        /// <summary>
        /// 从Resource加载AB资源
        /// </summary>
        /// <param name="_dir"></param>
        /// <param name="_packageName"></param>
        /// <param name="_levelName"></param>
        /// <param name="_mode"></param>
        internal UAsyncOperation LoadLevelFromResource(string _dir, string _packageName, string _levelName, ELoadLevelMode _mode,Action _finished)
        {
            AssetBundle package = LoadAssetBundle(_dir, _packageName, LoadResourceText, LoadResourcePackage);
            if (package == null) return null;

            var request = new List<UAssetBundleCreateRequest>();
            request.Add(new UAssetBundleCreateRequestDone(package));
            return new UAssetBundleLevelRequest(LevelManager.Container,LevelManager.Container.CreateInstance(_levelName, _mode), request, _finished);
        }

        /// <summary>
        /// 从磁盘上加载一个AB资源
        /// </summary>
        /// <typeparam name="Asset"></typeparam>
        /// <param name="_dir"></param>
        /// <param name="_packageName"></param>
        /// <param name="_assetName"></param>
        /// <returns></returns>
        internal Asset LoadAssetFromPath<Asset>(string _dir, string _packageName, string _assetName) where Asset : UnityEngine.Object
        {
            AssetBundle package = LoadAssetBundle(_dir, _packageName, LoadPathText, LoadPathPackage);

            if (package == null) return default(Asset);

            return package.LoadAsset<Asset>(_assetName);
        }

        internal UnityEngine.Object LoadAssetFromPath(Type _type, string _dir, string _packageName, string _assetName)
        {
            AssetBundle package = LoadAssetBundle(_dir, _packageName, LoadPathText, LoadPathPackage);

            if (package == null) return null;

            return package.LoadAsset(_assetName, _type);
        }

        internal UAsyncOperation LoadLevelFromPath(string _dir, string _packageName, string _levelName, ELoadLevelMode _mode,Action _finished)
        {
            AssetBundle package = LoadAssetBundle(_dir, _packageName, LoadPathText, LoadPathPackage);
            if (package == null) return null; 
            var request = new List<UAssetBundleCreateRequest>();
            request.Add(new UAssetBundleCreateRequestDone(package));
            return new UAssetBundleLevelRequest(LevelManager.Container, LevelManager.Container.CreateInstance(_levelName, _mode), request, _finished);
        }

        internal void Unload(string _dir,string _packageName,bool _isDispose) 
        {
            UnloadPackageByCount(_dir,_packageName,_isDispose);
        }

        void UnloadPackageByCount(string _dir, string _packageName, bool _isDispose, int _count=1) 
        {
            if (PackageManager.RemPackage(_packageName, _count, _isDispose))
            {
                string moduleName = UAssetBundleManifestManager.ParseModuleName(_packageName);
                string fullModuleName = GetAssetBundlePath(_dir, moduleName, moduleName);
                List<string> dependence = ManifestManager.GetDependence(fullModuleName, _packageName);
                foreach (var v in dependence)
                {
                    Unload(_dir, v, _isDispose);
                }
            }
        }

        internal void Dispose(bool _isDispose) 
        {
            foreach (var v in Notes) 
            {
                UnloadPackageByCount(v.Value.Dir,v.Value.PackageName,_isDispose,v.Value.Count);
            }
            Notes.Clear();
        }

        #region 同步AB包加载
        string LoadResourceText(string _dir)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(_dir);
            AssetBundle package = AssetBundle.LoadFromMemory(textAsset.bytes);
            TextAsset manifest = package.LoadAsset<TextAsset>("manifest");
            string text = manifest.text;
            package.Unload(true);
            return text;
        }

        AssetBundle LoadResourcePackage(string _dir)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(_dir);
            AssetBundle package = AssetBundle.LoadFromMemory(textAsset.bytes);
            return package;
        }

        protected string LoadPathText(string _dir)
        {
            AssetBundle package = AssetBundle.LoadFromFile(_dir + "." + Manager.PackageExtension);
            TextAsset manifest = package.LoadAsset<TextAsset>("manifest");
            string text = manifest.text;
            package.Unload(true);
            return text;
        }

        AssetBundle LoadPathPackage(string _dir)
        {
            return AssetBundle.LoadFromFile(_dir + "." + Manager.PackageExtension);
        }

        protected string GetAssetBundlePath(string _dir, string _moduleName, string _packageName)
        {
            if (IsUseModuleFolder)
                return string.Format("{0}/{1}/{2}", _dir, _moduleName, _packageName);
            else
                return string.Format("{0}/{1}", _dir, _packageName);
        }

        AssetBundle LoadAssetBundle(string _dir, string _packageName, Func<string, string> _manifestLoader, Func<string, AssetBundle> _packageLoader)
        {
            string moduleName = UAssetBundleManifestManager.ParseModuleName(_packageName);
            string fullPackageName = GetAssetBundlePath(_dir, moduleName, _packageName);

            // 以前的代码
            AssetBundle target = PackageManager.ApplyPackage(fullPackageName); 
            if (target != null ) return target;

            string fullModuleName = GetAssetBundlePath(_dir, moduleName, moduleName); 
            if (false == ManifestManager.Contains(fullModuleName))
            {
                //> 加载依赖关系文件
                string content = _manifestLoader(fullModuleName);
                ManifestManager.AddManifest(fullModuleName, content);
            }

            AssetBundle package = _packageLoader(fullPackageName);

            if (package != null)
            {
                PackageManager.AddPackage(fullPackageName, package);

                //> 只有在当前AB加载成功的时候，才去载入依赖文件
                List<string> dependence = ManifestManager.GetDependence(fullModuleName, _packageName);
                foreach (var v in dependence)
                {
                    LoadAssetBundle(_dir, v, _manifestLoader, _packageLoader);
                }
            }
            return package;
        }
        #endregion
    }
}