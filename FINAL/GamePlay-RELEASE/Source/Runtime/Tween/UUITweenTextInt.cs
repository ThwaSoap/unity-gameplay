using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GamePlay.Tween 
{
    [RequireComponent(typeof(Text))]
    public class UUITweenTextInt : UTween
    {
        public int From;
        public int To;
        int Current;
        Text Target;
        Func<int, string> Format;
        public override void Awake()
        {
            Target = GetComponent<Text>();
            base.Awake();
        }

        protected override void UpdateTween(float _value)
        {
            Current = (int)(From + (To - From) * _value);
            SetInt(Current);
        }

        public void SetValue(int _value) 
        {
            From = To = Current = _value;
            SetInt(_value);
        }

        public void ChangeValue(int _value) 
        {
            From = Current;
            To = _value;
            SetInt(Current);
            if (enabled == false) 
            {
                this.PlayForward(true);
            }
        }

        void SetInt(int _int) 
        {
            if (Format != null)
            {
                Target.text = Format(_int);
            }
            else
            {
                Target.text = _int.ToString();
            }
        }
    }
}