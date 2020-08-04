using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GamePlayEditor
{
    static class EGUIConfig
    {
        public static GUIStyle FolderStyle;


        static EGUIConfig()
        {
            FolderStyle = new GUIStyle();
            //FolderStyle.normal.textColor = new Color(0.44f, 0.44f, 0.44f);
            FolderStyle.contentOffset = new Vector2(5, 2);
            //ReadPointStyle.contentOffset = new Vector2(0,3);
        }
    }

    abstract class EGUI
    {
        public bool Visible=true;
        public void Render()
        {
            if (!Visible) return;
            OnRender();
        }

        protected abstract void OnRender();
    }

    /// <summary>
    /// GUI组件对象
    /// </summary>
    class EGUIComponent : EGUI
    {
        protected override void OnRender()
        {

        }
    }

    class EGUIToggle : EGUI
    {
        bool Value;
        public event Action<bool> OnValueChanged;
        public bool IsOn
        {
            get { return Value; }
            set {
                Value = value;
                OnValueChanged?.Invoke(value);
            }
        }
        string Title;
        public EGUIToggle(string _title, bool _default=false, Action<bool> _onValueChanged = null)
        {
            Title = _title;
            Value = _default;
            OnValueChanged = _onValueChanged;
        }

        protected override void OnRender()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Title,GUILayout.ExpandWidth(true));
            bool next = EditorGUILayout.Toggle(IsOn,GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
            if (next != IsOn) IsOn = next;
        }
    }

    class EGUITitle : EGUI
    {
        string Title;
        public EGUITitle(string _title)
        {
            Title = _title;
        }
        protected override void OnRender()
        {
            GUILayout.Box(Title, GUILayout.ExpandWidth(true));
        }
    }

    class EGUIToolBar : EGUI
    {
        string[] Menu;
        int Index;
        public event Action<string> OnValueChanged;
        public EGUIToolBar(string[] _menu, int _default, Action<string> _onValueChanged)
        {
            Menu = _menu;
            Index = _default;
            OnValueChanged = _onValueChanged;
            OnValueChanged?.Invoke(Menu[Index]);
        }

        protected override void OnRender()
        {
            int next = GUILayout.Toolbar(Index, Menu);
            if (next != Index)
            {
                Index = next;
                OnValueChanged?.Invoke(Menu[Index]);
            }
        }
    }

    class EGUIEnumPopup : EGUI
    {
        Enum Select;
        public Enum Value
        {
            get { return Select; }
            set
            {
                Select = value;
                OnValueChanged?.Invoke(value);
            }
        }
        public event Action<Enum> OnValueChanged;
        public EGUIEnumPopup(Enum _default)
        {
            Select = _default;
        }

        protected override void OnRender()
        {
            Enum next = EditorGUILayout.EnumPopup(Value);
            if (next != Value)
            {
                Value = next; 
            }
        }
    }

    class EGUIButton : EGUI
    {
        public event Action OnClick;

        string Title;

        public EGUIButton(string _title, Action _onClick = null)
        {
            Title = _title;
            OnClick = _onClick;
        }

        protected override void OnRender()
        {
            if (GUILayout.Button(Title))
            {
                OnClick?.Invoke();
            }
        }
    }

    /// <summary>
    /// 纵向布局元素
    /// </summary>
    class EGUIVerticalLayout : EGUI
    {
        List<EGUI> Elements = new List<EGUI>();

        public EGUIVerticalLayout Add(EGUI _element)
        {
            Elements.Add(_element);
            return this;
        }

        public EGUIVerticalLayout Remove(EGUI _element)
        {
            Elements.Remove(_element);
            return this;
        }

        protected override void OnRender()
        {
            EditorGUILayout.BeginVertical();
            foreach (var v in Elements)
            {
                v.Render();
            }
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// 横向布局元素
    /// </summary>
    class EGUIHorizontalLayout : EGUI
    {
        List<EGUI> Elements = new List<EGUI>();

        public EGUIHorizontalLayout Add(EGUI _element)
        {
            Elements.Add(_element);
            return this;
        }

        public EGUIHorizontalLayout Remove(EGUI _element)
        {
            Elements.Remove(_element);
            return this;
        }

        protected override void OnRender()
        {
            EditorGUILayout.BeginHorizontal();
            foreach (var v in Elements)
            {
                v.Render();
            }
            EditorGUILayout.EndHorizontal();
        }
    }



    /// <summary>
    /// 文字输入与浏览组件
    /// </summary>
    class EGUITextInput : EGUI
    {
        /// <summary>
        /// 在点击保存时调用该事件，如果你希望输入的值被存储应该返回true
        /// 否则返回false
        /// </summary>
        public event Func<string, string> OnSave;
        string Content;
        string EditContent;
        bool IsEditor;
        string Title;

        public EGUITextInput(string _title, string _content, Func<string, string> _saveCallback = null)
        {
            Content = _content;
            Title = _title;
            IsEditor = false;
            OnSave += _saveCallback;
        }

        protected virtual string FixContent(string _content) 
        {
            return _content;
        }

        protected override void OnRender()
        {
            if (IsEditor)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(Title, GUILayout.ExpandWidth(false));
                EditContent = GUILayout.TextField(EditContent, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Save", GUILayout.MaxWidth(150), GUILayout.MinWidth(80)))
                {
                    IsEditor = false;
                    if (OnSave != null)
                    {
                        Content = OnSave.Invoke(EditContent);
                    }
                }
                if (GUILayout.Button("Cancel", GUILayout.MaxWidth(150), GUILayout.MinWidth(80)))
                {
                    IsEditor = false;
                }

                EditContent = FixContent(EditContent);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(Title, GUILayout.ExpandWidth(false));
                GUILayout.Label(Content, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Edit", GUILayout.MaxWidth(150), GUILayout.MinWidth(80)))
                {
                    IsEditor = true;
                    EditContent = Content;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    class EGUIPathInput : EGUITextInput 
    {
        public EGUIPathInput(string _title, string _content, Func<string, string> _saveCallback = null) 
            : base(_title,_content,_saveCallback)
        {
        }

        protected override string FixContent(string _content)
        {
            if (GUILayout.Button("Pickup Path", GUILayout.MaxWidth(150), GUILayout.MinWidth(80))) 
            {
                string _outValue = null;
                if (EditorGamePlay.TryPickupAssetPath(out _outValue)) 
                {
                    return _outValue;
                }
            }
            return _content;
        }
    }

    class EGUIFilePathInput : EGUITextInput
    {
        public EGUIFilePathInput(string _title, string _content, Func<string, string> _saveCallback = null)
            : base(_title, _content, _saveCallback)
        {
        }

        protected override string FixContent(string _content)
        {
            if (GUILayout.Button("Pickup Path", GUILayout.MaxWidth(150), GUILayout.MinWidth(80)))
            {
                string _outValue = null;
                if (EditorGamePlay.TryPickupAssetFilePath(out _outValue))
                {
                    return _outValue;
                }
            }
            return _content;
        }
    }


    /// <summary>
    /// 文件夹目录对象
    /// </summary>
    class EGUIToggleFolder : EGUI
    {
        /// <summary>
        /// 相对于Assets的路径
        /// </summary>
        public string RelativePath { get; private set; }
        /// <summary>
        /// 当前文件夹的名称
        /// </summary>
        public string FolderName { get; private set; }
        /// <summary>
        /// 选中状态
        /// </summary>
        public bool Choose = false;
        UnityEngine.Object Instance;
        GUIContent FolderContent
        {
            get { return EditorGUIUtility.ObjectContent(Instance, typeof(UnityEngine.Object)); }
        }

        public EGUIToggleFolder(string _subPath)
        {
            Instance = AssetDatabase.LoadAssetAtPath(_subPath, typeof(UnityEngine.Object));
            FolderName = Instance.name;
            RelativePath = _subPath;
        }
        ~EGUIToggleFolder()
        {
            Instance = null;
        }

        protected override void OnRender()
        {
            EditorGUILayout.BeginHorizontal();
            Choose = EditorGUILayout.Toggle(Choose, GUILayout.MaxWidth(10));
            EditorGUILayout.Foldout(false, FolderContent, false, EGUIConfig.FolderStyle);
            if (GUILayout.Button("TO", GUILayout.MaxWidth(50)))
            {
                EditorGUIUtility.PingObject(Instance);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// 文件目录(不包含子目录)
    /// </summary>
    class EGUIFolderList : EGUI
    {
        Vector2 Position = Vector2.zero;
        List<EGUIToggleFolder> List = new List<EGUIToggleFolder>();
        public string RootPath { get; private set; }

        public EGUIFolderList(string _rootPath)
        {
            ResetPath(_rootPath);
        }

        public void ResetPath(string _rootPath)
        {
            List.Clear();
            RootPath = _rootPath;
            string[] subFolders = AssetDatabase.GetSubFolders("Assets/" + _rootPath);
            foreach (var v in subFolders)
            {
                List.Add(new EGUIToggleFolder(v));
            }
        }

        public void SetAll(bool _choose)
        {
            foreach (var v in List)
            {
                v.Choose = _choose;
            }
        }

        public List<EGUIToggleFolder> GetChooseFolder()
        {
            List<EGUIToggleFolder> result = new List<EGUIToggleFolder>();
            foreach (var v in List)
            {
                if (v.Choose)
                {
                    result.Add(v);
                }
            }
            return result;
        }

        protected override void OnRender()
        {
            Position = EditorGUILayout.BeginScrollView(Position);
            EditorGUILayout.BeginVertical();
            foreach (var v in List)
            {
                v.Render();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }

    /// <summary>
    /// 文件夹树节点,同时包含子文件夹对象 
    /// </summary>
    class EGUIFolderTree : EGUI
    {
        public string RootPath { get; protected set; }

        public List<EGUIFolderTree> Tree { get; protected set; }

        EGUIComponent Component; 
        UnityEngine.Object Instance;
        bool Open;
        string Prefix;
        GUIContent FolderContent
        {
            get { return EditorGUIUtility.ObjectContent(Instance, typeof(UnityEngine.Object)); }
        }
        public EGUIFolderTree() { }
        public EGUIFolderTree(string _rootPath, int _depth)
        {
            if (_depth > 0)
                Prefix = "  ";
            else
                Prefix = "";
            for (int i = 0; i < _depth; i++)
            {
                Prefix += "  ";
            }
            Tree = new List<EGUIFolderTree>();
            RootPath = _rootPath;
            Instance = AssetDatabase.LoadAssetAtPath(RootPath, typeof(UnityEngine.Object));
            string[] subFolders = AssetDatabase.GetSubFolders(RootPath);
            foreach (var v in subFolders)
            {
                Tree.Add(new EGUIFolderTree(v, _depth + 1));
            }
        }

        void RemoveChildrenComponents()
        {
            Component = null;
            foreach (var v in Tree)
            {
                v.RemoveChildrenComponents();
            }
        }

        /// <summary>
        /// 移除子节点所有的Component对象
        /// </summary>
        public void RemoveAllComponentsOfChild()
        {
            foreach (var v in Tree)
            {
                v.RemoveChildrenComponents();
            }
        }

        /// <summary>
        /// 为当前节点设置一个Component对象
        /// </summary>
        /// <param name="_component"></param>
        public void SetComponent(EGUIComponent _component)
        {
            Component = _component;
        }

        public T GetComponent<T>() where T : EGUIComponent
        {
            return Component as T;
        }

        protected override void OnRender()
        {
            if (Instance != null)
            {
                if (Tree.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(Prefix, GUILayout.ExpandWidth(false));
                    Open = EditorGUILayout.Foldout(Open, FolderContent, true);
                    if (Component != null) Component.Render();
                    if (GUILayout.Button("TO", GUILayout.ExpandWidth(false)))
                    {
                        EditorGUIUtility.PingObject(Instance);
                    }
                    EditorGUILayout.EndHorizontal();

                    if (Open)
                    {
                        foreach (var v in Tree)
                        {
                            v.Render();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("   " + Prefix, GUILayout.ExpandWidth(false));
                    EditorGUILayout.LabelField(FolderContent);
                    if (Component != null) Component.Render();
                    if (GUILayout.Button("TO", GUILayout.ExpandWidth(false)))
                    {
                        EditorGUIUtility.PingObject(Instance);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }

    /// <summary>
    /// 文件夹树根节点
    /// 如果你需要创建一个树状的文件夹目录，使用这个对象
    /// </summary>
    class EGUIFolderTreeRoot : EGUIFolderTree
    {
        Vector2 Position;
        public EGUIFolderTreeRoot(string _root) : base()
        {
            ResetPath(_root);
        }

        /// <summary>
        /// 刷新路径
        /// </summary>
        /// <param name="_rootPath">相对于Assets的路径，不包含Assets</param>
        public void ResetPath(string _rootPath)
        {
            RootPath = _rootPath;
            Tree = new List<EGUIFolderTree>();
            string[] subFolders = AssetDatabase.GetSubFolders("Assets/" + _rootPath);
            foreach (var v in subFolders)
            {
                Tree.Add(new EGUIFolderTree(v, 0));
            }
        }

        protected override void OnRender()
        {
            Position = EditorGUILayout.BeginScrollView(Position);
            EditorGUILayout.BeginVertical();
            foreach (var v in Tree)
            {
                v.Render();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
}
