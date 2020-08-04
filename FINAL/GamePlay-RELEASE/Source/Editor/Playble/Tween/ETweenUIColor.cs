using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GamePlay.Tween;
using UnityEngine.UI;

namespace GamePlayEditor.Tween 
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UUITweenColor))]
    public class ETweenUIColor : ETweenInspector
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
            DrawProperty(ProFrom,"From",(v)=> 
            {
                v.From = v.GetComponent<MaskableGraphic>().color;
            },(v)=> v.From = Color.white);

            DrawProperty(ProTo, "To", (v) =>
            {
                v.To = v.GetComponent<MaskableGraphic>().color;
            }, (v) => v.To = Color.white);
        }

        void DrawProperty(SerializedProperty o,string _title,System.Action<UUITweenColor> _callbackFill, System.Action<UUITweenColor> _callbackClear) 
        {
            EditorGUILayout.BeginHorizontal();
            EGUIUtility.ColorField(o, _title);
            if (GUILayout.Button("Fill"))
            {
                foreach (UUITweenColor v in targets)
                {
                    _callbackFill(v); 
                }
            }

            if (GUILayout.Button("Clear"))
            {
                foreach (UUITweenColor v in targets)
                {
                    _callbackClear(v);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}