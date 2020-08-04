using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    using Internal;
    public class UAsyncOperationGroup : UAsyncOperation
    {
        float PrograssValue;
        public UAsyncOperationGroup(UAsyncOperation[] _list,System.Action _finish=null)
        {
            List<UAsyncOperation> result = new List<UAsyncOperation>();
            foreach (var v in _list)
            {
                if (v != null)
                {
                    result.Add(v);
                }
            }

            if (result.Count > 0)
            {
                PrograssValue = 0;
                this.StartCoroutine(Start(result, 1f / result.Count, _finish));
            }
            else 
            {
                PrograssValue = 1.1f;
            }
        }

        IEnumerator Start(List<UAsyncOperation> _operations,float _rate, System.Action _finish) 
        {
            foreach (var v in _operations) 
            {
                yield return v;
                PrograssValue += _rate;
            }
            _finish?.Invoke();
            PrograssValue = 1.1f;
            yield break;
        }

        public override float Progress 
        {
            get 
            {
                return PrograssValue / 1.1f;
            }
        }
    }
}

