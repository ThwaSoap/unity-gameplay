using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Tween 
{
    [RequireComponent(typeof(MaskableGraphic))]
    public class UUITweenAlpha : UTween
    {
        public float From;
        public float To;
        MaskableGraphic Target;
        public override void Awake()
        {
            Target = GetComponent<MaskableGraphic>();
            base.Awake();
        }

        protected override void UpdateTween(float _value)
        {
            var c = Target.color;
            c.a = Mathf.LerpUnclamped(From,To,_value);
            Target.color = c;
        }
    }
}