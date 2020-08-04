using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GamePlay.Internal 
{
    class UStageRequest : UAsyncOperation
    {
        UAsyncOperation Request;

        public event Action<UStage> Completed;

        public UStageRequest(IStage _stage,Action<IStage> _finished,Action<UStage> _completed) 
        {
            Completed += _completed;
            this.StartCoroutine(Start(_stage, _finished));
        }

        IEnumerator Start(IStage _stage, Action<IStage> _finished) 
        {
            Request = _stage.Load();
            yield return Request;
            Request = new UAsyncOperationDone();
            _finished?.Invoke(_stage);
            yield return new WaitForEndOfFrame();
            Completed?.Invoke(_stage as UStage);
            Completed = null;
        }

        public override float Progress 
        { 
            get 
            {
                return Request == null ? 0f : Request.Progress;
            } 
        }
    }
}
