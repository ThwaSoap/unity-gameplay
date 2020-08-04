using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GamePlayEditor.AssetBundleTools
{
    class EGUIComponentFolderModule : EGUIComponent
    {
        public bool IsEnable = false;
        public EGUIFolderTree ModuleRoot;

        public EGUIComponentFolderModule(bool _isEnable, EGUIFolderTree _moduleRoot)
        {
            ModuleRoot = _moduleRoot;
            if (IsEnable = _isEnable)
            {
                foreach (var v in ModuleRoot.Tree)
                {
                    v.SetComponent(new EGUIComponentFolderItem(v));
                }
            }
        }

        public void SetConfigMap(Dictionary<string, List<string>> _map)
        {
            if (IsEnable == false) return;
            foreach (var v in ModuleRoot.Tree)
            {
                v.GetComponent<EGUIComponentFolderItem>().ReadConfigFromMap(_map);
            }
        }

        public bool TryGetConfigMap(out Dictionary<string, List<string>> _outMap)
        {
            _outMap = null;
            if (IsEnable)
            {
                _outMap = new Dictionary<string, List<string>>();
                _outMap.Add("Single", new List<string>());
                _outMap.Add("Group", new List<string>());
                _outMap.Add("None", new List<string>());

                foreach (var v in ModuleRoot.Tree)
                {
                    v.GetComponent<EGUIComponentFolderItem>().WriteConfigToMap(_outMap);
                }
                return _outMap.Count > 0;
            }
            return false;
        }

        protected override void OnRender()
        {
            if (IsEnable)
            {
                if (GUILayout.Button("Del Config", GUILayout.ExpandWidth(false)))
                {
                    IsEnable = false;
                    string filePath = Path.Combine(EAssetBundleToolUtil.GetDiskPath(ModuleRoot.RootPath), "_.config.txt");
                    File.Delete(filePath);
                    File.Delete(filePath + ".meta");
                    ModuleRoot.RemoveAllComponentsOfChild();
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                if (GUILayout.Button("Create Config", GUILayout.ExpandWidth(false)))
                {
                    IsEnable = true;
                    string filePath = Path.Combine(EAssetBundleToolUtil.GetDiskPath(ModuleRoot.RootPath), "_.config.txt");
                    File.WriteAllText(filePath, "");

                    foreach (var v in ModuleRoot.Tree)
                    {
                        v.SetComponent(new EGUIComponentFolderItem(v));
                    }

                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
