using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Tween 
{
    public class UTweenPosition : UTween
    {
        public bool IsLocal;
        public bool X=true;
        public bool Y=true;
        public bool Z=true;
        public Vector3 From;
        public Vector3 To;
        Vector3 b;
        Vector3 v;
        protected override void UpdateTween(float _value)
        { 
            if (IsLocal)
            {
                b = transform.localPosition;
                v = Vector3.LerpUnclamped(From, To, _value);
            } 
            else 
            {
                b = transform.position;
                v = Vector3.LerpUnclamped(From, To, _value);
            }
            if (!X) v.x = b.x;
            if (!Y) v.y = b.y;
            if (!Z) v.z = b.z;

            if (IsLocal)
                transform.localPosition = v;
            else
                transform.position = v;
        }
    }
}
