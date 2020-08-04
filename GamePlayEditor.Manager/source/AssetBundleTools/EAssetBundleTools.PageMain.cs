using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System;

namespace GamePlayEditor.AssetBundleTools
{
    class EPageMain : EGUI
    {
        const string TITLE_BUILD_TARGET_CURRENT = "Current Platform";
        const string TITLE_BUILD_TARGET_SELECT = "All Platfrom";
        EAssetBundleToolConfig Config;
        EGUIVerticalLayout VerticalLayout = new EGUIVerticalLayout();
        EGUIFolderList FolderList;
        EGUIEnumPopup BuildTargetDropdown;
        Regex regExp = new Regex("[ \\[ \\] \\^ \\-_*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]");
        
        Action<string,string> OnSetModuleName;
        Action<string, string> OnClearModuleName;
        Func<string, bool> IsInvalidFile;

        /// <summary>
        /// 是否将打包完成AB文件拷贝到工程目录
        /// </summary>
        bool IsCopyToProject = false;

        public EPageMain(EAssetBundleToolConfig _config,Action<string,string> _eventSetModuleName,Action<string,string> _eventClearModuleName, Func<string, bool> _eventIsInvalidFile)
        {
            Config = _config;
            OnSetModuleName = _eventSetModuleName;
            OnClearModuleName = _eventClearModuleName;
            IsInvalidFile = _eventIsInvalidFile;
            VerticalLayout.Add(new EGUITitle("Base Setting"));
            VerticalLayout.Add(new EGUIPathInput("ResourceRoot:", Config.ResourcePath, EventSaveResourceRoot));
            VerticalLayout.Add(new EGUITextInput("AssetBundleOutput:",Config.AssetBundleOutput,EventSaveAssetBundleRoot));
            VerticalLayout.Add(new EGUITextInput("AssetBundleExtension:", Config.AssetBundleExtension, EventAssetBundleExtension));
            VerticalLayout.Add(new EGUITextInput("NotAssetBundleFolderName:",Config.NotAssetBundleFolderName, EventNotAssetBundleFolderName));
            VerticalLayout.Add(new EGUIFilePathInput("VersionListTemplate:", Config.VersionListTemplateFile, EventVersionListTemplate));

            VerticalLayout.Add(new EGUITitle("Module List Options"));
            EGUIHorizontalLayout hLayout = new EGUIHorizontalLayout();
            hLayout.Add(new EGUIButton("Choose All", OnClickChooseAll));
            hLayout.Add(new EGUIButton("Cancel All", OnClickCancelAll));
            hLayout.Add(new EGUIButton("Refresh",OnClickRefresh));
            VerticalLayout.Add(hLayout);
            VerticalLayout.Add(new EGUITitle("Module List"));
            FolderList = new EGUIFolderList(_config.ResourcePath);
            VerticalLayout.Add(FolderList);
            
            VerticalLayout.Add(new EGUITitle("Build Options"));
            BuildTargetDropdown = new EGUIEnumPopup(BuildTarget.Android);
            BuildTargetDropdown.Visible = false;
            VerticalLayout.Add(new EGUIToolBar(new string[] { TITLE_BUILD_TARGET_CURRENT, TITLE_BUILD_TARGET_SELECT },0,OnBuildTargetToolbarChanged));
            VerticalLayout.Add(BuildTargetDropdown);

            /*
             * 是否为模块创建依赖文件：
             * 无论你选择是还是否，在设置资源包名称的时候，都会主动为模块创建模块之间的以来关系文件
             *  如果选择是：
             *      在构建AB文件时,模块的依赖关系文件将会被构建为[ModuleName.bytes]的一个AB文件
             *  否则：
             *      在创建AB资源时，不会创建[ModuleName.bytes],同样该文件信息也不会写入版本文件中    
             */
            VerticalLayout.Add(new EGUIToggle("Create Dependence For Module", Config.IsCreateDependence, v => Config.IsCreateDependence = v));

            /*
             * 是否创建版本文件
             * 允许导出的字段有：
             *     RelativePath(相对于Version.List的路径),文件名称(文件名称),MD5(唯一标识),Size(文件大小),Module(模块名称)
             * 如果选择是：
             *     构建AB完成后将会创建Version.list文件在导出目录的跟目录下
             * 否则：
             *     不会创建任何东西
             * 注：支持导出：csv,json,制表符,xml等格式的版本列表文件
             */
            VerticalLayout.Add(new EGUIToggle("Create Version List For Module", Config.IsCreateVersionList, v => Config.IsCreateVersionList = v));

            /*
             * 是否使用模块化的目录结构对于输出的AB文件
             * 如果选择是：
             *     在构建时会为模块创建单独的目录，目录的名称为模块名称
             * 否则：
             *     所有的导出文件全部在根目录
             */
            VerticalLayout.Add(new EGUIToggle("Use Module Structure For AssetBunle File", Config.IsModuleStructure, v => Config.IsModuleStructure = v));

            /*
             * 导出的Version.list文件是否使用模块化结构
             * 如果选择是：
             *     单独的模块生成单独的模块文件夹和单独的Version.list文件，Version.list中只记录当前模块占用的资源信息
             * 否则：
             *     只产生一个Version.List文件，记录所有模块资源的版本信息
             */
            VerticalLayout.Add(new EGUIToggle("Use Module Structure For VersionList File", Config.IsModuleStructureForVersion, v => Config.IsModuleStructureForVersion = v));

            hLayout = new EGUIHorizontalLayout();
            hLayout.Add(new EGUIButton("Clear AB Name", OnClearAssetBundleName));
            hLayout.Add(new EGUIButton("Set AB Name", OnSetAssetBundleName));
            hLayout.Add(new EGUIButton("Output AssetBundle", OnClickOutputAssetBundle));
            VerticalLayout.Add(hLayout);
        }

        protected override void OnRender()
        {
            VerticalLayout.Render();
        }

        /// <summary>
        /// 模块跟目录发生了变化
        /// </summary>
        /// <param name="_inputPath"></param>
        /// <returns></returns>
        string EventSaveResourceRoot(string _inputPath)
        {
            _inputPath = _inputPath.TrimEnd('/', '\\').TrimStart('/', '\\');

            if (AssetDatabase.IsValidFolder("Assets/" + _inputPath))
            {
                Config.ResourcePath = _inputPath;
                FolderList.ResetPath(_inputPath);
            }
            else
            {
                Debug.LogError("Invalid Resource Root Path:" + _inputPath);
            }

            return Config.ResourcePath;
        }

        /// <summary>
        /// AssetBundle的输出跟目录发生了变化
        /// </summary>
        /// <param name="_inputPath"></param>
        /// <returns></returns>
        string EventSaveAssetBundleRoot(string _inputPath)
        {
            _inputPath = _inputPath.TrimEnd('/', '\\').TrimStart('/', '\\');
            Config.AssetBundleOutput = _inputPath;
            return Config.AssetBundleOutput;
        }

        /// <summary>
        /// AssetBundle的扩展名发生了变化
        /// </summary>
        /// <param name="_input"></param>
        /// <returns></returns>
        string EventAssetBundleExtension(string _input)
        { 
            if (regExp.IsMatch(_input))
            {
                Debug.LogError("Invalid Extension Name!");
            }
            else
            {
                Config.AssetBundleExtension = _input;
            }
            return Config.AssetBundleExtension;
        }

        string EventNotAssetBundleFolderName(string _input) 
        {
            Config.NotAssetBundleFolderName = _input;
            return Config.NotAssetBundleFolderName;
        }

        string EventVersionListTemplate(string _filePath)
        {
            if (EditorGamePlay.IsValidSubAsset(_filePath))
            {
                Config.VersionListTemplateFile = _filePath;
            }
            else
            {
                Debug.LogError("The file missing?");
            }
            return Config.VersionListTemplateFile;
        }

        /// <summary>
        /// 选中所有模块选项
        /// </summary>
        void OnClickChooseAll()
        {
            FolderList.SetAll(true);
        }

        /// <summary>
        /// 取消所有模块选项
        /// </summary>
        void OnClickCancelAll()
        {
            FolderList.SetAll(false);
        }

        /// <summary>
        /// 刷新模块列表
        /// </summary>
        void OnClickRefresh()
        {
            FolderList.ResetPath(Config.ResourcePath);
        }

        /// <summary>
        /// 清除被选中模块AssetBundle的名称
        /// </summary>
        void OnClearAssetBundleName()
        {
            var foldersObject = FolderList.GetChooseFolder();
            foreach (var v in foldersObject)
            {
                EAssetBundleToolUtil.ClearAssetBundleName(v.RelativePath);
                System.IO.File.Delete(EAssetBundleToolUtil.GetDiskPath(v.RelativePath + "/manifest.txt"));
                System.IO.File.Delete(EAssetBundleToolUtil.GetDiskPath(v.RelativePath + "/manifest.txt.meta"));
                OnClearModuleName(v.FolderName, v.RelativePath);
            }
            AssetDatabase.Refresh();
            Debug.Log("Clear AssetBundle Name Complete!");
        }

        /// <summary>
        /// 设置被选中模块AssetBundle的名称
        /// </summary>
        void OnSetAssetBundleName()
        {
            var foldersObject = FolderList.GetChooseFolder();
            
            for(int i = 0; i < foldersObject.Count; i++)
            {
                var v = foldersObject[i];
                string moduleName = v.FolderName.ToLower();
                OnSetModuleName(v.FolderName, v.RelativePath);
                EAssetBundleToolUtil.SetAssetBundleName(v.RelativePath, moduleName, Config.AssetBundleExtension, EventOnInvalidFile);
                TextAsset config = AssetDatabase.LoadAssetAtPath<TextAsset>(v.RelativePath + "/_.config.txt");
                if (config != null)
                {
                    Dictionary<string, List<string>> define = EAssetBundleToolUtil.GetNameConfig(config.text);
                    if (define.ContainsKey("Group")) EAssetBundleToolUtil.SetAssetBundleNameByGroupList(v.RelativePath, moduleName, define["Group"], Config.AssetBundleExtension);
                    if (define.ContainsKey("Single")) EAssetBundleToolUtil.SetAssetBundleNameBySingleList(v.RelativePath, moduleName, define["Single"], Config.AssetBundleExtension);
                }
                EAssetBundleToolUtil.UnsetAssetBundleName(v.RelativePath, Config.NotAssetBundleFolderName);
                EditorUtility.DisplayProgressBar("Set AssetBundle Name",v.FolderName, (i+1) / foldersObject.Count * 0.5f);
            }

            for (int i = 0; i < foldersObject.Count; i++)
            {
                var v = foldersObject[i];
                EAssetBundleToolUtil.CreateModuleManifest(v.RelativePath,v.FolderName.ToLower(),Config.AssetBundleExtension);
                EditorUtility.DisplayProgressBar("Create Manifest File", v.FolderName, (i + 1) / foldersObject.Count * 0.5f + 0.5f);
            }
            EditorUtility.ClearProgressBar();
            Debug.Log("Set AssetBundle Name Complete!");
        }

        bool EventOnInvalidFile(string _fileExtension)
        {
            return IsInvalidFile(_fileExtension);
        }


        /// <summary>
        /// 目标平台选择模式发生变化(选择当前平台|从所有平台中选择)
        /// </summary>
        /// <param name="_model"></param>
        void OnBuildTargetToolbarChanged(string _model)
        {
            if(_model == TITLE_BUILD_TARGET_SELECT)
                BuildTargetDropdown.Visible = true;
            else
                BuildTargetDropdown.Visible = false;
        }

        /// <summary>
        /// 点击输出AssetBundle文件
        /// </summary>
        void OnClickOutputAssetBundle()
        {
            //> 确定目标平台
            BuildTarget selection = EditorUserBuildSettings.activeBuildTarget;
            if (BuildTargetDropdown.Visible)
            {
                selection = (BuildTarget)BuildTargetDropdown.Value;
            }

            //> 确定输出模块
            var foldersObject = FolderList.GetChooseFolder();

            if (foldersObject.Count == 0)
            {
                Debug.LogError("Invalid build:Chose at least one module.");
            }

            AssetDatabase.RemoveUnusedAssetBundleNames();
            var allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            var outputPath1 = EAssetBundleToolUtil.GetDiskPath("AssetBundlePool/" + selection.ToString());
            var outputPath2 = EAssetBundleToolUtil.GetDiskPath(Config.AssetBundleOutput + "/" + selection.ToString());

            if (!Directory.Exists(outputPath1)) Directory.CreateDirectory(outputPath1);
            if (!Directory.Exists(outputPath2)) Directory.CreateDirectory(outputPath2);
             
            List<FileVersionInfo> versionList = new List<FileVersionInfo>();
            //> 构建AB文件到资源池
            foreach(var v in foldersObject)
            {
                var names = EAssetBundleToolUtil.GetAssetBundleNamesByModule(allAssetBundleNames, v.FolderName.ToLower());

                var buildMap = EAssetBundleToolUtil.GetBuildMapByAssetBundleNames(names);

                if (buildMap.Count == 0) continue;

                string finalOutputPath1 = outputPath1;
                string finalOutputPath2 = outputPath2;
                if (Config.IsModuleStructure)
                {
                    finalOutputPath1 += "/" + v.FolderName.ToLower();
                    finalOutputPath2 += "/" + v.FolderName.ToLower();
                    if (!Directory.Exists(finalOutputPath1)) Directory.CreateDirectory(finalOutputPath1);
                    if (!Directory.Exists(finalOutputPath2)) Directory.CreateDirectory(finalOutputPath2);
                }

                BuildPipeline.BuildAssetBundles(finalOutputPath1, buildMap.ToArray(), BuildAssetBundleOptions.StrictMode, selection);  
                //> 拷贝AB文件到输出目录
                List<FileVersionInfo> moduleVersion = new List<FileVersionInfo>();

                foreach (var f in names)
                {
                    if (File.Exists(finalOutputPath1 + "/" + f))
                    {
                        File.Copy(finalOutputPath1 + "/" + f, finalOutputPath2 + "/" + f, true);
                        //> 创建版本文件信息
                        moduleVersion.Add(TiyVersionListCreator.GetVersionInfoByFile(outputPath2, finalOutputPath2 + "/" + f, v.FolderName.ToLower()));
                    }
                    else Debug.LogErrorFormat("File is missing:{0}", finalOutputPath1 + "/" + f);

                }

                if (Config.IsCreateVersionList)
                {
                    if (Config.IsModuleStructureForVersion)
                    {
                        string versionFilePath = "";
                        if (Config.IsModuleStructure)
                        {
                            versionFilePath = outputPath2 + "/" + v.FolderName.ToLower() + "/" + "version.list";
                        }
                        else
                        {
                            versionFilePath = outputPath2 + "/" + v.FolderName.ToLower() + "." + "version.list";
                        }
                        TiyVersionListCreator.CreateVersionListByTemplate(moduleVersion, Config.FullVersionListTemplateFile, versionFilePath);
                    }
                    else
                    {
                        versionList.AddRange(moduleVersion);
                    }
                }
            }

            if (Config.IsCreateVersionList && versionList.Count > 0)
            {
                TiyVersionListCreator.CreateVersionListByTemplate(versionList, Config.FullVersionListTemplateFile, outputPath2 + "/version.list");
            }
             

            System.Diagnostics.Process.Start(outputPath2);
            Debug.Log("Build Complete!");
            GUIUtility.ExitGUI();
        }

        
    }
}
