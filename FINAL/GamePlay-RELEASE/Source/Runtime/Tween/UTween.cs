using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GamePlay.Tween 
{
    public enum EMode
    {
        /// <summary>
        /// 一次
        /// </summary>
        Once,
        /// <summary>
        /// 来回
        /// </summary>
        Trip
    }

    public enum ETrack
    {
        Forward,
        Reverse
    }

    public abstract class UTween : MonoBehaviour
    {
        public EMode Mode;
        //public ETrack DefaultTrack;
        /// <summary>
        /// Counter:number < 0 = loop
        /// Counter:number = 0 = once
        /// Counter:number > 0 = repeat times
        /// </summary>
        public int Counter;
        /// <summary>
        /// Group:number  = 0 = Single
        /// Group:number != 0 = GroupNumber
        /// </summary>
        public int Group;
        /// <summary>
        /// 在Awake时自动播放
        /// </summary>
        public bool AutoPlayAwake;
        /// <summary>
        /// 在完成后销毁脚本对象
        /// </summary>
        public bool AutoKill;
        /// <summary>
        /// 在完成后取消激活GameObject对象
        /// </summary>
        public bool AutoInactive; 

        /// <summary>
        /// 在Enable的时候重置
        /// </summary>
        public bool ResetEnable;
        /// <summary>
        /// 在调用PlayXXX的时候重置
        /// </summary>
        public bool ResetPlayCall;
        /// <summary>
        /// 如果Counter < 0,那么Complete 将会在UTween.OnDestroy的时候调用
        /// </summary>
        public Action<UTween> OnComplete;
        /// <summary>
        /// Tween的周期消耗的时长
        /// </summary>
        public float Duration;
        /// <summary>
        /// 延迟的时长
        /// </summary>
        public float DelayTime;
        /// <summary>
        /// 忽略时间缩放
        /// </summary>
        public bool IgnoreTimeScale;

        public AnimationCurve Curve = AnimationCurve.EaseInOut(0,0,1,1);

        float CurrentTime;
        float TargetTime;
        int Way;
        EMode myMode;
        int myCounter;
        float safetime 
        {
            get 
            {
                if (CurrentTime < 0) return 0;
                else if (CurrentTime > TargetTime && TargetTime > 0) return TargetTime;
                return CurrentTime; 
            }
        }

        public float LastTime 
        {
            get 
            {
                if (Way > 0) return TargetTime - CurrentTime;
                else if (Way < 0) return CurrentTime;
                else return TargetTime;
            }
        }

        ETrack DefaultTrack = ETrack.Forward;
        public virtual void Awake() 
        {
            ResetStatus(true);
            if (AutoPlayAwake)
            {
                Way = 1;
            }
            else enabled = false;
        }

        public virtual void OnEnable() 
        {
            if (ResetEnable) 
            {
                ResetStatus(DefaultTrack == ETrack.Forward ? true : false); 
            }

            Way = DefaultTrack == ETrack.Forward ? 1 : -1;
        }

        public void Update()
        {
            if (IgnoreTimeScale)
                CurrentTime += Time.unscaledDeltaTime * Way;
            else
                CurrentTime += Time.deltaTime * Way;

            if (Counter > -1 && Way < 0 && CurrentTime <= 0)
            {
                CurrentTime = 0;
                Complete(); 
            }
            else if (Counter > -1 && Way > 0 && CurrentTime >= TargetTime) 
            {
                CurrentTime = TargetTime;
                Complete();
            }

            if (Counter != myCounter) SetDirty();
            if (Mode != myMode) SetDirty();

            if (Way > 0 && CurrentTime < 0) return;
            else if (Way < 0 && CurrentTime > TargetTime) return;

            if (Mode == EMode.Once)
            {
                if (CurrentTime == TargetTime)
                {
                    UpdateTween(Curve.Evaluate(1));
                }
                else 
                {
                    UpdateTween(Curve.Evaluate(Mathf.Repeat(safetime, Duration) / Duration));
                }
            }
            else 
            {
                int tripWay = ((int)(safetime / Duration) % 2) == 0 ? 0 : 1;
                float value = Mathf.Repeat(safetime, Duration) / Duration;
                if (tripWay > 0) value = 1 - value;
                UpdateTween(Curve.Evaluate(value));
            }
        }

        void Complete() 
        {
            enabled = false;
            OnComplete?.Invoke(this);
            OnComplete = null;

            if (AutoKill) MonoBehaviour.Destroy(this);
            if (AutoInactive) gameObject.SetActive(false);
        }

        public void ResetStatus(bool _forward) 
        { 
            if (Mode == EMode.Once)
                TargetTime = Duration + Counter * Duration;
            else
                TargetTime = Duration * 2 + Counter * Duration * 2;
             
            if (_forward)
            {
                CurrentTime = -DelayTime;
                UpdateTween(0);
            }
            else 
            {
                CurrentTime = TargetTime + DelayTime;
                UpdateTween(Mode == EMode.Once ? 1 : 0);
            }

            Way = 0;
            myMode = Mode;
            myCounter = Counter;
        }

        public void PlayForward(bool _replay=false) 
        {
            Play(true,_replay);  
        }

        void Play(bool _forward, bool _replay) 
        {
            DefaultTrack = _forward ? ETrack.Forward : ETrack.Reverse;
            if (!gameObject.activeSelf) gameObject.SetActive(true); 
            if (!enabled) enabled = true;
            
            if (_replay || ResetPlayCall)
                ResetStatus(_forward);

            Way = _forward ? 1 : -1; 
        }

        public void PlayReverse(bool _replay=false) 
        {
            Play(false,_replay);
        }

        public void SetDirty() 
        {
            myCounter = Counter;
            myMode = Mode;
            if (Counter < 0)
            {
                TargetTime = -1f;
            }
            else
            {
                if (Mode == EMode.Once)
                    TargetTime = Duration + Counter * Duration;
                else
                    TargetTime = Duration * 2 + Counter * Duration * 2;
            } 
        } 

        protected abstract void UpdateTween(float _value);
    }
}