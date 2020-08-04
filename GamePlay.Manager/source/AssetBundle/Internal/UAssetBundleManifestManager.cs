using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Internal
{
    class UModuleManifest 
    {
        /// <summary>
        /// string: AssetBundle 的名称
        /// List<string>: 被引用的AssetBundle的相对路径，包含模块路径
        /// 例子：
        ///     string: maps.bytes
        ///     List<string>: commom/texture.bytes,module1/mesh.bytes......
        /// </summary>
        Dictionary<string, List<string>> Dependencies = new Dictionary<string, List<string>>();

        /// <summary>
        /// 根据内容创建一个基于模块的引用关系对象
        /// 注：
        ///     1._content由N行数据组成
        ///     2.每一行数据由引用对象和一组被引用对象组成，他们在行中的书写格式为 引用对象:被引用对象1,被引用对象2,被引用对象n...
        /// </summary>
        /// <param name="_content"></param>
        public UModuleManifest(string _content)
        {
            string[] lines = _content.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            int idxOwner = 0;
            int idxReference = 1;
            foreach (var v in lines)
            {
                string[] parameters = v.Split(':');
                if (parameters.Length != 2) continue;

                string[] dependence = parameters[idxReference].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (dependence.Length < 1) continue;

                Dependencies.Add(parameters[idxOwner], new List<string>(dependence));
            }
        }

        /// <summary>
        /// 获取_packgeName文件的引用文件列表
        /// </summary>
        /// <param name="_packageName"></param>
        /// <returns></returns>
        public List<string> GetDependence(string _packageName)
        {
            List<string> resultl;
            if (!Dependencies.TryGetValue(_packageName, out resultl))
            {
                resultl = new List<string>();
            }
            return resultl;
        }
    }

    /// <summary>
    /// AB资源的依赖关系管理器，每一个游戏实例对应一个单独的管理器
    /// 游戏实例之间不相互关联。
    /// </summary>
    class UAssetBundleManifestManager 
    {
        Dictionary<string, UModuleManifest> Store = new Dictionary<string, UModuleManifest>();

        /// <summary>
        /// 检查是否包含某个模块
        /// </summary>
        /// <param name="_fullModuleName">模块的绝对路径，包含模块名</param>
        /// <returns></returns>
        public bool Contains(string _fullModuleName)
        {
            return Store.ContainsKey(_fullModuleName);
        }

        /// <summary>
        /// 添加一个模块的依赖关系文件
        /// </summary>
        /// <param name="_fullModuleName">模块的绝对路径，包含模块名</param>
        /// <param name="_content">依赖文件的数据</param>
        public void AddManifest(string _fullModuleName, string _content)
        {
            if (!Store.ContainsKey(_fullModuleName))
            {
                Store.Add(_fullModuleName, new UModuleManifest(_content));
            }
        }

        /// <summary>
        /// 获取某个模块下某个文件的依赖文件集合
        /// </summary>
        /// <param name="_fullModuleName">模块的绝对路径</param>
        /// <param name="_packageName">AssetBundle的名字</param>
        /// <returns></returns>
        public List<string> GetDependence(string _fullModuleName, string _packageName)
        {
            UModuleManifest outManifest;
            if (Store.TryGetValue(_fullModuleName, out outManifest))
            {
                return outManifest.GetDependence(_packageName);
            }
            return new List<string>();
        }

        /// <summary>
        /// 移除某个模块的依赖关系数据
        /// </summary>
        /// <param name="_fullModuleName"></param>
        public void RemManifest(string _fullModuleName)
        {
            Store.Remove(_fullModuleName);
        }

        /// <summary>
        /// 清除所有的数据
        /// </summary>
        public void Clear()
        {
            Store.Clear();
        }

        public static string ParseModuleName(string _packageName)
        {
            int idx = _packageName.IndexOf('_');
            if (idx < 0) return _packageName;
            return _packageName.Remove(idx);
        }
    }
}
