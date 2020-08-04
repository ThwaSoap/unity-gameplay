using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    public class UAsyncOperationUnity : UAsyncOperation
    {
        AsyncOperation UnityOperation;

        public UAsyncOperationUnity(AsyncOperation _unityOpeartion)
        {
            UnityOperation = _unityOpeartion;
        }

        public override float Progress 
        {
            get 
            {
                return UnityOperation != null ? UnityOperation.progress : 1f;
            }
        }
    }
}
