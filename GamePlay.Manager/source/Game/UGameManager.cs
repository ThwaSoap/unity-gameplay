using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    using Internal;
     

    public sealed class UGameManager : MonoBehaviour, ILog
    {
        public bool IsEnableLog = true;
        /// <summary>
        /// 游戏的编号
        /// </summary>
        public string GameID;
        /// <summary>
        /// 资源包的后缀名
        /// </summary>
        public string AssetBundleExtension = "bytes";
        /// <summary>
        /// 是否释放资源在退出时
        /// </summary>
        public bool IsDisposeAssetBundleInQuit = true;

        public float StageUpdateInterval = 0;

        public ULevelManager LevelMgr { get; private set; } = new ULevelManager();
        public UAssetBundleManager AssetBundleMgr { get; private set; } = UAssetBundlePool.CreateManager();
        public UAssetManager AssetMgr { get; private set; } = new UAssetManager();
        public UDataManager DataMgr { get; private set; } = new UDataManager();
        public UStageContainer StageMgr { get; private set; } = new UStageContainer();

        public UUIManager UIMgr { get; private set; } = new UUIManager();

        private UCommandManager Commander = new UCommandManager();

        private UStartupManager Starter = new UStartupManager();
         
        IGameInstance Instance;
        public void Awake()
        {
            DataMgr.SetLog(this);
            Commander.SetLog(this);
            UIMgr.SetLog(this);
            //>========================
            Starter.Initialized(this);
            AssetMgr.Initialized(LevelMgr);
            AssetBundleMgr.Initialized(LevelMgr, AssetBundleExtension);
            UIMgr.Initialized(AssetMgr,AssetBundleMgr);
            StageMgr.SetUpdateInterval(StageUpdateInterval);
            DontDestroyOnLoad(gameObject);
            Starter.RegisterOptionObject(gameObject);
            Starter.RegisterOptionByChildren(gameObject);
            Instance=GetComponent<IGameInstance>();
        }

        public IEnumerator Start() 
        {
            yield return Instance?.StartInstance(this);
        }

        public IEnumerator Open(object _params) 
        {
            Starter.InitStartup();
            yield return Starter.LoadOptions();
            if (Instance != null)
            {
                yield return Instance.Open(this, _params);
            }
        }

        public void Close() 
        {
            Instance?.Close(this);
            Starter.UnloadOptions();
        }

        public void Update()
        {
            StageMgr.Update();
            UIMgr.Update();
        }

        public void OnDestroy()
        {
            try
            {
                StageMgr.Dispose();
                UIMgr.Dispose();
                LevelMgr.Dispose();
                AssetBundleMgr.Dispose(IsDisposeAssetBundleInQuit);
                DataMgr.Dispose();
                Commander.Dispose();
            }
            catch{}
        }

        /// <summary>
        /// 可选过程，
        /// 在解析命令的过程中，如果你有自定义的类型需要处理，那么可以使用这个函数实现
        /// </summary>
        /// <param name="_customTypes">所有可解析的类型</param>
        /// <param name="_onCustomConvertCallback">假设解析类型与内置类型不配对时，会将数据交给这个函数处理，</param>
        public void InitCommander(List<Type> _customTypes, Func<Type, string, object> _onCustomConvertCallback) 
        {
            Commander.CustomTypes = _customTypes;
            Commander.OnCustomConvertCallback = _onCustomConvertCallback;
        }

        #region Commander
        public void RegisterCmd(Type _classType) 
        {
            Commander.RegisterCmd(_classType);
        }

        public void UnregisterCmd(Type _classType)
        {
            Commander.UnregisterCmd(_classType);
        }

        public void Exc(string _content) 
        {
            Commander.Exc(_content);
        }
        #endregion

        #region Starter
        public void RegisterOptionByChildren(GameObject _go)
        {
            Starter.RegisterOptionByChildren(_go);
        }

        public void RegisterOptionTypes(Type _type)
        {
            Starter.RegisterOptionTypes(_type);
        }

        public void RegisterOptionObject(GameObject _go)
        {
            Starter.RegisterOptionObject(_go);
        }
        #endregion

        #region Output
        void ILog.Log(string _message)
        {
            if (!IsEnableLog) return;
            Debug.Log(_message);
        }

        void ILog.Warning(string _message)
        {
            if (!IsEnableLog) return;
            Debug.LogWarning(_message);
        }

        void ILog.Error(string _message)
        {
            if (!IsEnableLog) return;
            Debug.LogError(_message);
        }

        void ILog.Log(string _message, params object[] _args)
        {
            if (!IsEnableLog) return;
            Debug.LogFormat(_message, _args);
        }

        void ILog.Warning(string _message, params object[] _args)
        {
            if (!IsEnableLog) return;
            Debug.LogWarningFormat(_message, _args);
        }

        void ILog.Error(string _message, params object[] _args)
        {
            if (!IsEnableLog) return;
            Debug.LogErrorFormat(_message, _args);
        }
        #endregion
    }
}