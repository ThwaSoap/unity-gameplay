using System;
using System.Collections;
using UnityEngine;

namespace GamePlay.Internal
{
    class UResourceRequestUnity : UResourceRequest,IAssetRequest
    { 
        ResourceRequest Request;
        float Last = 0;

        public UResourceRequestUnity(string _path,Type _type,Action<UnityEngine.Object> _finish) 
        {
            this.StartCoroutine(Start(_path,_type,_finish));
        }

        IEnumerator Start(string _path, Type _type, Action<UnityEngine.Object> _finish) 
        {
            Request = Resources.LoadAsync(_path, _type);
            yield return Request;
            _finish?.Invoke(Request.asset);
            Last = 0.1f;
        }

        public override float Progress 
        { 
            get
            {
                return (Request.progress + Last) / 1.1f;
            } 
        }

        public override IAssetRequest AssetRequest => this;

        public override UnityEngine.Object Asset => Request.asset;

        Texture2D IAssetRequest.texture2D => Request.asset as Texture2D;

        TextAsset IAssetRequest.textAsset => Request.asset as TextAsset;

        Sprite IAssetRequest.sprite => Request.asset as Sprite;

        GameObject IAssetRequest.gameObject => Request.asset as GameObject;

        AudioClip IAssetRequest.audioClip => Request.asset as AudioClip;

        public override Asset GetAsset<Asset>()
        {
            return Request.asset as Asset;
        }
    }

    class UResourceRequestUnity<T> : UResourceRequestUnity
        where T : UnityEngine.Object
    {
        public UResourceRequestUnity(string _path, Action<T> _finish)
            :base(_path,typeof(T),(obj)=> { _finish?.Invoke(obj as T); })
        {}
    }
}
