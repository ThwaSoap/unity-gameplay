using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GamePlayEditor.Tween 
{
    public static class EGUIUtility
    { 
        static GUIStyle TextStyle;
        static Color Drak = new Color(0.15f, 0.15f, 0.15f);
        static Color Light = new Color(0.85f, 0.85f, 0.85f);
        static GUIStyle BtnWarning;
        static GUIStyle BtnMessage;

        public static GUIStyle GetBtnWarning()
        {
            if (BtnWarning == null)
            {
                var active = GUI.skin.FindStyle("flow node 6").normal;
                BtnWarning = new GUIStyle(GUI.skin.FindStyle("flow node 0"));
                BtnWarning.onNormal.background = active.background;
                BtnWarning.onFocused.background = active.background;
                BtnWarning.onActive.background = active.background;
                BtnWarning.onHover.background = active.background;
            }

            return BtnWarning;
        }
        public static GUIStyle GetBtnMessage()
        {
            if (BtnMessage == null)
            { 
                var active = GUI.skin.FindStyle("flow node 3").normal;
                BtnMessage = new GUIStyle(GUI.skin.FindStyle("flow node 0"));
                BtnMessage.onNormal.background = active.background;
                BtnMessage.onFocused.background = active.background;
                BtnMessage.onActive.background = active.background;
                BtnMessage.onHover.background = active.background;
            }

            return BtnMessage;
        } 



        public static GUIStyle GetTextStyle()
        {
            if (TextStyle == null)
            {
                TextStyle = new GUIStyle(GUI.skin.FindStyle("ShurikenLabel"));
                TextStyle.alignment = TextAnchor.MiddleCenter;
            }
            return TextStyle;
        }

        public static GUIStyle GetDrakTextStyle()
        {
            GetTextStyle().normal.textColor = Drak;
            return GetTextStyle();
        }

        public static GUIStyle GetLightTextStyle()
        {
            GetTextStyle().normal.textColor = Light;
            return GetTextStyle();
        } 
        public static bool PushButton(bool _value, string _name,params GUILayoutOption[] _options)
        {
            Rect rect = EditorGUILayout.GetControlRect(_options);
            //_value = EditorGUI.Toggle(rect, _value, GUI.skin.button);
            _value = EditorGUI.Toggle(rect, _value, GetBtnMessage());
            EditorGUI.LabelField(rect, _name,GetDrakTextStyle());
            return _value;
        }

        public static void PushButton(SerializedProperty o, string _title,params GUILayoutOption[] _options) 
        {
            if (o.hasMultipleDifferentValues)
            {
                Rect rect = EditorGUILayout.GetControlRect(_options);
                bool val = EditorGUI.Toggle(rect,true, GetBtnWarning());
                EditorGUI.LabelField(rect, _title, GetDrakTextStyle());
                if (val==false)
                {
                    o.boolValue = val;
                }
            }
            else 
            {
                o.boolValue = PushButton(o.boolValue,_title, _options);
            }
        }

        public static void EnumPop(SerializedProperty _property,string _title)
        { 
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(_title);
            if (_property.hasMultipleDifferentValues)
            {
                Rect size = EditorGUILayout.GetControlRect();
                EditorGUI.BeginProperty(size,new GUIContent(""),_property);
                int result = EditorGUI.Popup(size,-1, _property.enumNames);
                if (result != -1) 
                {
                    _property.enumValueIndex = result;
                }
                EditorGUI.EndProperty();
            }
            else
            {
                _property.enumValueIndex = (int)EditorGUILayout.Popup(_property.enumValueIndex,_property.enumNames);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void DelayFloatField(SerializedProperty o, GUIContent _title, GUIContent _width = null) 
        {
            Rect size = GUILayoutUtility.GetRect(_title, GUI.skin.label);
            float width = 0;

            if (_width != null)
               width = GUI.skin.label.CalcSize(_width).x;
            else 
               width = GUI.skin.label.CalcSize(_title).x;
            size.x += width + 0.5f;
            size.width -= width;
            if (o.hasMultipleDifferentValues)
            {
                EditorGUI.BeginProperty(size, new GUIContent(""), o);
                int input = Random.Range(int.MinValue, int.MaxValue);
                float v = EditorGUI.DelayedFloatField(size, input);
                if (v != input)
                {
                    o.floatValue = v;
                }
                EditorGUI.EndProperty();
            }
            else
            {
                o.floatValue = EditorGUI.DelayedFloatField(size, o.floatValue);
            }
            size.x -= width + 1.5f;
            EditorGUI.LabelField(size, _title);
        }


        static GUIContent NAME_X = new GUIContent("X");
        static GUIContent NAME_Y = new GUIContent("Y");
        static GUIContent NAME_Z = new GUIContent("Z");
        static GUIContent NAME_W = new GUIContent("W");
        public static void Vector4Field(SerializedProperty _property, string _title) 
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_title, GUILayout.MaxWidth(50));
            if (_property.hasMultipleDifferentValues)
            {
                var x = _property.FindPropertyRelative("x");
                var y = _property.FindPropertyRelative("y");
                var z = _property.FindPropertyRelative("z");
                var w = _property.FindPropertyRelative("w");
                DelayFloatField(x, NAME_X);
                DelayFloatField(y, NAME_Y);
                DelayFloatField(z, NAME_Z);
                DelayFloatField(w, NAME_W);
            }
            else 
            { 
                _property.vector4Value = EditorGUILayout.Vector4Field("",_property.vector4Value);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void Vector3Field(SerializedProperty _property, string _title)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_title, GUILayout.MaxWidth(50));
            if (_property.hasMultipleDifferentValues)
            {
                var x = _property.FindPropertyRelative("x");
                var y = _property.FindPropertyRelative("y");
                var z = _property.FindPropertyRelative("z"); 
                DelayFloatField(x, NAME_X);
                DelayFloatField(y, NAME_Y);
                DelayFloatField(z, NAME_Z);
            }
            else
            {
                _property.vector3Value = EditorGUILayout.Vector3Field("", _property.vector3Value);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void Vector2Field(SerializedProperty _property, string _title)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_title, GUILayout.MaxWidth(50));
            if (_property.hasMultipleDifferentValues)
            {
                var x = _property.FindPropertyRelative("x");
                var y = _property.FindPropertyRelative("y");
                DelayFloatField(x, NAME_X);
                DelayFloatField(y, NAME_Y); 
            }
            else
            {
                _property.vector2Value = EditorGUILayout.Vector2Field("", _property.vector2Value);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void QuaternionField(SerializedProperty _property, string _title)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_title, GUILayout.MaxWidth(50));
            var x = _property.FindPropertyRelative("x");
            var y = _property.FindPropertyRelative("y");
            var z = _property.FindPropertyRelative("z");
            var w = _property.FindPropertyRelative("w");
            DelayFloatField(x, NAME_X);
            DelayFloatField(y, NAME_Y);
            DelayFloatField(z, NAME_Z);
            DelayFloatField(z, NAME_W);  
            EditorGUILayout.EndHorizontal();
        }

        public static void ColorField(SerializedProperty o, string _title) 
        {
            if (o.hasMultipleDifferentValues)
            {
                Rect size = EditorGUILayout.GetControlRect();
                EditorGUI.BeginProperty(size, new GUIContent(_title), o);
                Color input = new Color(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
                Color output = EditorGUILayout.ColorField(_title, input);
                if (input != output)
                {
                    o.colorValue = output;
                }
                EditorGUI.EndProperty();
            }
            else
            {
                o.colorValue = EditorGUILayout.ColorField(_title, o.colorValue);
            }
        }
    }
}
