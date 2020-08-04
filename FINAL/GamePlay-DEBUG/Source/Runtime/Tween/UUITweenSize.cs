using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Tween 
{
    [RequireComponent(typeof(RectTransform))]
    public class UUITweenSize : UTween
    {
        public Vector2 From;
        public Vector2 To; 
        RectTransform Target;
        public override void Awake()
        {
            Target = GetComponent<RectTransform>();
            base.Awake();
        }

        protected override void UpdateTween(float _value)
        {
            Target.sizeDelta = Vector2.LerpUnclamped(From,To,_value);
        }
    }
}