using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Tween 
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UUITweenAlphaGroup : UTween
    {
        public float From;
        public float To;
        CanvasGroup Target;
        public override void Awake()
        {
            Target = GetComponent<CanvasGroup>();
            base.Awake();
        }

        protected override void UpdateTween(float _value)
        { 
            Target.alpha = Mathf.LerpUnclamped(From,To,_value); 
        }
    }
}