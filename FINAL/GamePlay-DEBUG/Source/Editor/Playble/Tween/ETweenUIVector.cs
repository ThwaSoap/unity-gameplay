using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using GamePlay.Tween;
using System;

namespace GamePlayEditor.Tween 
{ 
    public abstract class ETweenUIVector2<T> : ETweenInspector
        where T : UTween
    {
        SerializedProperty ProFrom;
        SerializedProperty ProTo; 
        protected override void OnEnable()
        {
            base.OnEnable();
            ProFrom = serializedObject.FindProperty("From");
            ProTo = serializedObject.FindProperty("To"); 
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            ProFrom.Dispose();
            ProTo.Dispose(); 
        }
        protected override void OnDrawChildren()
        {   
            DrawProperty(ProFrom, "From", OnFillFrom, OnClearFrom);
            DrawProperty(ProTo, "To",  OnFillTo, OnClearTo); 

            serializedObject.ApplyModifiedProperties();
        }

        protected abstract void OnFillFrom(T _target);
        protected abstract void OnFillTo(T _target);
        protected abstract void OnClearFrom(T _target);
        protected abstract void OnClearTo(T _target);

        void DrawProperty(SerializedProperty o,string _name,Action<T> _fillCallback,Action<T> _clearCallback) 
        { 
            EditorGUILayout.BeginHorizontal();
            EGUIUtility.Vector2Field(o, _name); 
            if (GUILayout.Button("Fill", GUILayout.Width(70)))
            {
                foreach (var v in targets) 
                { 
                    if (v is T) 
                    {
                        _fillCallback(v as T); 
                    } 
                }
                serializedObject.SetIsDifferentCacheDirty();
            }

            if (GUILayout.Button("Clear",GUILayout.Width(70))) 
            {
                foreach (var v in targets)
                {
                    if (v is T)
                    {
                        _clearCallback(v as T);
                    } 
                }

                serializedObject.SetIsDifferentCacheDirty();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UUITweenAnchoredPosition))]
    public class ETweenUIPosition : ETweenUIVector2<UUITweenAnchoredPosition>
    {
        protected override void OnClearFrom(UUITweenAnchoredPosition _target)
        {
            _target.From = Vector2.zero;
        }

        protected override void OnClearTo(UUITweenAnchoredPosition _target)
        {
            _target.To = Vector2.zero;
        }

        protected override void OnFillFrom(UUITweenAnchoredPosition _target)
        { 
            _target.From = (_target.transform as RectTransform).anchoredPosition;
        }

        protected override void OnFillTo(UUITweenAnchoredPosition _target)
        { 
            _target.To = (_target.transform as RectTransform).anchoredPosition;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UUITweenSize))]
    public class UUITweenUISizeInspector : ETweenUIVector2<UUITweenSize>
    {
        protected override void OnClearFrom(UUITweenSize _target)
        {  
            _target.From = Vector2.zero; 
        }

        protected override void OnClearTo(UUITweenSize _target)
        {
            _target.To = Vector2.zero;
        }

        protected override void OnFillFrom(UUITweenSize _target)
        {
            _target.From = (_target.transform as RectTransform).sizeDelta;
        }

        protected override void OnFillTo(UUITweenSize _target)
        {
            _target.To = (_target.transform as RectTransform).sizeDelta;
        }
    }


    [CanEditMultipleObjects]
    [CustomEditor(typeof(UUITweenAnchoredPosition3D))]
    public class ETweenUIPosition3D : ETweenInspector 
    {
        SerializedProperty ProFrom;
        SerializedProperty ProTo;
        protected override void OnEnable()
        {
            base.OnEnable();
            ProFrom = serializedObject.FindProperty("From");
            ProTo = serializedObject.FindProperty("To");
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            ProFrom.Dispose();
            ProTo.Dispose();
        }
        protected override void OnDrawChildren()
        {
            DrawProperty(ProFrom, "From", (v)=> 
            {
                v.From = (v.transform as RectTransform).anchoredPosition3D;
            }, 
            (v)=> 
            {
                v.From = Vector3.zero;
            });
            DrawProperty(ProTo, "To", (v) =>
            {
                v.To = (v.transform as RectTransform).anchoredPosition3D;
            }, 
            (v) =>
            {
                v.To = Vector3.zero;
            }); 
            serializedObject.ApplyModifiedProperties();
        }
         

        void DrawProperty(SerializedProperty o, string _name, Action<UUITweenAnchoredPosition3D> _fillCallback, Action<UUITweenAnchoredPosition3D> _clearCallback)
        {
            EditorGUILayout.BeginHorizontal();
            EGUIUtility.Vector3Field(o, _name);
            if (GUILayout.Button("Fill", GUILayout.Width(70)))
            {
                foreach (var v in targets)
                {
                    if (v is UUITweenAnchoredPosition3D)
                    {
                        _fillCallback(v as UUITweenAnchoredPosition3D);
                    }
                }
                serializedObject.SetIsDifferentCacheDirty();
            }

            if (GUILayout.Button("Clear", GUILayout.Width(70)))
            {
                foreach (var v in targets)
                {
                    if (v is UUITweenAnchoredPosition3D)
                    {
                        _clearCallback(v as UUITweenAnchoredPosition3D);
                    }
                }

                serializedObject.SetIsDifferentCacheDirty();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

