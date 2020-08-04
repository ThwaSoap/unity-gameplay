using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Tween 
{
    public class UTweenEulerAngles : UTween
    {
        public Vector3 From;
        public Vector3 To;
        public bool IsLocal;
        public bool Smooth;

        protected override void UpdateTween(float _value)
        {
            Vector3 value;
            if(Smooth)
                value = Vector3.LerpUnclamped(From,To,_value);
            else
                value = Vector3.SlerpUnclamped(From,To,_value);

            if (IsLocal)
                transform.localEulerAngles = value;
            else
                transform.eulerAngles = value;

        }
    }
}

