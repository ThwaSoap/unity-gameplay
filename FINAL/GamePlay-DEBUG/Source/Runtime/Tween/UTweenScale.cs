using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Tween 
{
    public class UTweenScale : UTween
    {
        public Vector3 From;
        public Vector3 To; 
        protected override void UpdateTween(float _value)
        { 
            transform.localScale = Vector3.LerpUnclamped(From,To,_value); 
        }
    }
}

