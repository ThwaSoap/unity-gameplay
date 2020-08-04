using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Tween 
{
    public class UTweenRotation : UTween
    {
        public Quaternion From;
        public Quaternion To;
        public bool Smooth;
        public bool IsLocal;

        protected override void UpdateTween(float _value)
        {
            Quaternion value;
            if (Smooth)
                value = Quaternion.SlerpUnclamped(From, To, _value);
            else
                value = Quaternion.LerpUnclamped(From,To,_value);

            if(IsLocal)
                transform.localRotation = value;
            else
                transform.rotation = value;
        }
    }
}

