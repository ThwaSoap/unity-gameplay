using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using GamePlay;
using System.IO;
using System.Reflection;

namespace GamePlayEditor.UDBTool
{
    public class UDBView : EditorWindow
    {
        [MenuItem("GamePlayTools/UDBTools/UDBView")]
        static void Init() 
        {
            var window = EditorWindow.GetWindow<UDBView>(false, "UDBView");
            window.Show(); 
        }

        Rect dragRect = new Rect(200, -2, 1, 300);
        Rect listRect = new Rect(-2, -2, 200, 300);
        Rect contentRect = new Rect(202, -2,200, 300); 
        private void OnGUI()
        {
            Rect mainWindow = this.position;
            dragRect.y = -2;
            if (dragRect.x > mainWindow.width - 30) dragRect.x = mainWindow.width - 15;
            if (dragRect.x < 100) dragRect.x = 100;
            
            dragRect.height = mainWindow.height + 4; 
            listRect.height = mainWindow.height + 4;
            contentRect.height = mainWindow.height + 4; 
            listRect.width = dragRect.x + 4;
            //contentRect.x  = dragRect.x + 4;
            //contentRect.width = mainWindow.width - listRect.width;

            BeginWindows();
            GUILayout.BeginHorizontal();
            listRect = GUILayout.Window(0, listRect, OnDrawList,"File List",GUILayout.MinWidth(100)); 
            
            dragRect.x = listRect.width - 4;
            contentRect.x = dragRect.x + 4;
            contentRect.width = mainWindow.width - listRect.width;

            contentRect = GUILayout.Window(1, contentRect, OnDrawContent, "Flie Content");
            dragRect = GUILayout.Window(2, dragRect, OnDragLine, "",GUI.skin.label);
            GUILayout.EndHorizontal();
            EndWindows(); 


        }

       
        class FileDesc 
        { 
            public string name { get; private set; }
            public int targetInstaneId { get; private set; }

            public FileDesc(int _targetInstaneId, string _name) 
            {
                targetInstaneId = _targetInstaneId;
                name = _name;
            }
        }

        string PathRoot = "";
        string SearchFile = "";
        string SearchContent = "";
        string AssemlyName = "";
        List<FileDesc> Files = new List<FileDesc>();
        List<FileDesc> FileFilter = new List<FileDesc>();

        FileDesc Select = null;
        FileDesc Focus = null;
        Vector2 ListScrollViewPos=Vector2.zero;
        Vector2 ContentScrollViewPos = Vector2.zero;
        UDB DB = UDB.CreateInstance();
        List<UDBPropertyTitleDB> Titles;
        object[] Contents;
        bool IsSearchView = false;
        int DataCount = 500;
        int PageIndex = 0;
        int PageCount = 0; 
         

        private void OnEnable()
        {
            PathRoot = EditorPrefs.GetString("UDBView.PageRoot","");
            AssemlyName = EditorPrefs.GetString("UDBView.AssemlyName","");
        }

        private void OnDisable()
        {
            DB.Clear();
            EditorPrefs.SetString("UDBView.PageRoot", PathRoot);
            EditorPrefs.SetString("UDBView.AssemlyName", AssemlyName);
        }

        void OnDragLine(int _id) 
        {
            GUILayout.Space(1);
            GUI.DragWindow(); 
        }

        bool Seem(FileDesc _a,FileDesc _b) 
        {
            if (_a == null) return false;
            if (_b == null) return false;
            return _a.targetInstaneId == _b.targetInstaneId;
        }

        void SearchListFile() 
        {
            string contentBasePath = Application.dataPath.Remove(Application.dataPath.Length - 6, 6);
            string contentAbslPath = Application.dataPath + PathRoot;

            string[] files = Directory.GetFiles(contentAbslPath, "*.txt", SearchOption.AllDirectories);
            Files.Clear();
            FileFilter.Clear();
            foreach (var v in files)
            {
                var obj = AssetDatabase.LoadAssetAtPath(v.Substring(contentBasePath.Length), typeof(TextAsset));
                Files.Add(new FileDesc(obj.GetInstanceID(), Path.GetFileNameWithoutExtension(v)));
            }
        }

        void OnDrawList(int _id) 
        {
            if (GUILayout.Button("Pickup Path")) 
            {
                string path = "";
                foreach(var v in Selection.objects) 
                {
                    path = AssetDatabase.GetAssetPath(v);
                    if (!string.IsNullOrEmpty(path)) 
                    {
                        int nameLen = Path.GetFileName(path).Length;
                        PathRoot = path.Remove(path.Length - nameLen - 1, nameLen + 1);
                        PathRoot = PathRoot.Substring(6);
                        Debug.Log(PathRoot);
                        break;
                    }
                }
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AssemblyName:",GUILayout.MaxWidth(120));
            AssemlyName=EditorGUILayout.TextField(AssemlyName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Path:",GUILayout.MaxWidth(50));
            PathRoot=GUILayout.TextField(PathRoot);
            if (GUILayout.Button("Search", GUILayout.MaxWidth(listRect.width / 4f))) 
            {
                SearchListFile();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.MaxWidth(50));
            string nextSearch = GUILayout.TextField(SearchFile);
            GUILayout.EndHorizontal();

            if (nextSearch != SearchFile) 
            {
                SearchFile = nextSearch;
                FileFilter.Clear(); 
                if (SearchFile != "") 
                {
                    foreach (var v in Files) 
                    {
                        if (v.name.Contains(SearchFile)) 
                        {
                            FileFilter.Add(v);
                        }
                    }
                }
            }

            var show = SearchFile.Length > 0 ? FileFilter : Files;
            ListScrollViewPos=GUILayout.BeginScrollView(ListScrollViewPos);
            GUILayout.BeginVertical(); 
            float normal = listRect.width / 4 * 3;
            float select = listRect.width; 
            foreach (var v in show) 
            {
                bool seem = Seem(Select, v);
                if (GUILayout.Button(v.name, GUILayout.MaxWidth(seem ? select:normal)))
                {
                    if (!seem)
                    {
                        Select = v;
                        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GetAssetPath(v.targetInstaneId));
                        if (asset != null) 
                        {
                            string assemlyName = string.IsNullOrEmpty(AssemlyName) ? "Assembly-CSharp" : AssemlyName;
                            string key = Assembly.CreateQualifiedName(assemlyName, v.name); 
                            System.Type type = System.Type.GetType(key);
                            if (type != null) 
                            {
                                DB.LoadData(type, asset.text);
                                Titles = DB.GetTitle(v.name);
                                SetContents(DB.Select(v.name,"",true));
                            }                            
                        }
                    }
                    else 
                    {
                        if (Seem(Focus,v))
                        {
                            Focus = null;
                        }
                        else
                        {
                            Focus = v;
                            EditorGUIUtility.PingObject(v.targetInstaneId);
                        }
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            if (show.Count == 0) 
            {
                EditorGUILayout.HelpBox("Empty",MessageType.Warning);
            }
            EditorGUILayout.HelpBox(Application.dataPath+PathRoot,MessageType.None);
        }

        

        void OnDrawContent(int _id) 
        {
            GUILayout.Space(1); 
            if (Select == null) 
            {
                EditorGUILayout.HelpBox("Not Selection.",MessageType.None);
                return;
            }
            string key = Select.name;
            int begin = PageIndex * DataCount;
            int end = begin + DataCount;
            if (end > Contents.Length) end = Contents.Length;

            DrawTitle();
            DrawRow(begin,end); 
            DrawPageInfo();

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Search:",GUILayout.MaxWidth(45));
            SearchContent=EditorGUILayout.TextField(SearchContent);
            if (GUILayout.Button("Search")) 
            {
                SetContents(DB.Select(key,SearchContent));
                IsSearchView = true;
            }
            if (GUILayout.Button("Clear")) 
            {
                SearchContent = "";
                if (IsSearchView) 
                {
                    IsSearchView = false;
                    SetContents(DB.Select(key, ""));
                }
            }
            GUILayout.EndHorizontal();
        }

        void SetContents(object[] _contents) 
        {
            Contents = _contents;
            PageIndex = 0;
            int last = (Contents.Length % DataCount) > 0 ? 1 : 0;
            PageCount = Contents.Length / DataCount + last;
        }

        void DrawPageInfo() 
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<-"))
            {
                if (PageIndex > 0)
                {
                    PageIndex -= 1;
                }
            }

            GUILayout.Space(25);
            EditorGUILayout.HelpBox(string.Format("{0}/{1}        Count:{2}", PageIndex + 1,PageCount,Contents.Length),MessageType.None);
            GUILayout.Space(25);
            if (GUILayout.Button("->"))
            {
                if (PageIndex + 1 < PageCount)
                {
                    PageIndex += 1;
                }
            }
            GUILayout.EndHorizontal();
        }

        void DrawRow(int _begin,int _end) 
        {
            ContentScrollViewPos=GUILayout.BeginScrollView(ContentScrollViewPos);
            object v;
            for (int i = _begin; i < _end; i++)
            {
                v = Contents[i];
                GUILayout.BeginHorizontal();
                for (int n = 0; n < Titles.Count; n++) 
                {
                    GUILayout.Button(Titles[n].GetStringValue(v), GUILayout.MaxWidth(250),GUILayout.MinWidth(50));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        void DrawTitle() 
        {
            GUILayout.BeginHorizontal();
            foreach (var v in Titles) 
            {
                GUILayout.Button(v.Name,GUILayout.MaxWidth(250), GUILayout.MinWidth(50));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            foreach (var v in Titles)
            {
                GUILayout.Button(v.TypeName, GUILayout.MaxWidth(250), GUILayout.MinWidth(50));
            }
            GUILayout.EndHorizontal();
        }
    }
}