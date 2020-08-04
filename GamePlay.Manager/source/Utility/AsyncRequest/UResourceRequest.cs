using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    public interface IAssetRequest
    {
        Texture2D texture2D { get; }
        TextAsset textAsset { get; }
        Sprite sprite { get; }
        GameObject gameObject { get; }
        AudioClip audioClip { get; }
    }

    public abstract class UResourceRequest : UAsyncOperation
    {
        public abstract Asset GetAsset<Asset>() where Asset : UnityEngine.Object;
        public abstract IAssetRequest AssetRequest { get; }
        public abstract UnityEngine.Object Asset { get; }
    }
}

