using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    using Internal;
    public class UUpdateProgress
    {
        /// <summary>
        /// 当前正在更新的模块名称
        /// </summary>
        public string ModuleName;
        /// <summary>
        /// 当前正在更新文件名称
        /// </summary>
        public string FileName;
        /// <summary>
        /// 当前下载字节数
        /// </summary>
        public ulong DownloadedBytes;
        /// <summary>
        /// 总字节数
        /// </summary>
        public ulong AllBytes;
        /// <summary>
        /// 下载的文件数量
        /// </summary>
        public int DownloadedCount;
        /// <summary>
        /// 总文件数量
        /// </summary>
        public int AllCount;
        /// <summary>
        /// 每秒的流量
        /// </summary>
        public ulong FluxBytes;
        /// <summary>
        /// 下载总进度[0~1]
        /// </summary>
        public float Progress;
    }

    [Serializable]
    public class UFileVersionRemote
    {
        /// <summary>
        /// 模块的名称
        /// </summary>
        public string ModuleName;
        /// <summary>
        /// 文件名称带后缀名
        /// </summary>
        public string FileName;
        /// <summary>
        /// MD5编码
        /// </summary>
        public string MD5;
        /// <summary>
        /// 文件字节数
        /// </summary>
        public long Size;
        /// <summary>
        /// 文件的相对路径
        /// </summary>
        public string RelativePath;
    }

    public abstract class URemoteVersionRequest : UAsyncOperation 
    {
        public List<UFileVersionRemote> Info { get; protected set; }
        public bool IsSuccess { get; protected set; } 
    }

    class UUpdateSystem
    {
        static Dictionary<string, FileVersionUpdateInfo> VersionFiles = new Dictionary<string, FileVersionUpdateInfo>();

        /// <summary>
        /// 1.从CDN下载版本信息列表
        /// </summary>
        /// <param name="_url">CDN地址</param>
        /// <param name="_completed">可选：完成后回调</param>
        /// <param name="_password">可选：版本文件的密码</param>
        /// <param name="_validSeconds">可选：默认为1天的有效期，超过有效期，会从服务器下载新的版本信息</param>
        /// <returns></returns>
        static public URemoteVersionRequest DownloadVersionFile(string _url, Action<URemoteVersionRequest> _completed = null, string _password = null, float _validSeconds = 7200)
        {
            FileVersionUpdateInfo outInfo;
            if (false == VersionFiles.TryGetValue(_url, out outInfo))
            {
                outInfo = new FileVersionUpdateInfo();
                outInfo.State = EDownloadState.Fail;
                VersionFiles.Add(_url, outInfo);
            }

            if (outInfo.State == EDownloadState.Success)
            {
                if ((DateTime.Now - outInfo.LastUpdate).TotalSeconds >= _validSeconds)
                {
                    outInfo.State = EDownloadState.Fail;
                }
                else
                {
                    _completed?.Invoke(outInfo.Request);
                    return outInfo.Request;
                }
            }

            if (outInfo.State == EDownloadState.Fail)
            {
                outInfo.State = EDownloadState.Downloading;

                outInfo.Request = new URemoteVersionLoader(_url, (src) =>
                {
                    if (string.IsNullOrEmpty(_password) == false)
                    {
                        return UFile.RemKeystore(src, _password);
                    }
                    return src;
                },
                (isOk) =>
                {
                    if (outInfo.Request.IsSuccess)
                    {
                        outInfo.State = EDownloadState.Success;
                        outInfo.LastUpdate = DateTime.Now;
                    }
                    else
                    {
                        outInfo.State = EDownloadState.Fail;
                    }
                    outInfo.Completed?.Invoke(outInfo.Request);
                    outInfo.Completed = null;
                });
                outInfo.Completed = _completed;
            }
            else outInfo.Completed += _completed;

            return outInfo.Request;
        }

        /// <summary>
        /// 2.适用与逐个模块分别更新，筛选出被指定的模块的版本信息
        /// </summary>
        /// <param name="_versionList">从CDN上下载的版本信息列表</param>
        /// <param name="_modules">可选：筛选的模块</param>
        /// <returns></returns>
        static public Dictionary<string, List<UFileVersionRemote>> GetVersionMap(List<UFileVersionRemote> _versionList, List<string> _modules=null)
        {
            Dictionary<string, List<UFileVersionRemote>> result = new Dictionary<string, List<UFileVersionRemote>>();
            List<UFileVersionRemote> outList;

            if (_modules != null && _modules.Count > 0)
            {
                foreach (var v in _versionList)
                {
                    if (_modules.Contains(v.ModuleName) == false) continue;

                    if (false == result.TryGetValue(v.ModuleName, out outList))
                    {
                        outList = new List<UFileVersionRemote>();
                        result.Add(v.ModuleName, outList);
                    }
                    outList.Add(v);
                }
            }
            else
            {
                foreach (var v in _versionList)
                {
                    if (false == result.TryGetValue(v.ModuleName, out outList))
                    {
                        outList = new List<UFileVersionRemote>();
                        result.Add(v.ModuleName, outList);
                    }
                    outList.Add(v);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 2.适用于部分模块更新，筛选出指定的模块的版本信息
        /// </summary>
        /// <param name="_versionList">从CDN上下载的版本信息列表</param>
        /// <param name="_modules">可选：筛选的模块</param>
        /// <returns></returns>
        static public List<UFileVersionRemote> GetVersionList(List<UFileVersionRemote> _versionList, List<string> _modules)
        {
            if (_modules == null || _modules.Count == 0) return _versionList;
            List<UFileVersionRemote> result = new List<UFileVersionRemote>();
            var map = GetVersionMap(_versionList, _modules);
            foreach (var v in map)
            {
                result.AddRange(v.Value);
            }
            return result;
        }

        /// <summary>
        /// 3.获取的更新信息
        /// </summary>
        /// <param name="_assetPath">本地持久化目录</param>
        /// <param name="_versionList">需要参与更新的模块列表</param>
        /// <returns></returns>
        static public UUpdateInformation GetUpdateInformation(string _assetPath, List<UFileVersionRemote> _versionList)
        {
            var result = UFile.CompareFileVersion(_assetPath, _versionList);
            return UUpdateInformation.GetInstance(_assetPath, result);
        }

        /// <summary>
        /// 4.应用更新
        /// </summary>
        /// <param name="_information">更新信息对象</param>
        /// <param name="_url">远程服务器地址</param>
        /// <param name="_tempPath">可选：临时的下载目录，整个模块下载完成后，才会应用到持久化目录</param>
        /// <param name="_progressCallback">可选：进度信息回调函数</param>
        /// <param name="_completed">可选：在完成时回调<bool:是否完成,string:错误信息></param>
        /// <returns></returns>
        static public UAsyncOperation Apply(UUpdateInformation _information, string _url, string _tempPath = null, Action<UUpdateProgress> _progressCallback = null, Action<bool, string> _completed = null)
        {
            if (string.IsNullOrEmpty(_tempPath))
            {
                _tempPath = _information.AssetPath;
            }
            return new DownloadFileList(_information.FinalFiles, _url, _information.AssetPath, _tempPath, _progressCallback, _completed); ;
        }
    } 
}
