using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Internal
{
    static class UInternalExtends 
    {
        internal static UGlobalRoot Instance;

        internal static UGlobalRoot GetRoot()
        {
            if (null == Instance)
            {
                GameObject go = new GameObject("__UGLOBAL.ROOT__");
                GameObject.DontDestroyOnLoad(go);
                go.hideFlags = HideFlags.NotEditable | HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
                Instance = go.AddComponent<UGlobalRoot>();
            }
            return Instance;
        }

        public static void StartCoroutine(this object obj, IEnumerator _target)
        {
            if (_target != null)
                GetRoot().StartCoroutine(_target);
        }

        public static void StopCoroutine(this object obj, IEnumerator _target)
        {
            if (_target != null)
                GetRoot().StopCoroutine(_target);
        }

        public static void StopAllCoroutine(this object obj)
        {
            if (Instance != null)
            {
                Instance.StopAllCoroutines();
            }
        }

        public static bool IsEnableLog=true;
       /* #region Output Extends
        public static void Log(this object _obj, string _message)
        {
            if (!IsEnableLog) return;
            Debug.Log(_message);
        }

        public static void Warning(this object _obj, string _message)
        {
            if (!IsEnableLog) return;
            Debug.LogWarning(_message);
        }

        public static void Error(this object _obj, string _message)
        {
            if (!IsEnableLog) return;
            Debug.LogError(_message);
        }

        public static void Log(this object _obj, string _message, params object[] _args)
        {
            if (!IsEnableLog) return;
            Debug.LogFormat(_message, _args);
        }

        public static void Warning(this object _obj, string _message, params object[] _args)
        {
            if (!IsEnableLog) return;
            Debug.LogWarningFormat(_message, _args);
        }

        public static void Error(this object _obj, string _message, params object[] _args)
        {
            if (!IsEnableLog) return;
            Debug.LogErrorFormat(_message, _args);
        }
        #endregion*/
    }
}
