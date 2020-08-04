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
    [CustomEditor(typeof(UTweenRotation))]
    public class ETweenRotationInspector : ETweenInspector
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
                     v.From = v.transform.localRotation;
                 else
                     v.From = v.transform.rotation;
             }, (v) => v.From = Quaternion.identity);
            DrawProperty(ProTo, "To", (v) =>
            {
                if (v.IsLocal)
                    v.To = v.transform.localRotation;
                else
                    v.To = v.transform.rotation;
            },(v)=>v.To = Quaternion.identity); 

            serializedObject.ApplyModifiedProperties(); 
        }

        void DrawProperty(SerializedProperty o,string _name,Action<UTweenRotation> _fillCallback,Action<UTweenRotation> _clearCallback) 
        { 
            EditorGUILayout.BeginHorizontal();
            EGUIUtility.QuaternionField(o, _name); 
            if (GUILayout.Button("Fill", GUILayout.Width(70)))
            {
                foreach (var v in targets) 
                { 
                    if (v is UTweenRotation) 
                    {
                        _fillCallback(v as UTweenRotation); 
                    } 
                }
                serializedObject.SetIsDifferentCacheDirty();
            }

            if (GUILayout.Button("Clear",GUILayout.Width(70))) 
            {
                foreach (var v in targets)
                {
                    if (v is UTweenRotation)
                    {
                        _clearCallback(v as UTweenRotation);
                    } 
                }

                serializedObject.SetIsDifferentCacheDirty();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

