using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GamePlay.Internal
{
    /// <summary>
    /// 通过异步释放UI，这么做是为了在UIContainer.Update不工作之后，仍然能使UI正常
    /// 的完成释放工作。
    /// </summary>
    class UUIDisposeRequest
    {
        public UUIDisposeRequest(UUIWorker[] _array, Action<UUIInfo> _unloadCallback) 
        {
            foreach (var v in _array) 
            {
                v.SetClose(true); 
                this.StartCoroutine(Start(v, _unloadCallback));
            }
        }

        IEnumerator Start(UUIWorker _worker, Action<UUIInfo> _unloadCallback) 
        {
            //> 等待UI加载完成
            yield return _worker.Request;

            //> 如果这个UI有实例脚本
            if (_worker.Script != null)
            {
                yield return new UAsyncOperationFuncBool(() => _worker.Script.CanDestroy);

                if (false == _worker.IsSceneObject) 
                {
                    GameObject.Destroy(_worker.Script.Root);
                }

                _unloadCallback(_worker.Info);
            }
        }
    }
}
