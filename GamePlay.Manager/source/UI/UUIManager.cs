using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    using Internal;
    public class UUIManager : IUIM
    {
        internal ILog Output;
        
        UAssetManager AssetMgr;
        UAssetBundleManager AssetBundleMgr;
        Dictionary<string, UUIContainer> Containers = new Dictionary<string, UUIContainer>();
        Dictionary<string, UUIInfo> Informations = new Dictionary<string, UUIInfo>();
        Dictionary<string, GameObject> Layers = new Dictionary<string, GameObject>();
        
        internal UAssetManager AssetManager => AssetMgr;
        internal UAssetBundleManager AssetBundleManager => AssetBundleMgr;

        internal UUIContainer DefaultContainer;

        internal UUIManager() 
        {
            DefaultContainer = new UUIContainer(this);
            Containers.Add(DefaultContainer.GetHashCode().ToString(), DefaultContainer); 
        }

        internal void SetLog(ILog _output) 
        {
            Output = _output;
        }

        public void Initialized(UAssetManager _assetMgr, UAssetBundleManager _assetBundleMgr) 
        {
            AssetMgr = _assetMgr;
            AssetBundleMgr = _assetBundleMgr;
        }

        #region 注册UI信息
        public void RegisterUIInfo(UUIInfo _info) 
        {
            if (Informations.ContainsKey(_info.GetName())) 
            {
                Output.Error("注册UI失败，因为{0}已经存在.", _info.GetName());
                return;
            }
            Informations.Add(_info.GetName(), _info);
        }

        public void UnregisterUIInfo(string _name) 
        {
            if (Informations.ContainsKey(_name))
            {
                //> 需要处理卸载之前必须要完成的事情
                Informations.Remove(_name);
            }
        }
        #endregion

        #region 注册层
        public void RegisterLayer(string _ownerName, GameObject _go)
        {
            if (!Layers.ContainsKey(_ownerName) && _go != null)
            {
                Layers.Add(_ownerName, _go);
            }
        }

        public void UnregisterLayer(string _ownerName)
        {
            Layers.Remove(_ownerName);
        }

        public GameObject FindLayer(string _ownerName)
        {
            return Layers[_ownerName];
        }

        public bool TryGetLayer(string _ownerName, out GameObject _outGo)
        {
            return Layers.TryGetValue(_ownerName, out _outGo);
        }
        #endregion

        public void CreateContainer(string _name) 
        {
            UUIContainer outValue;
            if (!Containers.TryGetValue(_name, out outValue))
            {
                outValue = new UUIContainer(this);
                Containers.Add(_name, outValue);
            }
        }

        public UUIContainer GetContainer(string _name="") 
        {
            if (string.IsNullOrEmpty(_name)) 
            {
                return DefaultContainer;
            }

            return Containers[_name];
        }

        public UUIRequest Open(string _name, bool _preloading = false, EOpenMode _openMode = EOpenMode.Additive)
        {
            return GetContainer().Open(_name,_preloading,_openMode);
        }

        public UUIRequest PreLoad(string _name)
        {
            return GetContainer().PreLoad(_name);
        }

        public void Close(string _name, bool _isDestroy = false)
        {
            GetContainer().Close(_name,_isDestroy);
        }

        public void Update() 
        {
            foreach (var v in Containers) {
                v.Value.Update();
            }
        }

        public void Dispose() 
        {
            AssetMgr = null;
            AssetBundleMgr = null;
            foreach (var v in Containers) 
            {
                v.Value.Dispose();
            }
            Containers.Clear();
            Layers.Clear();
            Informations.Clear();
            DefaultContainer = null;
        }

        public void TestDispose() 
        {
            foreach (var v in Containers)
            {
                v.Value.Dispose();
            }
        }

        #region 内部消息

        internal bool TryGetUIInfo(string _name, out UUIInfo _outInfo)
        {
            return Informations.TryGetValue(_name, out _outInfo);
        }

        internal bool TryGetSingleWorker(UUIInfoStaticSingle _info, out UUIContainer _container, out UUIWorker _worker)
        {
            _container = null;
            _worker = null;
            foreach (var v in Containers)
            {
                if (v.Value.TryGetWoker(_info, out _worker))
                {
                    _container = v.Value;
                    return true;
                }
            }
            return false;
        }

        internal bool TryGetContainerByWorker(UUIWorker _instance, out UUIContainer _container)
        {
            _container = null;
            foreach (var v in Containers)
            {
                if (v.Value.Contains(_instance))
                {
                    _container = v.Value;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region IUIM 接口 
        ILog IUIM.Output => Output;
        GameObject IUIM.FindLayer(string _layerName)
        {
            return Layers[_layerName];
        }
        //>场景中的UI被删除
        void IUIM.OnDestroyUI(IUI _scriptInstance)
        {
            UUIContainer container;
            if (TryGetContainerByWorker(_scriptInstance.Worker, out container)) 
            {
                container.OnWorkerDestroy(_scriptInstance.Worker);
            }
        }

        //> UI自身调用了Close
        void IUIM.OnCloseUI(IUI _scriptInstance,bool _isDestroy)
        {
            UUIContainer container;
            if (TryGetContainerByWorker(_scriptInstance.Worker, out container))
            {
                container.OnWorkerClose(_scriptInstance.Worker, _isDestroy);
            }
        }
        #endregion
    }
}
