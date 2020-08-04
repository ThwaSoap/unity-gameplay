using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
namespace GamePlayEditor.AssetBundleTools
{ 
    class EPageNameConfig : EGUI
    {
        EAssetBundleToolConfig Config;
        EGUIVerticalLayout VerticalLayout = new EGUIVerticalLayout();
        EGUIFolderTreeRoot TreeRoot;
        public EPageNameConfig(EAssetBundleToolConfig _config)
        {
            Config = _config; 
            VerticalLayout.Add(new EGUITitle("Module List Option"));
            EGUIHorizontalLayout hLayout = new EGUIHorizontalLayout();
            hLayout.Add(new EGUIButton("Save",OnClickSave));
            hLayout.Add(new EGUIButton("Refresh", OnClickRefresh));
            VerticalLayout.Add(hLayout);
            VerticalLayout.Add(new EGUITitle("Module List")); 
            TreeRoot = new EGUIFolderTreeRoot(Config.ResourcePath);
            ReloadFolderData();
            VerticalLayout.Add(TreeRoot);

        }
        protected override void OnRender()
        {
            VerticalLayout.Render();
        }

        /// <summary>
        /// 点击存储将模块下的配置属性写入本地文件_.config.txt
        /// </summary>
        void OnClickSave()
        {
            foreach (var v in TreeRoot.Tree)
            {
                string fPath = EAssetBundleToolUtil.GetDiskPath(v.RootPath + "/_.config.txt");
                var component = v.GetComponent<EGUIComponentFolderModule>();
                Dictionary<string, List<string>> outConfigMap;
                if (component.TryGetConfigMap(out outConfigMap))
                {
                    File.WriteAllText(fPath, EAssetBundleToolUtil.ConfigMapToString(outConfigMap));
                }
            }
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 点击刷新
        /// </summary>
        void OnClickRefresh()
        {
            TreeRoot.ResetPath(Config.ResourcePath);
            ReloadFolderData();
        }

        /// <summary>
        /// 重新载入文件树结构的数据
        /// </summary>
        void ReloadFolderData()
        {
            foreach (var v in TreeRoot.Tree)
            {
                string fPath = EAssetBundleToolUtil.GetDiskPath(v.RootPath + "/_.config.txt"); 
                bool isEnable = File.Exists(fPath);
                v.SetComponent(new EGUIComponentFolderModule(isEnable, v));
                if (isEnable)
                {
                    v.GetComponent<EGUIComponentFolderModule>().SetConfigMap(EAssetBundleToolUtil.StringToConfigMap(File.ReadAllText(fPath)));
                }
            }
        }
    }
}