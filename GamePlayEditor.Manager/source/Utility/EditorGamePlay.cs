using System.IO;
using UnityEditor;
using UnityEngine;

namespace GamePlayEditor
{
    public static class EditorGamePlay 
    {
        /// <summary>
        /// 尝试拾取被选中资源的相对路径，相对路径中不包含"Assets/"
        /// </summary>
        /// <param name="_value"></param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool TryPickupAssetPath(out string _value) 
        {
            _value = null;
            foreach (var v in Selection.objects)
            {
                _value = AssetDatabase.GetAssetPath(v);
                if (!string.IsNullOrEmpty(_value))
                {
                    int nameLen = Path.GetFileName(_value).Length;
                    _value = _value.Remove(_value.Length - nameLen - 1, nameLen + 1);
                    _value = _value.Substring(6);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 尝试拾取被选中资源的相对路径，包含文件名，相对路径中不包含"Assets/"
        /// </summary>
        /// <param name="_value"></param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool TryPickupAssetFilePath(out string _value) 
        {
            _value = null;
            foreach (var v in Selection.objects)
            {
                _value = AssetDatabase.GetAssetPath(v);
                if (!string.IsNullOrEmpty(_value))
                {
                    int nameLen = Path.GetFileName(_value).Length;
                    _value = _value.Substring(6);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断某个相对于Assets/的子路径是否存在
        /// </summary>
        /// <param name="_subPath"></param>
        /// <returns></returns>
        public static bool IsValidSubPath(string _subPath)
        {
            string path = Path.Combine(Application.dataPath, _subPath);
            return Directory.Exists(path);
        }

        public static bool IsValidSubAsset(string _subPath) 
        {
            _subPath = _subPath.Trim('\\').Trim('/');
            string path = Application.dataPath + "/" + _subPath;
            return File.Exists(path);
        }
    }
}
