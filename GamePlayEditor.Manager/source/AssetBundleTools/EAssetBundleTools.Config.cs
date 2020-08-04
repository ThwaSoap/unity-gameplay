using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GamePlayEditor.AssetBundleTools
{
    class EAssetBundleToolConfig
    {
        class Config
        {
            /// <summary>
            /// 需要被打包的资源根目录
            /// </summary>
            public string ResourceRootPath = "MyResources";
            /// <summary>
            /// 输出目录
            /// </summary>
            public string AssetBundleOutput = "AssetBundles";
            /// <summary>
            /// AB文件的扩展名
            /// </summary>
            public string AssetBundleExtension = "bytes";

            public string NotAssetBundleFolderName = "not assetbundle";
            
            public string VersionListTemplateFile = "GamePlay/Editor/Mangers/AssetBundleTools/Res/Template/json.txt";
            /// <summary>
            /// 是否采用模块结构生成AB文件
            /// </summary>
            public bool IsModuleStructure=false;
            /// <summary>
            /// 是否为模块创建以来关系文件
            /// </summary>
            public bool IsCreateDependence=true;
            /// <summary>
            /// 是否创建版本列表文件
            /// </summary>
            public bool IsCreateVersionList=true;
            /// <summary>
            /// 对版本文件是否使用模块化结构
            /// </summary>
            public bool IsModuleStructureForVersion=false;
        }

        public event System.Action<string> OnPropertyChanged;
        public const string EVENT_RESOURCEROOT="ResourceRoot";
        Config ConfigObject;

        #region 初始化接口
        Config GetDefaultConfig()
        {
            Config config = new Config();
            return config;
        }

        public void LoadConfig()
        {
            string jsonText = EditorPrefs.GetString("EAssetBundleTools.Config", "");

            ConfigObject = JsonUtility.FromJson<Config>(jsonText);

            if (ConfigObject == null)
            {
                ConfigObject = GetDefaultConfig();
            }
        }

        public void ResetConfig()
        {
            ConfigObject = GetDefaultConfig();
        }

        public void SaveConfig()
        {
            string jsonText = JsonUtility.ToJson(ConfigObject);
            EditorPrefs.SetString("EAssetBundleTools.Config", jsonText);
        }
        #endregion

        #region 属性字段接口
        public string ResourcePath {
            get { return ConfigObject.ResourceRootPath; }
            set {
                OnPropertyChanged?.Invoke(EVENT_RESOURCEROOT);
                ConfigObject.ResourceRootPath = value;
            }
        }

        public string AssetBundleOutput {
            get { return ConfigObject.AssetBundleOutput; }
            set
            {
                ConfigObject.AssetBundleOutput = value;
            }
        }

        public string AssetBundleExtension
        {
            get { return ConfigObject.AssetBundleExtension; }
            set
            {
                ConfigObject.AssetBundleExtension = value;
            }
        }

        public string NotAssetBundleFolderName 
        {
            get { return ConfigObject.NotAssetBundleFolderName; }
            set { ConfigObject.NotAssetBundleFolderName = value; }
        }

        public bool IsModuleStructure
        {
            get { return ConfigObject.IsModuleStructure; }
            set
            {
                ConfigObject.IsModuleStructure = value;
            }
        }

        public bool IsCreateDependence
        {
            get { return ConfigObject.IsCreateDependence; }
            set
            {
                ConfigObject.IsCreateDependence = value;
            }
        }

        public bool IsCreateVersionList
        {
            get { return ConfigObject.IsCreateVersionList; }
            set
            {
                ConfigObject.IsCreateVersionList = value;
            }
        }

        public bool IsModuleStructureForVersion
        {
            get { return ConfigObject.IsModuleStructureForVersion; }
            set
            {
                ConfigObject.IsModuleStructureForVersion = value;
            }
        }

        public string VersionListTemplateFile
        {
            get { return ConfigObject.VersionListTemplateFile; }
            set
            {
                ConfigObject.VersionListTemplateFile = value;
            }
        }

        public string FullVersionListTemplateFile 
        {
            get 
            {
                return Application.dataPath + VersionListTemplateFile; 
            }
        }
        #endregion
    }
}
