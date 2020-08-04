using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GamePlayEditor.Tween 
{
    using GamePlay.Tween;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UTween), true)] 
    public class ETweenInspector : Editor
    {
        GUIContent NAME_COUNTER = new GUIContent("Counter");
        GUIContent NAME_GROUP = new GUIContent("Group");
        GUIContent NAME_DURATION = new GUIContent("Duration");
        GUIContent NAME_DELAYTIME = new GUIContent("DelayTime");
        GUIContent NAME_CURVE = new GUIContent("");


        static string[] CounterBar = new string[]
        {
            "LOOP",
            "ONCE",
            "REPEAT"
        };

        static string[] GroupBar = new string[]
        {
            "SINGLE",
            "NUMBER"
        };

        SerializedProperty ProMode;
         
        SerializedProperty ProCounter;
        SerializedProperty ProGroup;
        SerializedProperty ProDuration;
        SerializedProperty ProDelayTime;
        SerializedProperty ProCurve;
         
        SerializedProperty ProResetPlayCall;
        SerializedProperty ProResetEnable; 
        SerializedProperty ProIgnoreTimeScale;

        SerializedProperty ProAutoPlayAwake; 
        SerializedProperty ProAutoKill;
        SerializedProperty ProAutoInactive;
        protected virtual void OnEnable()
        {
            ProMode = serializedObject.FindProperty("Mode");
            
            ProCounter = serializedObject.FindProperty("Counter");
            ProGroup = serializedObject.FindProperty("Group");
            ProDuration = serializedObject.FindProperty("Duration");
            ProDelayTime = serializedObject.FindProperty("DelayTime");
            ProCurve = serializedObject.FindProperty("Curve");

 
            ProResetPlayCall = serializedObject.FindProperty("ResetPlayCall");
            ProResetEnable = serializedObject.FindProperty("ResetEnable");
            ProIgnoreTimeScale = serializedObject.FindProperty("IgnoreTimeScale");
            ProAutoPlayAwake = serializedObject.FindProperty("AutoPlayAwake"); 
            ProAutoKill = serializedObject.FindProperty("AutoKill");
            ProAutoInactive = serializedObject.FindProperty("AutoInactive");
        }

        protected virtual void OnDisable() 
        {
            ProMode.Dispose(); 
            ProCounter.Dispose();
            ProGroup.Dispose();
            ProDuration.Dispose();
            ProDelayTime.Dispose();
            ProCurve.Dispose();
             
            ProResetPlayCall.Dispose();
            ProResetEnable.Dispose();
            ProIgnoreTimeScale.Dispose();
            ProAutoPlayAwake.Dispose(); 
            ProAutoKill.Dispose();
            ProAutoInactive.Dispose();
        }

        public override void OnInspectorGUI()
        {
            UTween o = this.target as UTween;
            EGUIUtility.EnumPop(ProMode,  "Mode"); 
            DrawCounter(o);
            DrawGroupNumber(o);
            OnDrawChildren();
            EditorGUILayout.DelayedFloatField(ProDuration, NAME_DURATION);
            EditorGUILayout.DelayedFloatField(ProDelayTime, NAME_DELAYTIME);
            if (ProDuration.floatValue <= 0) ProDuration.floatValue = 1f;
            if (ProDelayTime.floatValue < 0) ProDelayTime.floatValue = 0;
            //Rect rect = new Rect(Vector2.zero, Vector2.one);
            //rect.xMin = -100;
            //rect.xMax = 100;
            //rect.yMin = -100;
            //rect.yMax = 100;
            //rect.center = new Vector2(0.5f,0.5f);
            //rect.x = 0;
            //rect.y = 0;
            //EditorGUILayout.CurveField(ProCurve, Color.green, rect, NAME_CURVE, GUILayout.Height(80));
            ProCurve.animationCurveValue=EditorGUILayout.CurveField(ProCurve.animationCurveValue,GUILayout.Height(80));
            DrawToggle();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawCounter(UTween o) 
        {
            if (this.targets.Length > 1) 
            { 
                EditorGUILayout.DelayedIntField(ProCounter, NAME_COUNTER);
                return;
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Counter");
            int idx = 0;
            if (ProCounter.intValue == 0)
                idx = 1;
            else if (ProCounter.intValue > 0)
                idx = 2;
            else
                idx = 0;
            idx = GUILayout.Toolbar(idx, CounterBar);
            if (idx == 0)
            {
                ProCounter.intValue = -1;
            }
            else if (idx == 1)
            {
                ProCounter.intValue = 0;
            }
            else if (ProCounter.intValue < 1) 
            {
                ProCounter.intValue = 1;
            }
            EditorGUILayout.EndHorizontal();
            if (idx == 2) 
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Value");
                ProCounter.intValue = EditorGUILayout.DelayedIntField(ProCounter.intValue);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawGroupNumber(UTween o) 
        {
            if (this.targets.Length > 1)
            {
                EditorGUILayout.DelayedIntField(ProGroup, NAME_GROUP);
                return;
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Group");
            int idx = 0;
            if (ProGroup.intValue == 0)
                idx = 0; 
            else
                idx = 1;
            idx = GUILayout.Toolbar(idx, GroupBar);
            if (idx == 0)
            {
                ProGroup.intValue = 0;
            }
            else if (ProGroup.intValue == 0) 
            {
                ProGroup.intValue = 1;
            }
            EditorGUILayout.EndHorizontal();
            if (idx == 1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Value");
                ProGroup.intValue = EditorGUILayout.DelayedIntField(ProGroup.intValue);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawToggle() 
        {
            EditorGUILayout.HelpBox("Options", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            EGUIUtility.PushButton(ProIgnoreTimeScale, "IGNORE SCALE TIME", GUILayout.MinWidth(80));
            EGUIUtility.PushButton(ProAutoPlayAwake, "AWAKE PLAY", GUILayout.MinWidth(80));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(); 
            EGUIUtility.PushButton(ProResetEnable, "RESET AT ENABLE", GUILayout.MinWidth(80));
            EGUIUtility.PushButton(ProResetPlayCall, "RESET AT PLAYCALL", GUILayout.MinWidth(80)); 
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EGUIUtility.PushButton(ProAutoKill, "AUTO KILL", GUILayout.MinWidth(80));
            EGUIUtility.PushButton(ProAutoInactive, "AUTO INACTIVE", GUILayout.MinWidth(80)); 
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void OnDrawChildren() { }
    }
}