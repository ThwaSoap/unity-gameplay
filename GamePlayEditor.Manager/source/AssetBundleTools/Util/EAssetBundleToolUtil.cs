using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace GamePlayEditor.AssetBundleTools
{ 
    class EAssetBundleToolUtil
    {
        

        public static string GetDiskPath(string _relativePath)
        {
            string root = Application.dataPath.Remove(Application.dataPath.Length - "/Assets".Length);
            return Path.Combine(root, _relativePath);
        }

        public static List<string> GetFiles(string _relativePath, SearchOption _options = SearchOption.AllDirectories, Func<string, bool> _invalidFileCallback = null)
        {
            string root = Application.dataPath.Remove(Application.dataPath.Length - "/Assets".Length);
            string path = Path.Combine(root, _relativePath);
            string[] files = Directory.GetFiles(path, "*.*", _options);
            List<string> result = new List<string>();
            foreach (var v in files)
            {
                if (v.EndsWith(".meta")) continue;
                if (_invalidFileCallback != null && _invalidFileCallback(v)) continue;
                result.Add(v);
            }
            return result;
        }



        public static List<string> GetLocalFiles(string _relativePath, SearchOption _options = SearchOption.AllDirectories, Func<string, bool> _invalidFileCallback = null)
        {
            string root = Application.dataPath.Remove(Application.dataPath.Length - "/Assets".Length);
            string path = Path.Combine(root, _relativePath);

            if (!Directory.Exists(path)) return new List<string>();

            string[] files = Directory.GetFiles(path, "*.*", _options);
            List<string> result = new List<string>();
            foreach (var v in files)
            {
                if (v.EndsWith(".meta")) continue;
                if (_invalidFileCallback != null && _invalidFileCallback(v)) continue;
                result.Add(v.Substring(root.Length + 1));
            }
            return result;
        }

        public static List<string> GetLocalDir(string _relativePath, string _searchPattern, SearchOption _options = SearchOption.AllDirectories) 
        {
            string root = Application.dataPath.Remove(Application.dataPath.Length - "/Assets".Length);
            string path = Path.Combine(root, _relativePath);
            List<string> result = new List<string>();
            var list = Directory.GetDirectories(path, _searchPattern, _options);
            foreach (var v in list) 
            {
                result.Add(v.Substring(root.Length + 1));
            } 
            return result;
        }

        /// <summary>
        /// 清除AssetBundle名称
        /// </summary>
        /// <param name="_relativePath">相对工程的文件夹路径</param>
        /// <param name="_invalidFileCallback">无效的文件回调,无效返回true,有效返回false</param>
        public static void ClearAssetBundleName(string _relativePath, Func<string, bool> _invalidFileCallback = null)
        {
            var list = GetLocalFiles(_relativePath, SearchOption.AllDirectories, _invalidFileCallback);
            foreach (var v in list)
            {
                if (v.Contains("\\.")) continue;

                AssetImporter.GetAtPath(v).SetAssetBundleNameAndVariant("", "");
            }
        }


        /// <summary>
        /// 设置资源包的名称
        /// </summary>
        /// <param name="_relativePath">模块的相对路径</param>
        /// <param name="_moduleName">模块的名称</param>
        /// <param name="_fileExtensoin">AB的后缀名</param>
        /// <param name="_invalidFileCallback">无效文件过滤</param>
        public static void SetAssetBundleName(string _relativePath, string _moduleName, string _fileExtensoin, Func<string, bool> _invalidFileCallback = null)
        {
            string[] folders = AssetDatabase.GetSubFolders(_relativePath);
            foreach (var v in folders)
            {
                int index = v.LastIndexOf("/");
                string defaultName = v.Substring(index + 1);
                var list = GetLocalFiles(v, SearchOption.AllDirectories, _invalidFileCallback);
                foreach (var f in list)
                {
                    AssetImporter.GetAtPath(f).SetAssetBundleNameAndVariant(string.Format("{0}_{1}", _moduleName, defaultName), _fileExtensoin);
                }
            }
        }
        public static Dictionary<string, List<string>> GetNameConfig(string _content)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            string[] lines = _content.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            string key = "";
            string current;
            for (int i = 0; i < lines.Length; i++)
            {
                current = lines[i];
                if (current.EndsWith(":"))
                {
                    key = current.Remove(current.Length - 1);
                    if (result.ContainsKey(key) == false)
                    {
                        result.Add(key, new List<string>());
                    }
                }
                else if (key != "")
                {
                    if (result[key].Contains(current) == false)
                    {
                        result[key].Add(current);
                    }
                }
            }
            return result;
        }
        public static void SetAssetBundleNameByGroupList(string _basePath, string _moduleName, List<string> _group, string _extensionName)
        {
            foreach (var v in _group)
            {
                string[] parameters = v.Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
                string path = parameters[0].Trim(' '), value = "";
                if (parameters.Length == 2 && parameters[1] != "")
                {
                    value = parameters[1].Trim(' ');
                }

                string[] folders = path.Split('/');
                string folderName = folders[folders.Length - 1];
                var list = GetLocalFiles(_basePath + "/" + path);
                foreach (var f in list)
                {
                    if (value != "")
                    {
                        AssetImporter.GetAtPath(f).SetAssetBundleNameAndVariant(string.Format("{0}_{1}_{2}", _moduleName, value, folderName), _extensionName);
                    }
                    else
                    {
                        AssetImporter.GetAtPath(f).SetAssetBundleNameAndVariant(string.Format("{0}_{1}", _moduleName, folderName), _extensionName);
                    }
                }
            }
        }

        public static void SetAssetBundleNameBySingleList(string _basePath, string _moduleName, List<string> _singleList, string _extensionName)
        {
            foreach (string v in _singleList)
            {
                string[] parameters = v.Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
                string path = parameters[0].Trim(' '), value = "";
                if (parameters.Length == 2 && parameters[1] != "")
                {
                    value = parameters[1].Trim(' ');
                }

                //var list = GetLocalFiles(_basePath + "/" + path);
                var list = GetLocalFiles(path);
                foreach (var f in list)
                {
                    int index = f.Replace('\\', '/').LastIndexOf('/');

                    string fName = f.Substring(index + 1);
                    index = fName.LastIndexOf(".");
                    fName = fName.Remove(index);
                    if (value != "")
                    {
                        AssetImporter.GetAtPath(f).SetAssetBundleNameAndVariant(string.Format("{0}_{1}_{2}", _moduleName, value, fName), _extensionName);
                    }
                    else
                    {
                        AssetImporter.GetAtPath(f).SetAssetBundleNameAndVariant(string.Format("{0}_{1}", _moduleName, fName), _extensionName);
                    }
                }
            }
        }

        public static void UnsetAssetBundleName(string _basePath,string _folderName) 
        {
            var list = GetLocalDir(_basePath, _folderName); 
            foreach (var v in list) 
            {
                var files = GetLocalFiles(v);
                foreach (var f in files) 
                {
                    AssetImporter.GetAtPath(f).SetAssetBundleNameAndVariant("","");
                }
            }
        }

        /*static List<string> GetGroupOfAssetBundle(string _moduleName)
        {
            string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            List<string> group = new List<string>();
            foreach (var n in assetBundleNames)
            {
                int splitIndex = n.IndexOf("_");
                if (splitIndex > -1)
                {
                    string key = n.Split('_')[0];
                    if (key == _moduleName)
                        group.Add(n);
                }
            }
            return group;
        }*/

        public static string GetMD5(MD5 _md5, string _filePath, out long _outLenth)
        {
            string strMd5 = "";
            _outLenth = 0;
            using (FileStream fs = new FileStream(_filePath, FileMode.Open))
            {
                byte[] retVal = _md5.ComputeHash(fs);
                for (int i = 0; i < retVal.Length; i++)
                {
                    strMd5 += retVal[i].ToString("x2");
                }
                _outLenth = fs.Length;
                fs.Close();
            }
            return strMd5;
        }

        static string GetFileName(string _fileName)
        {
            int idx = _fileName.IndexOf('.');
            if (idx != -1)
            {
                return _fileName.Remove(idx);
            }
            return _fileName;
        }

        static string StrArrayToString(string[] _array)
        {
            if (_array == null) return "";
            string value = "";
            foreach (var v in _array)
            {
                value += GetFileName(v) + ",";
            }
            return value;
        }

        /// <summary>
        /// 创建模块的依赖关系文件
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_moduleName"></param>
        /// <param name="_assetBundleExtension"></param>
        public static void CreateModuleManifest(string _path, string _moduleName,string _assetBundleExtension)
        {
            string manifestContent = "";

            List<string> bundleNameOfModule = GetAssetBundleNamesByModule(_moduleName);

            foreach (var v in bundleNameOfModule)
            {
                string[] refs = AssetDatabase.GetAssetBundleDependencies(v, true);

                if (refs.Length == 0) continue;

                manifestContent += string.Format("{0}:{1}\r\n", GetFileName(v), StrArrayToString(refs));
            }

            string fileDir = GetDiskPath(_path + "/manifest.txt");
            File.WriteAllText(fileDir, manifestContent);
            AssetDatabase.Refresh();
            AssetImporter setting = AssetImporter.GetAtPath(_path + "/manifest.txt");
            setting.SetAssetBundleNameAndVariant(_moduleName, _assetBundleExtension);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 获取该模块下所有的AB名称
        /// </summary>
        /// <param name="_allAssetBundleName"></param>
        /// <param name="_moduleName"></param>
        /// <returns></returns>
        public static List<string> GetAssetBundleNamesByModule(string[] _allAssetBundleName, string _moduleName)
        {
            List<string> result = new List<string>();

            foreach (var v in _allAssetBundleName)
            {
                if (v.StartsWith(_moduleName)) result.Add(v);
            }
            return result;
        }

        public static List<string> GetAssetBundleNamesByModule(string _moduleName)
        {
            return GetAssetBundleNamesByModule(AssetDatabase.GetAllAssetBundleNames(),_moduleName);
        }

        /// <summary>
        /// 根据AB名称获取该模块下的所有的AssetBundle构建对象
        /// </summary>
        /// <returns></returns>
        public static List<AssetBundleBuild> GetBuildMapByAssetBundleNames(List<string> _assetBunleNames)
        {
            List<AssetBundleBuild> result = new List<AssetBundleBuild>();

            foreach (var v in _assetBunleNames)
            {
                var allPath = AssetDatabase.GetAssetPathsFromAssetBundle(v);
                if (allPath.Length == 0) continue;
                AssetBundleBuild map = new AssetBundleBuild();
                string[] nameAndExtension = v.Split('.');
                map.assetBundleName = nameAndExtension[0];
                map.assetBundleVariant = nameAndExtension[1];
                map.assetNames = allPath;
                result.Add(map);
            }
            return result;
        }


        public static string ConfigMapToString(Dictionary<string, List<string>> _map)
        {
            string content = "";

            foreach (var v in _map)
            {
                content += v.Key + ":\r\n";
                foreach (var l in v.Value)
                {
                    content += l + "\r\n";
                }
            }
            return content;
        }

        public static Dictionary<string, List<string>> StringToConfigMap(string _content)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            string[] lines = _content.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            string key = "";
            string current;
            for (int i = 0; i < lines.Length; i++)
            {
                current = lines[i];
                if (current.EndsWith(":"))
                {
                    key = current.Remove(current.Length - 1);
                    if (result.ContainsKey(key) == false)
                    {
                        result.Add(key, new List<string>());
                    }
                }
                else if (key != "")
                {
                    if (result[key].Contains(current) == false)
                    {
                        result[key].Add(current);
                    }
                }
            }
            return result;
        }
    }  
}

