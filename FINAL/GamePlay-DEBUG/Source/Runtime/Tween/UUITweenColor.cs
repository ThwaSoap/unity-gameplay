using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Tween 
{
    [RequireComponent(typeof(MaskableGraphic))]
    public class UUITweenColor : UTween
    {
        public Color From = Color.white;
        public Color To = Color.white;
        MaskableGraphic Target;
        public override void Awake()
        {
            Target = GetComponent<MaskableGraphic>();
            base.Awake();
        }

        protected override void UpdateTween(float _value)
        { 
            Target.color = Color.LerpUnclamped(From,To,_value);
        }
    }
}