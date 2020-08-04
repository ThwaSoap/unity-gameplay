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
    [CustomEditor(typeof(UTweenEulerAngles))]
    public class ETweenEulerAnglesInspector : ETweenInspector
    {
        SerializedProperty ProFrom;
        SerializedProperty ProTo;
        SerializedProperty ProIsLocal;
        SerializedProperty ProIsSmooth;
        protected override void OnEnable()
        {
            base.OnEnable();
            ProFrom = serializedObject.FindProperty("From");
            ProTo = serializedObject.FindProperty("To");
            ProIsLocal = serializedObject.FindProperty("IsLocal");
            ProIsSmooth = serializedObject.FindProperty("Smooth");
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            ProFrom.Dispose();
            ProTo.Dispose();
            ProIsLocal.Dispose();
            ProIsSmooth.Dispose();
        }
        protected override void OnDrawChildren()
        {
            EditorGUILayout.BeginHorizontal();
            EGUIUtility.PushButton(ProIsLocal, "Local");
            EGUIUtility.PushButton(ProIsSmooth, "Smooth");
            EditorGUILayout.EndHorizontal();
            DrawProperty(ProFrom, "From", (v) =>
             {
                 if (v.IsLocal)
                     v.From = v.transform.localEulerAngles;
                 else
                     v.From = v.transform.eulerAngles;

             }, (v) => v.From = Vector3.zero);
            DrawProperty(ProTo, "To", (v) =>
            {
                if (v.IsLocal)
                    v.To = v.transform.localEulerAngles;
                else
                    v.To = v.transform.eulerAngles; 
            },(v)=>v.To = Vector3.zero); 

            serializedObject.ApplyModifiedProperties(); 
        }

        void DrawProperty(SerializedProperty o,string _name,Action<UTweenEulerAngles> _fillCallback,Action<UTweenEulerAngles> _clearCallback) 
        { 
            EditorGUILayout.BeginHorizontal();
            EGUIUtility.Vector3Field(o, _name); 
            if (GUILayout.Button("Fill", GUILayout.Width(70)))
            {
                foreach (var v in targets) 
                { 
                    if (v is UTweenEulerAngles) 
                    {
                        _fillCallback(v as UTweenEulerAngles); 
                    } 
                }
                serializedObject.SetIsDifferentCacheDirty();
            }

            if (GUILayout.Button("Clear",GUILayout.Width(70))) 
            {
                foreach (var v in targets)
                {
                    if (v is UTweenEulerAngles)
                    {
                        _clearCallback(v as UTweenEulerAngles);
                    } 
                }

                serializedObject.SetIsDifferentCacheDirty();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

