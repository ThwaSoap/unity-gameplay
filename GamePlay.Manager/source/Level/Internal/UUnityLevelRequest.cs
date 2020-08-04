using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace GamePlay.Internal
{
    class UUnityLevelRequest : UAsyncOperation
    { 
        float LevelProgress;

        public override float Progress { get { return LevelProgress / 1.1f; } }
        internal UUnityLevelRequest(ULevelContainer _container, ULevel _instance,Action _finished) 
        { 
            this.StartCoroutine(Start(_container, _instance, _finished));
        }

        IEnumerator Start(ULevelContainer _container, ULevel _instance, Action _finished) 
        {
            if (_container.BindRequest(_instance,this)==false) 
            {
                LevelProgress = 1.1f;
                _container.UnbindRequest(_instance);
                yield break;
            }
            if (Application.CanStreamedLevelBeLoaded(_instance.Name)) 
            { 
                var request = SceneManager.LoadSceneAsync(_instance.Name, LoadSceneMode.Additive);
                var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                while (false==request.isDone)
                {
                    yield return new WaitForEndOfFrame();
                    LevelProgress = request.progress;
                }

                yield return new WaitForEndOfFrame();
                
                switch (_instance.LoadMode) 
                {
                    case ELoadLevelMode.SingleMainLevel:
                        _container.BindInstance(_instance, scene);
                        _container.SetSingleMainLevel(_instance); 
                        break;
                    case ELoadLevelMode.AdditiveMainLevel:
                        yield return CheckoutRequest(_container, _instance);
                        _container.BindInstance(_instance, scene);
                        _container.AdditiveMainLevel(_instance);
                        break;
                    case ELoadLevelMode.ReplaceMainLevel:
                        yield return CheckoutRequest(_container, _instance);
                        _container.BindInstance(_instance, scene);
                        _container.ReplaceMainLevel(_instance);
                        break;
                    case ELoadLevelMode.MergeToMainLevel:
                        yield return CheckoutRequest(_container, _instance);
                        _container.BindInstance(_instance, scene);
                        _container.MergeToMainLevel(_instance);
                        break;
                    case ELoadLevelMode.AdditiveSubLevel:
                        _container.BindInstance(_instance,scene);
                        break;
                    case ELoadLevelMode.SingleSubLevel:
                        yield return CheckoutRequest(_container, _instance);
                        _container.BindInstance(_instance,scene);
                        _container.SetSingleSubLevel(_instance);
                        break;
                }

                _finished?.Invoke();
                yield return new WaitForEndOfFrame();
            }
            LevelProgress = 1.1f;
            yield break;
        }

        IEnumerator CheckoutRequest(ULevelContainer _container,ULevel _instance) 
        {
            //> 检测前方是否有为激活的请求
            while (_container.NoInactiveRequest(_instance) == false)
            {
                yield return new WaitForEndOfFrame();
            }
            ULevel outFirst;
            while (_container.TryGetRequestByLoadMode(_instance, 1, ELoadLevelMode.SingleMainLevel, out outFirst) == false)
            {
                yield return outFirst;
            }
            while (_container.TryGetRequestByLoadMode(_instance, 1, ELoadLevelMode.ReplaceMainLevel, out outFirst) == false)
            {
                yield return outFirst;
            }
            while (_container.TryGetRequestByLoadMode(_instance, 1, ELoadLevelMode.AdditiveMainLevel, out outFirst) == false)
            {
                yield return outFirst;
            }
            while (_container.TryGetRequestByLoadMode(_instance, 1, ELoadLevelMode.MergeToMainLevel, out outFirst) == false)
            {
                yield return outFirst;
            }
        }
    }

    
}
