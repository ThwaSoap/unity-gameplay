using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    using Internal;

    public class UStageContainer 
    {
        Dictionary<string, IStage> Stages = new Dictionary<string, IStage>();
        float Timer = 0;
        float Interval = 0;
        public UStage Find(string _name) 
        {
            IStage outValue;
            if (Stages.TryGetValue(_name, out outValue))
                return outValue as UStage;
            else
                return null; 
        }

        public T Find<T>(string _name)
            where T : UStage
        {
            return Find(_name) as T;
        }
        public void SetUpdateInterval(float _value) 
        {
            Interval = _value;
        }
        public void RegisterStage(UStage _instance) 
        {
            if (false == Stages.ContainsKey(_instance.Name)) 
            {
                Stages.Add(_instance.Name, _instance);
            }
        }
        public void UnregisterStage(string _name)
        {
            IStage outValue;
            if (Stages.TryGetValue(_name, out outValue)) 
            {
                outValue.NextState = EStageStatus.Unload;
                if (outValue.State == EStageStatus.Loading)
                {
                    //> 这里怎么处理，回调函数怎么处理
                }
                else if (outValue.State == EStageStatus.Resting)
                {
                    //> 直接卸载已经加载完成却没有打开的舞台实例
                    outValue.Unload();
                    outValue.State = EStageStatus.Unload;
                }
                else if (outValue.State == EStageStatus.Working) 
                {
                    //> 关闭并且卸载已经打开的舞台实例
                    outValue.Close();
                    outValue.Unload();
                    outValue.State = EStageStatus.Unload; 
                }

                Stages.Remove(_name);
            }
        }
        #region 处理一个对象的业务
        public UAsyncOperation Load(string _name,Action<UStage> _completed=null) 
        {
            return LoadStage(_name, EStageStatus.Resting, _completed, null,null);
        }
        public UAsyncOperation Open(string _name,Action<UStage> _completed=null) 
        {
            return LoadStage(_name, EStageStatus.Working, _completed, (stage)=> 
            {
                stage.State = stage.NextState = EStageStatus.Working;
                stage.Open();
            }, 
            (stage) =>
            {
                stage.NextState = EStageStatus.Working;
            });
        } 
        public void Close(string _name)
        {
            IStage outValue;
            if (Stages.TryGetValue(_name, out outValue))
            {
                this.SetClose(outValue);
            }
        }
        public void Unload(string _name) 
        {
            IStage outValue;
            if (Stages.TryGetValue(_name, out outValue))
            {
                this.SetUnload(outValue);
            }
        }
        #endregion

        #region 处理一部分对象的业务
        public UAsyncOperation Load(List<string> _list,Action<UStage> _completedOne = null, Action _finished = null)
        {
            if (_list == null) return null;

            List<UAsyncOperation> result = new List<UAsyncOperation>();
            foreach (var v in _list)
            {
                result.Add(LoadStage(v, EStageStatus.Resting, _completedOne, null,null));
            }
            return new UAsyncOperationGroup(result.ToArray(), _finished);
        }
        public UAsyncOperation Open(List<string> _list,Action<UStage> _completedOne = null, Action _finished = null)
        {
            if (_list == null) return null;

            List<UAsyncOperation> result = new List<UAsyncOperation>();
            foreach (var v in _list)
            {
                result.Add(LoadStage(v, EStageStatus.Working, _completedOne, (stage) =>
                {
                    stage.State = stage.NextState = EStageStatus.Working;
                    stage.Open();
                }, 
                (stage) =>
                {
                    stage.NextState = EStageStatus.Working;
                }));
            }
            return new UAsyncOperationGroup(result.ToArray(), _finished);
        }
        public void Close(List<string> _list)
        {
            if (_list == null) return;
            foreach (var v in _list)
            {
                this.Close(v);
            }
        }
        public void Unload(List<string> _list)
        {
            if (_list == null) return;
            foreach (var v in _list) 
            {
                this.Unload(v);
            } 
        }
        #endregion

        #region 处理所有对象的业务
        public UAsyncOperation LoadAll(Action<UStage> _completedOne=null, Action _finished=null) 
        {
            List<UAsyncOperation> list = new List<UAsyncOperation>();
            foreach (var v in Stages) 
            {
                list.Add(LoadStage(v.Value, EStageStatus.Resting,_completedOne, null,null));
            }
            return new UAsyncOperationGroup(list.ToArray(),_finished);
        }
        public UAsyncOperation OpenAll(Action<UStage> _completedOne = null, Action _finished = null)
        {
            List<UAsyncOperation> list = new List<UAsyncOperation>();
            foreach (var v in Stages)
            {
                list.Add(LoadStage(v.Value, EStageStatus.Working, _completedOne, (stage)=> 
                {
                    stage.State = stage.NextState = EStageStatus.Working;
                    stage.Open();
                },
                (stage)=> 
                {
                    stage.NextState = EStageStatus.Working;
                }));
            }
            return new UAsyncOperationGroup(list.ToArray(), _finished);
        }
        public void CloseAll() 
        {
            foreach (var v in Stages) this.SetClose(v.Value);
        }
        public void UnloadAll() 
        {
            foreach (var v in Stages) this.SetUnload(v.Value);
        }

        public void Dispose() 
        {
            UnloadAll();
            Stages.Clear();
        }
        #endregion

        public void Update() 
        {
            Timer += Time.unscaledDeltaTime;
            if (Timer >= Interval) 
            {
                Timer -= Interval;
                //> 子舞台的Tick
                foreach (var v in Stages)
                {
                    Get(v.Value).Update();

                    if (v.Value.State == EStageStatus.Working)
                        v.Value.Update();
                }
            }
        }

        UStage Get(IStage _stage) 
        {
            return _stage as UStage;
        }

        #region 内维护函数

        /// <summary>
        /// 一个场景加载完毕后调用的回调函数
        /// </summary>
        /// <param name="_stage"></param>
        void OnStageLoaded(IStage _stage)
        {
            //> 需要进入到工作状态
            if (_stage.NextState == EStageStatus.Working)
            {
                _stage.State = EStageStatus.Working;
                _stage.Open();
            }
            //> 在加载期间接受到了卸载指令
            else if (_stage.NextState == EStageStatus.Unload)
            {
                _stage.State = EStageStatus.Unload;
                _stage.Unload();
            }
            else
            {
                _stage.State = EStageStatus.Resting;
            }
        }
        UAsyncOperation LoadStage(string _name, EStageStatus _init_nextState, Action<UStage> _completed, Action<IStage> _opationAtResting,Action<IStage> _optionAtLoading)
        {
            IStage outValue;
            if (Stages.TryGetValue(_name, out outValue))
            {
                return LoadStage(outValue, _init_nextState, _completed, _opationAtResting, _optionAtLoading);
            }
            return null;
        }

        UAsyncOperation LoadStage(IStage _stage, EStageStatus _next, Action<UStage> _completed, Action<IStage> _opationAtResting,Action<IStage> _optionAtLoading) 
        {
            if (_stage.State == EStageStatus.Working) return _stage.Progress;

            if (_stage.State == EStageStatus.Resting)
            {
                _opationAtResting?.Invoke(_stage);
                _completed?.Invoke(_stage as UStage);
                return _stage.Progress;
            }
            else if (_stage.State == EStageStatus.Loading)
            {
                _optionAtLoading?.Invoke(_stage);
                //_stage.NextState = EStageStatus.Resting;
                (_stage.Progress as UStageRequest).Completed += _completed;
                return _stage.Progress;
            }

            _stage.State = EStageStatus.Loading;
            _stage.NextState = _next;
            _stage.Progress = new UStageRequest(_stage, OnStageLoaded, _completed);
            return _stage.Progress;
        }
        void SetClose(IStage _stage)
        {
            if (_stage.State == EStageStatus.Loading)
            {
                _stage.NextState = EStageStatus.Resting;
            }
            else if (_stage.State == EStageStatus.Working)
            {
                _stage.Close();
                _stage.State = _stage.NextState = EStageStatus.Resting;
            }
        }

        void SetUnload(IStage _stage)
        {
            if (_stage.State == EStageStatus.Loading)
            {
                _stage.NextState = EStageStatus.Unload;
            }
            else if (_stage.State == EStageStatus.Resting)
            {
                _stage.Unload();
                _stage.State = _stage.NextState = EStageStatus.Unload;
            }
            else if (_stage.State == EStageStatus.Working)
            {
                _stage.Close();
                _stage.Unload();
                _stage.State = _stage.NextState = EStageStatus.Unload;
            }
        }
        #endregion

    }
}
