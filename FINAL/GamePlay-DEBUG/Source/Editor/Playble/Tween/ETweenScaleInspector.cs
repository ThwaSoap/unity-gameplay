using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using GamePlay.Tween;
using System;

namespace GamePlayEditor.Tween 
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UTweenScale))]
    public class ETweenScaleInspector : ETweenInspector
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
            DrawProperty(ProFrom, "From", (v) =>
             {
                v.From = v.transform.localScale;
             }, (v) => v.From = Vector3.one);
            DrawProperty(ProTo, "To", (v) =>
            {
                v.To = v.transform.localScale;
            },(v)=>v.To = Vector3.one); 

            serializedObject.ApplyModifiedProperties(); 
        }

        void DrawProperty(SerializedProperty o,string _name,Action<UTweenScale> _fillCallback,Action<UTweenScale> _clearCallback) 
        { 
            EditorGUILayout.BeginHorizontal();
            EGUIUtility.Vector3Field(o, _name); 
            if (GUILayout.Button("Fill", GUILayout.Width(70)))
            {
                foreach (var v in targets) 
                { 
                    if (v is UTweenScale) 
                    {
                        _fillCallback(v as UTweenScale); 
                    } 
                }
                serializedObject.SetIsDifferentCacheDirty();
            }

            if (GUILayout.Button("Clear",GUILayout.Width(70))) 
            {
                foreach (var v in targets)
                {
                    if (v is UTweenScale)
                    {
                        _clearCallback(v as UTweenScale);
                    } 
                }

                serializedObject.SetIsDifferentCacheDirty();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

