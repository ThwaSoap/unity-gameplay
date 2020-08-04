using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GamePlayEditor.AssetBundleTools
{ 
    public class EAssetBundleTools : EditorWindow
    {
        EAssetBundleToolConfig Configer;
        
        /// <summary>
        /// 所有的页面实例
        /// </summary>
        Dictionary<string, EGUI> Pages = new Dictionary<string, EGUI>();
        /// <summary>
        /// 当前显示的页面
        /// </summary>
        EGUI CurrentPage;
        EGUIToolBar ToolBar;
        
        #region create window instance on here
        [MenuItem("GamePlayTools/AssetBundleTool")]
        static void Init()
        {
             EditorWindow.GetWindow<EAssetBundleTools>("AssetBundleTools",true).Show();
        }
        #endregion

        #region how to init the window and destory it.
        protected virtual void OnEnable()
        {
            Configer = new EAssetBundleToolConfig();
            Configer.LoadConfig();

            Pages = new Dictionary<string, EGUI>();
            Pages.Add("AssetBundleNameConfig", new EPageNameConfig(Configer));
            Pages.Add("Main",new EPageMain(Configer, OnSetModuleName, OnClearModuleName,IsInvalidFile));

            ToolBar = new EGUIToolBar(new string[] {
                "Main",
                "AssetBundleNameConfig"
            },0,
            (choose)=> {
                CurrentPage = Pages[choose];
            }); 
        }
        
        void OnDisable()
        {
            if(Configer != null) Configer.SaveConfig();
        }
        #endregion

        protected virtual void OnGUI()
        {
            if (ToolBar != null) ToolBar.Render();
            if (CurrentPage != null)
            {
                CurrentPage.Render();
            }
        }

        protected virtual void OnSetModuleName(string _moduleName,string _path)   { }
        protected virtual void OnClearModuleName(string _moduleName,string _path) { }
        protected virtual bool IsInvalidFile(string _extension) { return false; }
    }
}
