using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePlay.Internal
{
    class ULevelContainer
    {
        ILog Output;
        #region SEAL
        static int Handle = 1000;
        public ULevel CreateInstance(string _levelName, ELoadLevelMode _loadMode)
        {
            ULevel instance = new ULevel(_levelName, GetHandle(), _loadMode);
            Wokers.Add(instance);
            return instance;
        }


        static int GetHandle()
        {
            return ++Handle;
        }
        #endregion

        List<ULevel> Wokers = new List<ULevel>();
        List<ULevel> Useless = new List<ULevel>();

        internal void SetLog(ILog _output) 
        {
            Output = _output;
        }
        public void Dispose() 
        {
            Dispose(Wokers);
            Dispose(Useless);
            Wokers.Clear();
            Useless.Clear();
        }

        void Dispose(List<ULevel> _list) 
        {
            ULevel instance;
            while(_list.Count > 0)
            {
                instance = _list[_list.Count - 1];
                if (instance.IsBind) 
                {
                    UselessLevel(instance, instance.BindInstance);
                }
                _list.RemoveAt(_list.Count - 1);
            }
        }

        public void UnloadByName(string _levelName)
        {
            int idx = Wokers.FindIndex((v) => { return v.Name == _levelName; });
            if (idx > -1)
            {
                var instance = Wokers[idx];
                Wokers.RemoveAt(idx);
                if (instance.IsBind)
                    UnloadLevel(instance.BindInstance);
                else
                    Useless.Add(instance);
            }
        }

        #region 内部接口
        /// <summary>
        /// 获取主关卡
        /// </summary>
        /// <returns></returns>
        internal ULevel GetMainLevel()
        {
            if (Wokers.Count > 0) return Wokers[0];
            else return null;
        }

        internal bool IsWorking(ULevel _instance)
        {
            return Wokers.IndexOf(_instance) > -1;
        }

        internal void UnbindRequest(ULevel _instance)
        {
            Output.Error("释放了一个请求:{0}", _instance.Name);
            Useless.Remove(_instance);
        }

        internal bool BindRequest(ULevel _instance, UUnityLevelRequest _request)
        {
            _instance.Request = _request;
            return IsWorking(_instance);
        }

        internal void BindInstance(ULevel _instance, Scene _unityInstance)
        {
            _instance.SetBind(_unityInstance);
            if (IsWorking(_instance) == false)
            {
                UselessLevel(_instance, _unityInstance);
            }
        }

        internal void SetSingleMainLevel(ULevel _instance)
        {
            int idx = Wokers.IndexOf(_instance);
            if (idx <= 0) return;
            List<ULevel> removed = new List<ULevel>();
            for (int i = 0; i < idx; i++)
            {
                removed.Add(Wokers[i]);
            }
            Wokers.RemoveRange(0, idx);

            foreach (var v in removed)
            {
                if (v.IsBind)
                {
                    UselessLevel(v, v.BindInstance);
                }
                else
                {
                    Useless.Add(v);
                }
            }
            removed.Clear();
        }

        internal void AdditiveMainLevel(ULevel _instance)
        {
            int idx = Wokers.IndexOf(_instance);
            if (idx <= 0) return;
            Wokers.RemoveAt(idx);
            Wokers.Insert(0, _instance);
        }

        internal void ReplaceMainLevel(ULevel _instance)
        {
            int idx = Wokers.IndexOf(_instance);
            if (idx <= 0) return;
            Wokers.RemoveAt(idx);
            ULevel oldMainLevel = Wokers[0];
            Wokers.RemoveAt(0);
            Wokers.Insert(0, _instance);
            if (oldMainLevel.IsBind)
            {
                UselessLevel(oldMainLevel, oldMainLevel.BindInstance);
            }
        }

        internal void MergeToMainLevel(ULevel _instance)
        {
            int idx = Wokers.IndexOf(_instance);
            if (idx <= 0) return;

            var target = Wokers[0].BindInstance;
            var source = _instance.BindInstance;
            SceneManager.MergeScenes(source, target);
            Wokers.RemoveAt(idx);
        }

        internal void SetSingleSubLevel(ULevel _instance)
        {
            int idx = Wokers.IndexOf(_instance);
            if (idx <= 0) return;
            List<int> idxs = new List<int>();
            List<ULevel> removed = new List<ULevel>();
            for (int i = 1; i < idx; i++)
            {
                idxs.Insert(0, i);
                removed.Add(Wokers[i]);
            }

            foreach (var v in idxs) Wokers.RemoveAt(v);

            foreach (var v in removed)
            {
                if (v.IsBind)
                {
                    UselessLevel(v, v.BindInstance);
                }
                else
                {
                    Useless.Add(v);
                }
            }
            removed.Clear();
        }


        internal bool NoInactiveRequest(ULevel _instance)
        {
            int idx = Wokers.IndexOf(_instance);
            if (idx < 0) return true;
            int newIdx = Wokers.FindIndex((obj) =>
            {
                if (obj == _instance) return true;

                if (obj.IsBind == false && obj.Request == null)
                {
                    return true;
                }
                return false;
            });
            return newIdx == idx;
        }

        internal bool TryGetRequestByLoadMode(ULevel _instance, int _startIndex, ELoadLevelMode _loadMode, out ULevel _outFirstLevel)
        {
            _outFirstLevel = null;
            int idx = Wokers.IndexOf(_instance);
            if (idx < _startIndex) return true;
            int newIdx = Wokers.FindIndex(_startIndex, (obj) =>
            {
                //> 遇到自己，
                if (obj == _instance) return true;
                else if (obj.LoadMode == _loadMode) return true;
                return false;
            });
            _outFirstLevel = Wokers[newIdx];
            return newIdx == idx;
        }

        const string USELESS_NAME = "___.useless.___";
        void UnloadLevel(Scene oldScene)
        {
            Scene newScene = SceneManager.GetSceneByName(USELESS_NAME);
            if (newScene.isLoaded == false)
            {
                newScene = SceneManager.CreateScene(USELESS_NAME);
            }
            List<GameObject> reuslt = new List<GameObject>();
            oldScene.GetRootGameObjects(reuslt);
            foreach (var v in reuslt)
            {
                v.SetActive(false);
                GameObject.Destroy(v, 1f);
            }
            SceneManager.MergeScenes(oldScene, newScene);
        }

        internal void UselessLevel(ULevel _instance, Scene _unityInstance)
        {
            Useless.Remove(_instance);
            UnloadLevel(_unityInstance);
        }
        #endregion
    }
}
