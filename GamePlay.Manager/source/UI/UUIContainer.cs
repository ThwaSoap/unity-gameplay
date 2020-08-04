using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace GamePlay
{
    using Internal;
    public enum EOpenMode 
    {
        Single,
        Additive
    }

    public class UUIContainer
    {
        ILog Output 
        { 
            get 
            {
                return Manager.Output;
            } 
        }
        UUIManager Manager;
        List<UUIWorker> Container = new List<UUIWorker>();
        List<UUIWorker> WorkLine = new List<UUIWorker>();
        public UUIContainer(UUIManager _manager)
        {
            Manager = _manager;
        }

        public UUIRequest Open(string _name, bool _preloading=false, EOpenMode _openMode = EOpenMode.Additive)
        {
            return Load(_name,_preloading,_openMode);
        }

        public UUIRequest PreLoad(string _name) 
        {
            return Load(_name, true, EOpenMode.Additive);
        }

        public UUIView Find(string _name) 
        {
            UUIWorker worker = Container.Find((v) => { return v.Info.GetName() == _name; });
            return worker.Script as UUIView;
        }

        public T Find<T>(string _name) where T : UUIView
        {
            return Find(_name) as T;
        }

        public void Close(string _name,bool _isDestroy=false) 
        {
            UUIWorker worker = Container.Find((v)=> { return v.Info.GetName() == _name; });
            OnWorkerClose(worker,_isDestroy);
        }

        public void Update()
        {
            lock (Container) 
            {
                for (int i = 0; i < Container.Count; i++)
                {
                    var worker = Container[i];
                    if (worker.State == EWorkerState.Exiting) 
                    {
                        if (worker.ExitTime < 0)
                        {
                            if (worker.Script.CanDestroy)
                            {
                                UnloadWorker(worker, worker.Script.Root);
                                i--;
                            }
                        }
                        else 
                        {
                            worker.ExitTime -= Time.unscaledDeltaTime;
                        }
                    }
                }
            }
        }

        public void Dispose() 
        {
            var result = Container.ToArray();
            Container.Clear();
            WorkLine.Clear();
            new UUIDisposeRequest(result, UnloadResourceByInfo);
        }


        #region 内部函数
        UUIRequest Load(string _name, bool _preloading, EOpenMode _openMode) 
        {
            UUIWorker outWorker;
        //LOADING_AGAIN:
            if (TryGetWoker(_name, out outWorker))
            {
                //> 已经加载完毕了
                if (outWorker.State == EWorkerState.Loading || outWorker.State == EWorkerState.Working)
                {
                    return outWorker.Request;
                }
                else
                {
                    //> 这段代码是不会发生的
                    /*if (null == outWorker.Script)
                    {
                        RemoveFromContainer(outWorker);
                        UnloadResourceByInfo(outWorker.Info);
                        goto LOADING_AGAIN;
                    }*/

                    if (_preloading)
                        outWorker.SetRest();
                    else
                    {
                        WorkLine.Add(outWorker);
                        if (WorkLine.Count > 1)
                        {
                            WorkLine[WorkLine.Count - 2].SetDownward();
                        }
                        outWorker.SetOpen();
                        outWorker.SetUpward();
                    }
                    return outWorker.Request;
                }
            }
            else
            {
                UUIInfo outUIInfo;
                if (Manager.TryGetUIInfo(_name, out outUIInfo) == false)
                {
                    Output.Error("没有找到 {0} 的信息，加载UI失败！", _name);
                    return null;
                }

                var worker = LoadUI(outUIInfo, _preloading ? EWorkerState.Resting : EWorkerState.Working, (WORKER, TEMPLATE) =>
                { 
                    //> 加载完成，但是资源不存在
                    if (TEMPLATE == null)
                    {
                        RemoveFromContainer(WORKER);
                        UnloadResourceByInfo(WORKER.Info);
                        Output.Error("UI对象为空:{0}", outUIInfo.GetName());
                        return;
                    }

                    //> 在加载期间调用了Dispose
                    if (Container.Contains(WORKER) == false)
                    {
                        if (false == WORKER.Info.IsSceneObject) 
                        {
                            GameObject.Destroy(TEMPLATE);
                        }
                        UnloadResourceByInfo(WORKER.Info);
                        Output.Warning("不存在的容器实例:{0}",WORKER.Info.GetName());
                        return;
                    }

                    if (false == WORKER.Info.IsSceneObject) 
                    {
                        GameObject.DontDestroyOnLoad(TEMPLATE);
                    }

                    //> 对象身上没有挂在UUI脚本
                    if (false == WORKER.SetLoaded(Manager, TEMPLATE))
                    {
                        Output.Error("在UI对象{0}身上没有发现UUI脚本对象", _name); 
                        UnloadWorker(WORKER,TEMPLATE);
                        return;
                    }

                    WORKER.Request.InitScript(TEMPLATE.GetComponent<UUIView>());

                    //> 处理下一个环节的工作
                    if (WORKER.NextState == EWorkerState.Working)
                    {
                        WORKER.SetOpen();
                        if (WorkLine[WorkLine.Count - 1] != WORKER)
                            WORKER.SetDownward();
                        else
                            WORKER.SetUpward();
                    }

                    //> 加载期间被设置了退出
                    else if (WORKER.NextState == EWorkerState.Exiting)
                    {
                        WORKER.SetClose(WORKER.NextExitState); 
                    }
                });

                if (_preloading == false && WorkLine.Count > 1)
                {
                    WorkLine[WorkLine.Count - 2].SetDownward();
                }

                //> 根据工作模式，清理工作队列
                if (_preloading == false && _openMode == EOpenMode.Single)
                {
                    int idx = WorkLine.FindIndex((v) => worker == v);

                    if (idx > 0)
                    {
                        List<UUIWorker> temp = new List<UUIWorker>();
                        for (int i = 0; i < idx; i++)
                        {
                            temp.Insert(0, WorkLine[i]);
                        }
                        WorkLine.RemoveRange(0, idx);

                        //> 处理需要下岗的工人
                        foreach (var v in temp)
                        {
                            v.SetClose(false);
                        }
                        temp.Clear();
                    }
                }
                return worker?.Request;
            }
        }

        void RemoveFromContainer(UUIWorker _worker) 
        {
            Container.Remove(_worker);
            WorkLine.Remove(_worker);
        }

        void AddedToContainer(UUIWorker _worker, EWorkerState _nextState) 
        {
            Container.Add(_worker);
            if (_nextState == EWorkerState.Working) 
            {
                WorkLine.Add(_worker);
            }
        }
        

        UUIWorker LoadUI(UUIInfo _info, EWorkerState _nextState, Action<UUIWorker,GameObject> _callback) 
        {
            if (_info is UUIInfoResource) 
                return LoadFromResource(_info as UUIInfoResource, _nextState, _callback); 
            else if (_info is UUIInfoAssetBundle) 
                return LoadFromAssetBundle(_info as UUIInfoAssetBundle, _nextState, _callback); 
            else if (_info is UUIInfoStaticSingle) 
                return LoadFromStaticSingle(_info as UUIInfoStaticSingle, _nextState, _callback); 
            else if (_info is UUIInfoStaticTemplate) 
                return LoadFromStaticTemplate(_info as UUIInfoStaticTemplate, _nextState, _callback); 
            else 
            {
                //> 交给别人处理
                return null;
            }
        }

        /// <summary>
        /// 从Resource文件夹下加载UI
        /// </summary>
        /// <param name="_info">UI的信息</param>
        /// <param name="_callback">完成后的回调函数</param>
        /// <returns></returns>
        UUIWorker LoadFromResource(UUIInfoResource _info, EWorkerState _nextState, Action<UUIWorker, GameObject> _callback) 
        {
            UUIWorker worker = new UUIWorker(EWorkerState.Loading,_nextState,_info, _info.IsSceneObject);
            AddedToContainer(worker,_nextState);
            var request = Manager.AssetManager.Loader.LoadAssetAsync<GameObject>(_info.DIR);
            worker.Request = new UUIRequestBy(request, worker, _callback);
            return worker;
        }

        /// <summary>
        /// 从AssetBundle中加载UI
        /// </summary>
        /// <param name="_info">UI的信息</param>
        /// <param name="_callback">完成后的回调函数</param>
        /// <returns></returns>
        UUIWorker LoadFromAssetBundle(UUIInfoAssetBundle _info, EWorkerState _nextState, Action<UUIWorker, GameObject> _callback) 
        {
            UAssetBundleLoaderProxyManager proxyManager = Manager.AssetBundleManager[_info.GetLoaderName()]; ;

            if (proxyManager == null) 
            {
                Output.Error("从AssetBundle加载{0}.{1}UI失败，加载器{2}不存在.",_info.PackageName,_info.AssetName,_info.GetLoaderName());
                return null;
            }

            UAssetBundleLoaderProxy proxy;
            if (string.IsNullOrEmpty(_info.ProxyName))
                proxy = proxyManager.GetDefaultProxy();
            else
                proxy = proxyManager.GetProxy(_info.ProxyName);

            if (proxy == null) 
            {
                Output.Error("从AssetBundle加载{0}.{1}UI失败，没有有效的加载器代理组件{2}.", _info.PackageName, _info.AssetName, _info.ProxyName);
                return null;
            }

            var request = proxy.GetLoader(_info.PackageName).LoadAssetAsync<GameObject>(_info.AssetName);

            UUIWorker worker = new UUIWorker(EWorkerState.Loading, _nextState, _info,_info.IsSceneObject);
            AddedToContainer(worker, _nextState);
            worker.Request = new UUIRequestBy(request, worker, _callback);
            return worker;
        }

        /// <summary>
        /// 以静态单例模式加载UI
        /// </summary>
        /// <param name="_info"></param>
        /// <param name="_callback"></param>
        /// <returns></returns>
        UUIWorker LoadFromStaticSingle(UUIInfoStaticSingle _info, EWorkerState _nextState, Action<UUIWorker, GameObject> _callback) 
        {
            if (null == _info.Instance) return null;

            UUIContainer container;
            UUIWorker worker;
            if (Manager.TryGetSingleWorker(_info, out container, out worker))
            {
                container.RemoveFromContainer(worker);
                Container.Add(worker);
                if (worker.State == EWorkerState.Working)
                {
                    WorkLine.Add(worker);
                }
            }
            else 
            {
                worker = new UUIWorker(EWorkerState.Resting,_nextState,_info,_info.IsSceneObject);
                worker.Request = new UUIRequestByDone(_info.Instance);
                AddedToContainer(worker, _nextState);
                _callback(worker, _info.Instance);
            }
            return worker;
        }

        /// <summary>
        /// 从静态模板中加载UI
        /// </summary>
        /// <param name="_info">UI的信息</param>
        /// <param name="_callback">完成时回调函数</param>
        /// <returns></returns>
        UUIWorker LoadFromStaticTemplate(UUIInfoStaticTemplate _info, EWorkerState _nextState, Action<UUIWorker, GameObject> _callback) 
        {
            if (_info.Instance == null) return null;
            UUIWorker worker = new UUIWorker(EWorkerState.Resting,_nextState,_info,_info.IsSceneObject);
            var go = GameObject.Instantiate(_info.Instance);
            worker.Request = new UUIRequestByDone(go);
            AddedToContainer(worker, _nextState);
            _callback(worker, go);
            return worker;
        }

        void UnloadWorker(UUIWorker _worker,GameObject _go) 
        {
            _worker.SetUnload();
            if (false == _worker.IsSceneObject) 
            {
                GameObject.Destroy(_go);
            }
            RemoveFromContainer(_worker);
            UnloadResourceByInfo(_worker.Info);
        }

        void UnloadResourceByInfo(UUIInfo _info) 
        {
            if (_info is UUIInfoAssetBundle) 
            {
                UUIInfoAssetBundle info = _info as UUIInfoAssetBundle;

                UAssetBundleLoaderProxyManager proxyManager = Manager.AssetBundleManager[info.GetLoaderName()]; ;

                if (proxyManager == null) return;

                UAssetBundleLoaderProxy proxy;
                if (string.IsNullOrEmpty(info.ProxyName))
                    proxy = proxyManager.GetDefaultProxy();
                else
                    proxy = proxyManager.GetProxy(info.ProxyName);

                proxy?.GetLoader(info.PackageName).UnloadAsset(false);
            }
        }

        bool TryGetWoker(string _name, out UUIWorker _worker) 
        {
            _worker = Container.Find((v)=> { return v.Info.GetName() == _name; });
            return _worker != null;
        }

        internal bool TryGetWoker(UUIInfoStaticSingle _info, out UUIWorker _worker) 
        {
            _worker = Container.Find((v) => { return v.Info == _info; });
            return _worker != null;
        }

        internal bool Contains(UUIWorker _worker) 
        {
            return Container.Contains(_worker);
        }

        internal void OnWorkerClose(UUIWorker _worker, bool _isDestroy)
        {
            UUIWorker upward = null;
            if (_worker != null)
            {
                //> 处理工人状态
                _worker.SetClose(_isDestroy);

                if (WorkLine.Count == 0) return;

                //> 处理工作队列
                if (WorkLine.Count > 1 && WorkLine[WorkLine.Count - 1] == _worker)
                {
                    upward = WorkLine[WorkLine.Count - 2];
                }
                WorkLine.Remove(_worker); 
                upward?.SetUpward();
            }
        }

        internal void OnWorkerDestroy(UUIWorker _worker)
        {
            if (_worker.State == EWorkerState.Working) 
            {
                _worker.SetClose(false);
            }
            _worker.SetUnload();
            RemoveFromContainer(_worker);
            UnloadResourceByInfo(_worker.Info);
        }
        #endregion
    }
}
