using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GamePlay.Tween 
{
    public class UUIPlayTween : UUIPlay
    {
        public int GroupID = 0;
        public int CounterType = 0;
        public bool Forward = true;

        Action Callback;
        public override void Play(Action _finished)
        {
            Callback = _finished;
            var f = GetFilter();
            var list = this.gameObject.GetComponentsInChildren<UTween>();
            UTween last = null;
            foreach (var v in list) 
            {
                if (f(v.Counter) == false) continue;

                if (Forward)
                    v.PlayForward();
                else
                    v.PlayReverse();

                if (null == last) 
                    last = v; 
                else if (v.LastTime > last.LastTime)  
                    last = v;
            }

            if (last != null)
            {
                last.OnComplete += OnComplete;
            }
            else 
            {
                Callback?.Invoke();
            }
        }

        public override void Stop()
        {
            Callback = null;
            var f = GetFilter();
            var list = this.gameObject.GetComponentsInChildren<UTween>();
            foreach (var v in list)
            {
                if (f(v.Counter) == false) continue;
                v.enabled = false;
            }   
        }

        void OnComplete(UTween _tween) 
        {
            _tween.OnComplete -= OnComplete;

            Callback?.Invoke();
            Callback = null;
        }
        #region 筛选器
        bool LoopCounter(int _compare)
        {
            return _compare < 0;
        }

        Func<int, bool> GetFilter()
        {
            if (CounterType == 0) return SingleCounter;
            else if (CounterType < 0) return LoopCounter;
            else return NumberCounter;
        }

        bool SingleCounter(int _compare)
        {
            return _compare == 0;
        }

        bool NumberCounter(int _compare)
        {
            return _compare > 0;
        }
        #endregion
    }
}

