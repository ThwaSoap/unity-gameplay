using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Tween 
{
    [RequireComponent(typeof(RectTransform))]
    public class UUITweenAnchoredPosition3D : UTween
    {
        public Vector3 From;
        public Vector3 To; 
        RectTransform Target;
        public override void Awake()
        {
            Target = GetComponent<RectTransform>();
            base.Awake();
        }

        protected override void UpdateTween(float _value)
        {
            Target.anchoredPosition3D = Vector3.LerpUnclamped(From,To,_value);
        }
    }
}