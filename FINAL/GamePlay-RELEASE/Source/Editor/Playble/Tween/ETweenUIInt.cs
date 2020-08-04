using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using GamePlay.Tween;
using System;

namespace GamePlayEditor.Tween 
{ 
    public abstract class ETweenUIInt<T> : ETweenInspector
    {
        GUIContent NAME_FROM = new GUIContent("From");
        GUIContent NAME_TO = new GUIContent("To");
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
            EGUIUtility.DelayFloatField(ProFrom, NAME_FROM);
            EGUIUtility.DelayFloatField(ProTo, NAME_TO,NAME_FROM);
            serializedObject.ApplyModifiedProperties(); 
        }
         
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UUITweenTextInt))]
    public class ETweenUITextInt : ETweenUIInt<UUITweenTextInt> 
    {
    }
}

