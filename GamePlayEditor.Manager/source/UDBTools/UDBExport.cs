using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GamePlayEditor.UDBTool 
{ 
    
    public class UDBExport : EditorWindow
    {
        [MenuItem("GamePlayTools/UDBTools/UDBExportTool")]
        static void Init()
        {
            var window = (UDBExport)EditorWindow.GetWindow(typeof(UDBExport), false, "UDBExportTool");
            
            window.Show();
        }

        string ConfigPath()
        {
            return EditorPrefs.GetString("GamePlay.UDB.Config", "GamePlay/Editor/Config/Config.ini");
        }

        Vector2 ScrollViewPos = Vector2.zero;   
        static List<Thread> EmployeeThread = new List<Thread>();
        static RuntimeWoker Worker = new RuntimeWoker();
        Config Config;
        ConfigPropertys ConfigPropertys;

        static int SheetIndex=0, SheetCount =0;
        static int RowIndex=0, RowCount=0;
        static int FileIndex=0, FileCount=0;
        static string SheetName = "", ExcelName="";

        static CancellationTokenSource CancelController;
        static Task Content;
       
        EGUIHorizontalLayout HLayout;
        private void OnEnable()
        {
            HLayout = new EGUIHorizontalLayout();
            var configInput = new EGUIFilePathInput("ConfigPath",EditorPrefs.GetString("GamePlay.UDB.Config","GamePlay/Editor/Config/Config.ini"),OnConfigPathChanged);
            HLayout.Add(configInput);
            HLayout.Add(new EGUIButton("LoadConfig", () =>
            {
                string filePath = GetConfigFilePath();
                if (File.Exists(filePath)) 
                {
                    Config.Initialized(filePath);
                    ConfigPropertys = new ConfigPropertys(Config);
                }
            }));

            Config = new Config();
            Config.Initialized(GetConfigFilePath());
            ConfigPropertys = new ConfigPropertys(Config);
        }

        string GetConfigFilePath() 
        {
            string assetPath = EditorPrefs.GetString("GamePlay.UDB.Config","GamePlay/Editor/Config/Config.ini").TrimStart('\\').TrimStart('/');
            string basePath = Application.dataPath.TrimEnd('\\').TrimEnd('/');
            return string.Format("{0}/{1}", basePath, assetPath);
        }

        string OnConfigPathChanged(string newPath) 
        {
            string filePath = string.Format("{0}/{1}", Application.dataPath, newPath);
            if (File.Exists(filePath)) 
            {
                EditorPrefs.SetString("GamePlay.UDB.Config",newPath);
                return newPath;
            }
            return ConfigPath();
        }

        private void OnDisable()
        { 
            Config.SaveConfig(GetConfigFilePath(), ConfigPropertys);
            Config = null;
        }
         
        
        private void OnInspectorUpdate() 
        {
            if (Content != null) 
            {
                if (Content.IsCompleted || Content.IsCanceled || Content.IsFaulted)
                {
                    Content = null;
                    AssetDatabase.Refresh();
                }

                this.Repaint();
            }
                
        } 

        private void OnGUI()
        {
            GUILayout.Label("Config", EditorStyles.boldLabel);
            HLayout.Render();
            ScrollViewPos =GUILayout.BeginScrollView(ScrollViewPos); 
            GUILayout.Label("Base Setting", EditorStyles.boldLabel);
            ConfigPropertys.InputPath = EditorGUILayout.DelayedTextField("InputPath", ConfigPropertys.InputPath);
            ConfigPropertys.OutputDataPath = EditorGUILayout.DelayedTextField("OutputDataPath", ConfigPropertys.OutputDataPath);
            ConfigPropertys.ContainerFile = EditorGUILayout.DelayedTextField("ContainerFile", ConfigPropertys.ContainerFile);
            ConfigPropertys.ContainerSheet = EditorGUILayout.DelayedTextField("ContainerSheet", ConfigPropertys.ContainerSheet);
            ConfigPropertys.IsCreateFolder = EditorGUILayout.Toggle("IsCrateFolder", ConfigPropertys.IsCreateFolder);
            ConfigPropertys.IsIgnoreCaseFile = EditorGUILayout.Toggle("IsIgnoreCaseFile", ConfigPropertys.IsIgnoreCaseFile);
            ConfigPropertys.IsIgnoreCaseSheet = EditorGUILayout.Toggle("IsIgnoreCaseSheet", ConfigPropertys.IsIgnoreCaseSheet);
            ConfigPropertys.ExportFileType = (EExportFileType)EditorGUILayout.EnumPopup("ExportFileType", ConfigPropertys.ExportFileType);
            ConfigPropertys.DescriptionRow = EditorGUILayout.DelayedIntField("DescriptionRow", ConfigPropertys.DescriptionRow);
            ConfigPropertys.PropertyTypeRow = EditorGUILayout.DelayedIntField("PropertyTypeRow", ConfigPropertys.PropertyTypeRow);
            ConfigPropertys.PropertyNameRow = EditorGUILayout.DelayedIntField("PropertyNameRow", ConfigPropertys.PropertyNameRow);
            ConfigPropertys.StartExportRow = EditorGUILayout.DelayedIntField("StartExportRow", ConfigPropertys.StartExportRow);

            ConfigPropertys.IsOutputCode = EditorGUILayout.BeginToggleGroup("IsOutputCode", ConfigPropertys.IsOutputCode);
            GUILayout.Label("OutputCode Setting", EditorStyles.boldLabel);
            ConfigPropertys.OutputCodePath = EditorGUILayout.DelayedTextField("OutputCodePath", ConfigPropertys.OutputCodePath);
            ConfigPropertys.ContainerNamespace = EditorGUILayout.DelayedTextField("ContainerNamespace", ConfigPropertys.ContainerNamespace);
            ConfigPropertys.OutputCodeFileName = EditorGUILayout.DelayedTextField("OutputCodeFileName", ConfigPropertys.OutputCodeFileName);
            if (ConfigPropertys.ExportFileType == EExportFileType.UnicodeText)
            {
                ConfigPropertys.IsDefineUDBDatasetProperty = EditorGUILayout.Toggle("IsDefineUDBDataSet", ConfigPropertys.IsDefineUDBDatasetProperty);
            }
            EditorGUILayout.EndToggleGroup(); 
            GUILayout.EndScrollView();
            

            if (Content==null)
            {
                if (GUILayout.Button("Export", GUILayout.Height(45)))
                {
                    SheetIndex = SheetCount = RowIndex = RowCount = FileIndex = FileCount = 0;
                    SheetName = "";
                    ExcelName = "";
                    CancelController = new CancellationTokenSource();
                    Content = Task.Run(ThreadJob,CancelController.Token); 
                }
            }
            else
            {
                //> 绘制进度
                GUILayout.BeginHorizontal();
                float sheetProgress = (float)RowIndex / RowCount;
                float excelProgress = (float)SheetIndex / SheetCount;
                if (sheetProgress < 1) excelProgress += (1f / SheetCount) * sheetProgress;

                float allProgress = (float)FileIndex / FileCount;
                if (excelProgress < 1) allProgress += (1f / FileCount) * excelProgress;

                string showSheetProgressText = "Sheet:" + SheetName + "[" + RowIndex + "/" + RowCount + "]";
                string showExcelProgressText = "Excel:"+ ExcelName + ":" + (excelProgress * 100).ToString("0.00") + "%";
                string showAllProgressText = "Files:" + FileIndex + "/" + FileCount;
                  
                EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(15, 15, "TextField"), sheetProgress, showSheetProgressText);
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(15, 15, "TextField"), excelProgress, showExcelProgressText);
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(15, 15, "TextField"), allProgress, showAllProgressText);
                EditorGUILayout.EndVertical();
                if (GUILayout.Button("Cancel", GUILayout.Height(45)))
                {
                    CancelController.Cancel(); 
                }
                GUILayout.EndHorizontal();
            }
        }

        void ThreadJobCallback(int _sheetIndex, int _sheetCount, string _sheetName, int _rowIndex, int _rowCount) 
        {
            SheetIndex = _sheetIndex;
            SheetCount = _sheetCount;
            RowIndex = _rowIndex;
            RowCount = _rowCount;
            SheetName = _sheetName; 
        }
         
        //> 转换工作的线程
        void ThreadJob()
        {
            bool exit = false;
            if (!Worker.UrlChecker(ConfigPropertys.GetInUrl(), ConfigPropertys.IsCreateFolder)) exit = true;
            if (!Worker.UrlChecker(ConfigPropertys.GetOutputDataPath(), ConfigPropertys.IsCreateFolder)) exit = true;
            if (!Worker.UrlChecker(ConfigPropertys.GetOutputCodePath(), ConfigPropertys.IsCreateFolder) && ConfigPropertys.IsOutputCode) exit = true;
            if (exit)
            {
                UnityEngine.Debug.LogError("Some folders don't exist. Please To check and create the folder or open the 'IsCreateFolder' option."); 
                return;
            }

            ConfigPropertys property = ConfigPropertys;
            CancellationToken token = CancelController.Token;
            List<string> excelFiles = new List<string>();
            List<ClassInfo> codedatas = new List<ClassInfo>();
            
            Worker.GetUrlOfXlsxFiles(property.GetInUrl(), property.GetContainerFiles(), property.IsIgnoreCaseFile, ref excelFiles);
            FileCount = excelFiles.Count;
            
            //> 转换和解析数据
            for (FileIndex = 0; FileIndex < FileCount; FileIndex++) 
            {
                if (token.IsCancellationRequested) break;
                
                string url = excelFiles[FileIndex];
                ExcelName = Path.GetFileName(url);
                RowIndex = RowCount = 0;
                SheetIndex = SheetCount = 0;

                //> 到处一个excel中所有的sheet
                var collectionData = Worker.ExportFile(
                    url, property.GetOutputDataPath(),
                    property.IsIgnoreCaseSheet,
                    property.IsRemoveEmptyLine,
                    property.GetContainerSheets(),
                    property.StartExportRow,
                    property.GetExportFileSplitSign(),
                    property.GetExportFileExtensionName(),
                    property.IsOutputCode,
                    property.PropertyTypeRow,
                    property.PropertyNameRow,
                    property.DescriptionRow,
                    CancelController,
                    ThreadJobCallback
                );

                //ThreadJobCallback(SheetCount, SheetCount, SheetName, RowCount, RowCount);
                
                //> 需要生成的代码数据
                codedatas.AddRange(collectionData.ClassInfos); 

                foreach (var v in collectionData.TableContent) 
                {
                    File.WriteAllText(string.Format("{0}/{1}", property.GetOutputDataPath(), v.Key), v.Value);
                }
            }

            //> 创建数据
            if (!property.IsOutputCode || token.IsCancellationRequested) 
                return;

            Worker.ExportCodeFile(
                property.GetOutputCodePath(),
                property.OutputCodeFileName,
                property.GetCodeNamespace(),
                property.IsDefineUDBDatasetProperty,
                codedatas );
        }
    }
}