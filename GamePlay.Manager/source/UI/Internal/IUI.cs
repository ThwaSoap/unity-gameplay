using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Internal 
{
    interface IUIM
    {
        ILog Output { get; }
        /// <summary>
        /// 找到自己应该挂在到的节点
        /// </summary>
        /// <param name="_layerName"></param>
        /// <returns></returns>
        GameObject FindLayer(string _layerName);

        /// <summary>
        /// 由View发起的Destroy事件
        /// </summary>
        /// <param name="_scriptInstance"></param>
        void OnDestroyUI(IUI _scriptInstance);
        /// <summary>
        /// 由View发起的Close事件
        /// </summary>
        /// <param name="_scriptInstance"></param>
        void OnCloseUI(IUI _scriptInstance,bool _isDestroy);
    }

    interface IUI
    {
        bool CanDestroy { get; }
        float ExitTime { get; }
        EUIExitMode ExitMode { get; }

        UUIWorker Worker { get; set; }

        GameObject Root { get; }
        void Loaded(IUIM _iuim, GameObject _root);
        //void SetModel(UDataModel _model);
        void Open();
        void Upward();
        void Downward();
        void Close();
        void Unload();
    }
}

