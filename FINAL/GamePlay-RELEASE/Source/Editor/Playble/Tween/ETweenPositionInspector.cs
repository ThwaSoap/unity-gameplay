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
    [CustomEditor(typeof(UTweenPosition))]
    public class ETweenPositionInspector : ETweenInspector
    {
        SerializedProperty ProFrom;
        SerializedProperty ProTo;
        SerializedProperty ProIsLocal;
        SerializedProperty ProX;
        SerializedProperty ProY;
        SerializedProperty ProZ;
        protected override void OnEnable()
        {
            base.OnEnable();
            ProFrom = serializedObject.FindProperty("From");
            ProTo = serializedObject.FindProperty("To");
            ProIsLocal = serializedObject.FindProperty("IsLocal");
            ProX = serializedObject.FindProperty("X");
            ProY = serializedObject.FindProperty("Y");
            ProZ = serializedObject.FindProperty("Z");
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            ProFrom.Dispose();
            ProTo.Dispose();
            ProIsLocal.Dispose();
            ProX.Dispose();
            ProY.Dispose();
            ProZ.Dispose();
        }
        protected override void OnDrawChildren()
        { 
            GUILayout.BeginHorizontal();
            EGUIUtility.PushButton(ProIsLocal, "Local Value",GUILayout.MinWidth(70));
            EGUIUtility.PushButton(ProX, "Mask X", GUILayout.Width(70));
            EGUIUtility.PushButton(ProY, "Mask Y",GUILayout.Width(70));
            EGUIUtility.PushButton(ProZ, "Mask Z", GUILayout.Width(70));
            GUILayout.EndHorizontal();
            DrawProperty(ProFrom, "From", (v) =>
             {
                 if (v.IsLocal)
                     v.From = v.transform.localPosition;
                 else
                     v.From = v.transform.position;
             }, (v) => v.From = Vector3.zero);
            DrawProperty(ProTo, "To", (v) =>
            {
                if (v.IsLocal)
                    v.To = v.transform.localPosition;
                else
                    v.To = v.transform.position;
            },(v)=>v.To = Vector3.zero); 

            serializedObject.ApplyModifiedProperties();
        }

        void DrawProperty(SerializedProperty o,string _name,Action<UTweenPosition> _fillCallback,Action<UTweenPosition> _clearCallback) 
        { 
            EditorGUILayout.BeginHorizontal();
            EGUIUtility.Vector3Field(o, _name); 
            if (GUILayout.Button("Fill", GUILayout.Width(70)))
            {
                foreach (var v in targets) 
                { 
                    if (v is UTweenPosition) 
                    {
                        _fillCallback(v as UTweenPosition); 
                    } 
                }
                serializedObject.SetIsDifferentCacheDirty();
            }

            if (GUILayout.Button("Clear",GUILayout.Width(70))) 
            {
                foreach (var v in targets)
                {
                    if (v is UTweenPosition)
                    {
                        _clearCallback(v as UTweenPosition);
                    } 
                }

                serializedObject.SetIsDifferentCacheDirty();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

