using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GamePlay.Internal 
{
    abstract class UAssetBundleCreateRequest : UAsyncOperation
    {
        public abstract AssetBundle AssetBundle { get; }
    }

    class UAssetBundleCreateRequestDone : UAssetBundleCreateRequest 
    {
        AssetBundle Instance;
        public override AssetBundle AssetBundle { get{ return Instance; } }

        public UAssetBundleCreateRequestDone(AssetBundle _instance) 
        {
            Instance = _instance;
            Progress = 1f;
        }
    }

    class UAssetBundleCreateRequestUnity : UAssetBundleCreateRequest 
    {
        AssetBundleCreateRequest Request;
        float LastProgress;
        public override AssetBundle AssetBundle => Request.assetBundle;

        public UAssetBundleCreateRequestUnity(AssetBundleCreateRequest _request,string _fullPath,Action<string, AssetBundle> _completed) 
        {
            Request = _request; 
            this.StartCoroutine(Start(_fullPath, _completed));
        }

        IEnumerator Start(string _object,Action<string, AssetBundle> _completed) 
        {
            yield return Request;
            _completed?.Invoke(_object,Request.assetBundle);
            LastProgress = 0.1f;
        }

        public override float Progress { get { return (Request.progress + LastProgress) / 1.1f; } }
    }
}
