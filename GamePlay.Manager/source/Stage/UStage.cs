using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    using Internal;

    public enum EStageStatus 
    {
        Unload,
        Loading,
        Resting,
        Working
    }
    public class UStage : UStageContainer,IStage
    {

        #region IStageInterface
        EStageStatus IStage.State { get; set; }
        EStageStatus IStage.NextState { get; set; }
        UAsyncOperation IStage.Progress { get; set; }
        UAsyncOperation IStage.Load()
        {
            return OnLoad();
        }

        void IStage.Open()
        {
            OnOpen();
        }
        void IStage.Close()
        {
            OnClose();
        }

        void IStage.Unload()
        {
            OnUnload();
        }
        #endregion

        #region 派生函数
        public string Name { get; private set; }
        public UStage(string _name) { Name = _name; }
        protected virtual UAsyncOperation OnLoad() { return null; }
        protected virtual void OnClose() { }
        protected virtual void OnOpen() { }
        protected virtual void OnUnload() { }
        #endregion
    }
}