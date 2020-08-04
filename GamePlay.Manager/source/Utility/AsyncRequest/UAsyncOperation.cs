using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    public abstract class UAsyncOperation : IEnumerator
    {
        object IEnumerator.Current => null; 
        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }
        void IEnumerator.Reset(){}
        
        public virtual bool IsDone { get { return Progress >= 1f;  } }
        public virtual float Progress { get; protected set; }
    }

    public class UAsyncOperationDone : UAsyncOperation 
    {
        public override float Progress { get { return 1; } }
    }

    public class UAsyncOperationFuncFloat : UAsyncOperation 
    {
        Func<float> Callback;

        public UAsyncOperationFuncFloat(Func<float> _callback) { Callback = _callback; }
        public override float Progress 
        {
            get 
            {
                if (Callback == null) return 1f;
                else return Callback();
            }
        }
    }

    public class UAsyncOperationFuncBool : UAsyncOperation
    {
        Func<bool> Callback;

        public UAsyncOperationFuncBool(Func<bool> _callback) { Callback = _callback; }
        public override bool IsDone 
        {
            get 
            {
                if (Callback == null) return true;
                else return Callback();
            }
        }
    }
}
