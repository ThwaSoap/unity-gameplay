using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GamePlayEditor.AssetBundleTools
{
    class EGUIComponentFolderItem : EGUIComponent
    {
        public static string[] Options = new string[3]
        {
            "None",
            "Group",
            "Single"
        };

        EGUIFolderTree Parent;

        int Index;
        /// <summary>
        /// 当前的属性是否发生了变化
        /// </summary>
        bool IsChanged;
        /// <summary>
        /// 当前节点或者子节点是否有设置属性值
        /// </summary>
        bool ValidIndex
        {
            get
            {
                if (Index > 0) return true;

                foreach (var v in Parent.Tree)
                {
                    if (v.GetComponent<EGUIComponentFolderItem>().ValidIndex)
                        return true;
                }
                return false;
            }
        }

        static GUIContent Rad = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/GamePlay/Editor/Res/ip_rad.png"));
        static GUIContent Green = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/GamePlay/Editor/Res/ip_green.png"));
          

        public EGUIComponentFolderItem(EGUIFolderTree _parent, string _tag = "NONE")
        {
            IsChanged = false;
            Parent = _parent;
            for (int i = 0; i < Options.Length; i++)
            {
                if (Options[i] == _tag)
                {
                    Index = i;
                }
            }

            foreach (var v in Parent.Tree)
            {
                v.SetComponent(new EGUIComponentFolderItem(v));
            }
        }

        protected override void OnRender()
        {
            if (ValidIndex)
            {
                GUILayout.Label(Green, GUILayout.Width(16));
            }

            if (IsChanged)
            {
                GUILayout.Label(Rad, GUILayout.Width(16));
            }

            int next = EditorGUILayout.Popup(Index, Options, GUILayout.Width(120));
            if (next != Index)
            {
                Index = next;
                IsChanged = true;
            }
        }

        public void WriteConfigToMap(Dictionary<string, List<string>> _map)
        {
            _map[Options[Index]].Add(Parent.RootPath);

            foreach (var v in Parent.Tree)
            {
                v.GetComponent<EGUIComponentFolderItem>().WriteConfigToMap(_map);
            }
            IsChanged = false;
        }

        public void ReadConfigFromMap(Dictionary<string, List<string>> _map)
        {
            List<string> outList;
            if (_map.TryGetValue("Group", out outList))
            {
                if (outList.Contains(Parent.RootPath))
                {
                    Index = 1;
                }
            }

            if (_map.TryGetValue("Single", out outList))
            {
                if (outList.Contains(Parent.RootPath))
                {
                    Index = 2;
                }
            }

            foreach (var v in Parent.Tree)
            {
                v.GetComponent<EGUIComponentFolderItem>().ReadConfigFromMap(_map);
            }
        }
    }
}
