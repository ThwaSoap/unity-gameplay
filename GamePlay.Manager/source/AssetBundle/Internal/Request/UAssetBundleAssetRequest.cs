using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GamePlay.Internal
{
    class UAssetBundleAssetRequest : UResourceRequest, IAssetRequest
    {
        UnityEngine.Object Result;
        UAsyncOperation Request;
        float ValueProgress;
        public UAssetBundleAssetRequest(string _assetName, Type _type, List<UAssetBundleCreateRequest> _list, Action<UnityEngine.Object> _callback)
        {
            if (_list.Count > 0)
                this.StartCoroutine(Start(_assetName, _type, _list, _callback));
            else
                ValueProgress = 2.1f;

        } 

        IEnumerator Start(string _assetName, Type _type,List<UAssetBundleCreateRequest> _list, Action<UnityEngine.Object> _callback) 
        {
            float rate = 1f / _list.Count;
            
            foreach (var v in _list) 
            {
                yield return v;
                ValueProgress += rate;
            }

            ValueProgress = 1f;

            var header = _list[0];

            if (header.AssetBundle != null)
            {
                var request = header.AssetBundle.LoadAssetAsync(_assetName, _type);
                Request = new UAsyncOperationUnity(request);
                yield return request;
                Result = request.asset;

                _callback?.Invoke(Result);
                ValueProgress = 1.1f;
            }
            else ValueProgress = 2.1f;

            yield break;
        }

        public override float Progress 
        {
            get 
            {
                if (Request != null)
                {
                    return (ValueProgress + Request.Progress) / 2.1f;
                }
                else 
                {
                    return ValueProgress / 2.1f;
                }
            }
        }


        public override IAssetRequest AssetRequest => this;

        public override UnityEngine.Object Asset => Result;

        Texture2D IAssetRequest.texture2D => Result as Texture2D;

        TextAsset IAssetRequest.textAsset => Result as TextAsset;

        Sprite IAssetRequest.sprite => Result as Sprite;

        GameObject IAssetRequest.gameObject => Result as GameObject;

        AudioClip IAssetRequest.audioClip => Result as AudioClip;

        public override Asset GetAsset<Asset>()
        {
            return Result as Asset;
        }
    }
    class UAssetBundleAssetRequest<T> : UAssetBundleAssetRequest
        where T : UnityEngine.Object
    { 
        public UAssetBundleAssetRequest(string _assetName,  List<UAssetBundleCreateRequest> _list, Action<T> _callback)
            : base(_assetName,typeof(T),_list,(o)=> { _callback?.Invoke(o as T); }){}
    }
}
