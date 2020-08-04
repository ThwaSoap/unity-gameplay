using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Internal
{
    enum EWorkerState 
    {
        Loading,//> 加载中 
        Working,//> 工作中
        Resting,//> 休息中
        Exiting //> 退出中
    }
    class UUIWorker
    {
        public UUIInfo Info;
        public IUI Script;
        public UUIRequest Request;
        public float ExitTime;
        public bool IsSceneObject { get; private set; }
        public EWorkerState State { get; private set; }
        public EWorkerState NextState { get; set; }
        public bool NextExitState { get; private set; }

        public UUIWorker(EWorkerState _state, EWorkerState _nextState, UUIInfo _info,bool _isSceneObject) 
        {
            State = _state;
            NextState = _nextState;
            Info = _info;
            IsSceneObject = _isSceneObject;
        }

        public bool SetLoaded(IUIM _mgr,GameObject _go) 
        {
            State  = EWorkerState.Resting;
            Script = _go.GetComponent<IUI>();
            Script.Worker = this;
            Script?.Loaded(_mgr,_go);
            return Script != null;
        }
        public void SetClose(bool _isDestroy) 
        {
            if (State == EWorkerState.Loading)
            {
                NextExitState = _isDestroy;
                NextState = EWorkerState.Exiting;
            }
            else if (State == EWorkerState.Working)
            {
                if (Script.ExitMode == EUIExitMode.DelayDestroy && false == IsSceneObject || _isDestroy && false == _isDestroy)
                {
                    ExitTime = Script.ExitTime;
                    SyncState(EWorkerState.Exiting);
                }
                else
                {
                    SyncState(EWorkerState.Resting);
                }
                Script.Close();
            }
            else if (State == EWorkerState.Resting) 
            {
                if (Script.ExitMode == EUIExitMode.DelayDestroy && false == IsSceneObject || _isDestroy && false == _isDestroy) 
                {
                    ExitTime = Script.ExitTime;
                    SyncState(EWorkerState.Exiting);
                }
            }
        }
        public void SetOpen() 
        {
            SyncState(EWorkerState.Working);
            Script.Open();
        }
        public void SetRest() 
        {
            SyncState(EWorkerState.Resting);
        }

        public void SetUpward() 
        {
            Script?.Upward();
        }

        public void SetDownward() 
        {
            Script?.Downward();
        }

        public void SetUnload() 
        {
            Script?.Unload();
        }

        void SyncState(EWorkerState _newState) 
        {
            State = _newState;
            NextState = _newState;
        }
    }
}

