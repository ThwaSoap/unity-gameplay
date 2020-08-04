using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Internal
{

    class UAssetBundleLevelRequest : UAsyncOperation 
    {
        float AssetBundleProgress;
        float LevelProgress;

        public override float Progress 
        { 
            get
            {
                return (AssetBundleProgress + LevelProgress) / 2.1f;
            } 
        }

        public UAssetBundleLevelRequest(ULevelContainer _container, ULevel _instance, List<UAssetBundleCreateRequest> _lists,Action _finished) 
        {
            this.StartCoroutine(Start(_container, _instance, _lists, _finished));
        }

        IEnumerator Start(ULevelContainer _container, ULevel _instance, List<UAssetBundleCreateRequest> _lists,Action _finished) 
        {
            AssetBundleProgress = 0;
            if (_lists.Count > 0)
            {
                float rate = 1f / _lists.Count;
                foreach (var v in _lists)
                {
                    yield return v;
                    AssetBundleProgress += rate;
                }
            }
            AssetBundleProgress = 1;
            if (_lists[0].AssetBundle.isStreamedSceneAssetBundle) 
            {
                var request = new UUnityLevelRequest(_container, _instance, _finished);

                while (request.IsDone == false) 
                {
                    yield return new WaitForEndOfFrame();
                    LevelProgress = request.Progress;
                }
            }
            LevelProgress = 1.1f;
            yield break;
        }
    }
}
