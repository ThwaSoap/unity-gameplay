using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

using System.Security.Cryptography;

namespace GamePlay 
{
    using Internal;
    [Serializable]
    class FileVersionRemoteObject
    {
        public List<UFileVersionRemote> List = null;
    }

    public class AppPublishContentLoader : UAsyncOperation
    {
        protected Func<byte[], byte[]> UnpackKeystore { get; private set; }
        /// <summary>
        /// 文件版本列表
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// 是否下载成功
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// 协同对象
        /// </summary>
        UAsyncOperation Request;

        /// <summary>
        /// 下载进度
        /// </summary>
        public override float Progress
        {
            get
            {
                return !IsSuccess ? 1f : (Request != null ? Request.Progress : 0f);
            }
        }

        public AppPublishContentLoader(string _url, Func<byte[], byte[]> _unpackKeystore)
        {
            UnpackKeystore = _unpackKeystore;
            this.StartCoroutine(LoadAsset(UDownloadRequest.Send(_url)));
        }

        public AppPublishContentLoader(string _url)
        {
            UnpackKeystore = null;
            this.StartCoroutine(LoadAsset(UDownloadRequest.Send(_url)));
        }

        /// <summary>
        /// 获取下载数据的抽象过程
        /// </summary>
        /// <param name="_operation"></param>
        /// <returns></returns>
        protected string GetContent(UAsyncOperation _operation)
        {
            UDownloadRequest request = _operation as UDownloadRequest;
            byte[] binary;
            if (UnpackKeystore != null)
                binary = UnpackKeystore(request.Bytes);
            else
                binary = request.Bytes;

            if (binary != null)
                return Encoding.Default.GetString(binary);
            else
                return "";
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="_operation"></param>
        /// <returns></returns>
        protected IEnumerator LoadAsset(UAsyncOperation _operation)
        {
            IsSuccess = _operation != null;
            Request = _operation;
            Content = "";
            if (Request != null)
            {
                yield return Request;

                Content = GetContent(Request);
                if (string.IsNullOrEmpty(Content))
                {
                    Debug.LogError("The Publish Content is invalid!");
                    IsSuccess = false;
                    yield break;
                }
            }
            yield break;
        }
    }
}

namespace GamePlay.Internal
{
    enum EDownloadState
    {
        Success,
        Fail,
        Downloading
    }

    class FileVersionUpdateInfo
    {
        public URemoteVersionRequest Request;
        public EDownloadState State;
        public DateTime LastUpdate;
        public Action<URemoteVersionRequest> Completed;
    }

    class UFileVersionLocal
    {
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

    class UFile
    {
        /// <summary>
		/// 比较文件的版本信息.
		/// 并返回需要从服务器下载的文件列表，该文件列表是相对路径
		/// </summary>
		/// <returns>如果没有返回空列表.</returns>
		/// <param name="_inputPath">本地需要参与比较在文件根目录.</param>
		/// <param name="_serverVersion">服务器上最新的文件信息.</param>
		public static List<UFileVersionRemote> CompareFileVersion(string _inputPath, List<UFileVersionRemote> _serverVersion, string _searchPattern = "*.bytes")
        {
            List<UFileVersionRemote> result = new List<UFileVersionRemote>();

            //_inputPath = GetSafePath(_inputPath);
            _searchPattern = GetSafeSearchPattern(_searchPattern);

            int headLength = _inputPath.Length;
            var files = Directory.GetFiles(_inputPath, _searchPattern, SearchOption.AllDirectories);
            List<UFileVersionLocal> filesSubLocation = new List<UFileVersionLocal>();
            MD5 md5 = new MD5CryptoServiceProvider();
            foreach (var v in files)
            {
                long outLenth;
                var suburl = v.Substring(headLength).Replace('\\', '/');
                var strMdt = GetMD5(md5, v, out outLenth);
                var fv = new UFileVersionLocal();
                fv.RelativePath = suburl;
                fv.MD5 = strMdt;
                fv.Size = outLenth;
                filesSubLocation.Add(fv);
            }

            md5.Dispose();

            if (filesSubLocation.Count == 0)
            {
                return _serverVersion;
            }
            else
            {
                UFileVersionRemote target;
                UFileVersionLocal compare;
                for (int i = 0; i < _serverVersion.Count; i++)
                {
                    target = _serverVersion[i];
                    compare = filesSubLocation.Find((v) => { return v.RelativePath.Equals(target.RelativePath); });

                    if (null == compare || compare.MD5 != target.MD5)
                    {
                        result.Add(target);
                    }
                }
            }
            return result;
        }

        static string GetSafeSearchPattern(string _value)
        {
            return string.IsNullOrEmpty(_value) ? "*.*" : _value;
        }

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

        public static string GetSafePath(string _path)
        {
            return _path.Replace('\\', '/').TrimEnd('/') + "/";
        }

        public static string Combine(string _root, string _sub)
        {
            _root = _root.TrimEnd('\\', '/');
            _sub = _sub.TrimStart('\\', '/');
            return _root + "/" + _sub;
        }

        internal static byte[] AddKeystore(byte[] _bytes, string _keystore)
        {
            if (_keystore.Length != 0)
            {
                int a;
                for (int i = 0; i < _bytes.Length; i++)
                {
                    a = _bytes[i];
                    a = ~a;
                    _bytes[i] = (byte)(a - _keystore[i % _keystore.Length]);
                }
            }
            return _bytes;
        }


        internal static byte[] RemKeystore(byte[] _bytes, string _keystore)
        {
            if (_keystore.Length != 0)
            {
                int a;
                for (int i = 0; i < _bytes.Length; i++)
                {
                    a = _bytes[i] + _keystore[i % _keystore.Length];
                    a = ~a;
                    _bytes[i] = (byte)a;

                }
            }
            return _bytes;
        }
    } 
    /*
     * 跟远程的文件列表进行对比，筛选出需要下载的文件信息
     */
    class VersionCheckerWithRemote
    {
        Dictionary<string, List<UFileVersionRemote>> DownloadInfomation = new Dictionary<string, List<UFileVersionRemote>>();
        string CheckWorkPath;
        public VersionCheckerWithRemote(string _checkWorkPath)
        {
            this.CheckWorkPath = _checkWorkPath;
        }

        public void Checker(string _moduleName, List<UFileVersionRemote> _remoteList)
        {
            List<UFileVersionRemote> result = UFile.CompareFileVersion(CheckWorkPath, _remoteList);
            Remove(_moduleName);
            if (result.Count > 0)
            {
                DownloadInfomation.Add(_moduleName,result);
            }
        }

        public List<UFileVersionRemote> GetFileInfo(List<string> _modules)
        {
            List<UFileVersionRemote> result = new List<UFileVersionRemote>();
            foreach (var v in _modules)
            {
                if (DownloadInfomation.ContainsKey(v))
                {
                    result.AddRange(DownloadInfomation[v].ToArray());
                }
            }
            return result;
        }

        public List<UFileVersionRemote> GetFileInfo(string _modules)
        {
            List<UFileVersionRemote> result = new List<UFileVersionRemote>();
            if (DownloadInfomation.ContainsKey(_modules))
            {
                result.AddRange(DownloadInfomation[_modules].ToArray());
            }
            return result;
        }

        public List<UFileVersionRemote> GetFileInfo()
        {
            List<UFileVersionRemote> result = new List<UFileVersionRemote>();
            foreach (var v in DownloadInfomation)
            {
                result.AddRange(v.Value.ToArray());
            }
            return result;
        }

        public int GetFileCounter(List<string> _modules)
        {
            int counter = 0;
            foreach (var v in _modules)
            {
                if (DownloadInfomation.ContainsKey(v))
                {
                    counter += DownloadInfomation[v].Count;
                }
            }
            return counter;
        }

        public List<string> GetKeys()
        {
            List<string> keys = new List<string>();
            foreach (var v in DownloadInfomation)
            {
                if (v.Value.Count > 0) keys.Add(v.Key);
            }
            return keys;
        }

        public int GetFileCounter()
        {
            int counter=0;
            foreach (var v in DownloadInfomation)
            {
                counter += v.Value.Count;
            }
            return counter;
        }

        public void Remove(string _moduleName)
        {
            if(DownloadInfomation.ContainsKey(_moduleName))
                DownloadInfomation.Remove(_moduleName);
        }

        public void Remove(List<string> _moduleName)
        {
            foreach (var v in _moduleName)
            {
                DownloadInfomation.Remove(v);
            }
        }

        public void Remove()
        {
            DownloadInfomation.Clear();
        }

        void SmartAdd(UFileVersionRemote _info)
        {  
            List<UFileVersionRemote> outList;
            if (false == DownloadInfomation.TryGetValue(_info.ModuleName, out outList))
            {
                outList = new List<UFileVersionRemote>();
                DownloadInfomation.Add(_info.ModuleName, outList);
            }
            outList.Add(_info);
        }
    }

    class URemoteVersionLoader : URemoteVersionRequest
    {
        /// <summary>
        /// 协同对象
        /// </summary>
        UAsyncOperation Request;

        /// <summary>
        /// 下载进度
        /// </summary>
        public override float Progress
        {
            get
            {
                return !IsSuccess ? 1f : (Request != null ? Request.Progress : 0f);
            }
        }

        public URemoteVersionLoader(string _url, Func<byte[], byte[]> _unpackKeystore,Action<bool> _finished)
        { 
            this.StartCoroutine(Start(UDownloadRequest.Send(_url), _unpackKeystore, _finished));
        }

        /// <summary>
        /// 获取下载数据的抽象过程
        /// </summary>
        /// <param name="_operation"></param>
        /// <returns></returns>
        string GetContent(UAsyncOperation _operation, Func<byte[], byte[]> _unpackKeystore) 
        {
            UDownloadRequest request = _operation as UDownloadRequest;
            byte[] binary;
            if (_unpackKeystore != null)
                binary = _unpackKeystore(request.Bytes);
            else
                binary = request.Bytes;

            if (binary != null)
                return Encoding.Default.GetString(binary);
            else
                return "";
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="_operation"></param>
        /// <returns></returns>
        IEnumerator Start(UAsyncOperation _operation, Func<byte[], byte[]> _unpackKeystore, Action<bool> _finished)
        {
            IsSuccess = _operation != null;
            Request = _operation;
            Info = new List<UFileVersionRemote>();
            if (Request != null)
            {
                yield return Request;

                string content = GetContent(Request, _unpackKeystore); 
                if (string.IsNullOrEmpty(content))
                {
                    Debug.LogError("The Version Content is invalid!");
                    IsSuccess = false;
                    yield break;
                }

                var list = JsonUtility.FromJson<FileVersionRemoteObject>(content);

                if (list != null && list.List != null)
                {
                    Info.AddRange(list.List);
                }
                else
                {
                    Debug.LogError("The Version Content dose not came from json!");
                    IsSuccess = false;
                }

                _finished?.Invoke(IsSuccess);
            }
            yield break;
        }
    } 

    #region 文件下载处理函数
    /// <summary>
    /// 下载文件列表
    /// </summary>
    class DownloadFileList : UAsyncOperation
    {
        public bool IsSuccess { get; private set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        /// <value>The error.</value>
        public string Error { get; private set; }

        Action<UUpdateProgress> OnProgressChanged;
        UUpdateProgress ProgressInformation;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_fileInfos">需要被下载的文件列表</param>
        /// <param name="_downloadAddress">下载的地址,不包含子目录路径</param>
        /// <param name="_persistentPathRoot">持久化目录的路径</param>
        /// <param name="_tempPathRoot">临时文件目录的路径</param>
        /// <param name="_breakOnDownloadFailing">是否在下载失败时中断，默认为true</param>
        /// <param name="_onDebug">调试接口</param>
        public DownloadFileList(List<UFileVersionRemote> _fileInfos, string _downloadAddress, string _persistentPathRoot, string _tempPathRoot,
            Action<UUpdateProgress> _progressCallback = null, Action<bool, string> _completed = null, bool _breakOnDownloadFailing = true, Action<string> _onDebug = null)
        {
            OnProgressChanged = _progressCallback;
            ProgressInformation = new UUpdateProgress();
            if (_fileInfos == null || _fileInfos.Count == 0)
            {
                IsSuccess = true;
                Progress = 1f;
            }
            else
            {
                this.StartCoroutine(DoStart(_fileInfos, _downloadAddress, _persistentPathRoot, _tempPathRoot, _completed, _breakOnDownloadFailing, _onDebug));
            }
        }

        ulong Sum(List<UFileVersionRemote> _infos)
        {
            ulong value = 0;
            foreach (var v in _infos)
            {
                value += (ulong)v.Size;
            }
            return value;
        }

        /// <summary>
        /// 刷新进度
        /// </summary>
        /// <param name="downloadingBytes">已经下载的字节数</param>
        /// <param name="_allBytes">需要下载的字节数</param>
        /// <param name="_downloadingFileCount">已经下载的文件数</param>
        /// <param name="_allFileCount">需要下载的文件数</param>
        /// <param name="_fluxOnSeconds">每秒的流量</param>
        void RefreshProgress(ulong downloadingBytes, ulong _allBytes, int _downloadingFileCount, int _allFileCount, ulong _fluxOnSeconds, float _progress,string _moduleName,string _fileName)
        {
            ProgressInformation.AllBytes = _allBytes;
            ProgressInformation.DownloadedBytes = downloadingBytes;
            ProgressInformation.DownloadedCount = _downloadingFileCount;
            ProgressInformation.AllCount = _allFileCount;
            ProgressInformation.FluxBytes = _fluxOnSeconds;
            ProgressInformation.Progress = _progress;
            if (OnProgressChanged != null) OnProgressChanged(ProgressInformation); 
        }
        

        IEnumerator DoStart(List<UFileVersionRemote> _fileList, string _downloadAddress, string _persistentPathRoot, string _tempPathRoot, Action<bool, string> _completed, bool _breakOnDownloadFailing, Action<string> _onDebug)
        {

            List<UFileVersionRemote> downloadList;
            List<UFileVersionRemote> copyList = null;

            //> 排除缓存目录已经存在的文件 -- 待考虑功能
            //> 过滤掉缓存目录已经下载完毕的文件，根据MD5进行筛选 

            if (_persistentPathRoot != _tempPathRoot)
            {
                downloadList = UFile.CompareFileVersion(_tempPathRoot, _fileList);
                copyList = _fileList;
            }
            else 
            {
                downloadList = _fileList;
            }


            ulong allBytes = Sum(downloadList);
            ulong fluxOnSeconds = 0;
            ulong downloadingByte = 0;
            ulong downloadAllByte = 0;
            int fileCount = downloadList.Count;

            if (downloadList.Count > 0) 
            {
                //---------------------------------------------------
                float progressRatio = _tempPathRoot != _persistentPathRoot ? 0.9f / downloadList.Count : 1f / downloadList.Count;
                //> 这个列表中存储这下载到缓存目录的所有文件，在结尾，我们需要把这些文件拷贝到持久化目录
                List<string> tempFiles = new List<string>();
                //> 如果中途有文件没有成功下载到缓存目录则视为更新失败
                string module = "", file = "";
                for (var i = 0; i < _fileList.Count; i++)
                {
                    UFileVersionRemote info = _fileList[i];
                    string downloadAddress = UFile.Combine(_downloadAddress, info.RelativePath);
                    var request = UnityWebRequest.Get(downloadAddress);
                    var asyncSend = request.SendWebRequest();
                    module = info.ModuleName;
                    file = info.FileName;
                    while (asyncSend.isDone == false)
                    {
                        yield return new WaitForSeconds(1);
                        fluxOnSeconds = request.downloadedBytes - downloadingByte;
                        downloadingByte = request.downloadedBytes;
                        //> 在这里更新字节数
                        RefreshProgress(downloadAllByte + downloadingByte, allBytes, i + 1, fileCount, fluxOnSeconds, Progress, module, file);
                    }

                    if (string.IsNullOrEmpty(request.error))
                    {
                        string tempFilePath = UFile.Combine(_tempPathRoot, info.RelativePath).Replace('\\', '/');

                        int index = tempFilePath.LastIndexOf('/');
                        string url = tempFilePath.Remove(index);
                        if (!Directory.Exists(url))
                            Directory.CreateDirectory(url);

                        File.WriteAllBytes(tempFilePath, request.downloadHandler.data);
                        tempFiles.Add(tempFilePath);
                    }
                    else if (_breakOnDownloadFailing)
                    {
                        Error = string.Format("Can't download the file [{0}] from address [{1}]", info.RelativePath, _downloadAddress);
                        Progress = 1f;
                        _completed?.Invoke(false, Error);
                        yield break;
                    }

                    Progress += progressRatio;
                    downloadAllByte += (ulong)info.Size;
                    //> 在这里更新字节数
                    //RefreshProgress(downloadAllByte, allBytes, i + 2 > fileCount ? fileCount : i + 2, fileCount, fluxOnSeconds);
                    fluxOnSeconds = 0;
                    downloadingByte = 0;
                }

                RefreshProgress(downloadAllByte, allBytes, fileCount, fileCount, 0, Progress, module, file);
            }
          
            if (copyList != null && copyList.Count > 0)
            {
                float progressRatio = 0.1f / _fileList.Count;
                string module, file;
                //> 文件拷贝不计算在进度内
                foreach (var v in _fileList)
                {
                    module = v.ModuleName;
                    file = v.FileName;
                    string tempFile = UFile.Combine(_tempPathRoot, v.RelativePath);
                    string persistentFile = UFile.Combine(_persistentPathRoot, v.RelativePath).Replace('\\', '/');

                    int index = persistentFile.LastIndexOf('/');
                    string url = persistentFile.Remove(index);
                    if (!Directory.Exists(url))
                        Directory.CreateDirectory(url);

                    if (File.Exists(tempFile))
                    {
                        File.Copy(tempFile, persistentFile, true);
                        File.Delete(tempFile);
                        if (_onDebug != null)
                            _onDebug(persistentFile);
                        yield return null;
                    }
                    else if (_breakOnDownloadFailing)
                    {
                        Debug.LogErrorFormat("更新失败:没有找到->{0}", tempFile);
                        Error = string.Format("The file is mission->{0}", tempFile);
                        Progress = 1f;
                        _completed(false, Error);
                        yield break;
                    }
                    Progress += progressRatio;
                    //RefreshProgress(allBytes, allBytes, fileCount, fileCount, 0, Progress, v.ModuleName, v.FileName);
                }
            }


            
            IsSuccess = true;
            Progress  = 1f;
            //RefreshProgress(allBytes, allBytes, fileCount, fileCount, 0, 1f, "", "");
            if (_completed != null)
                _completed(true, "");
            yield break;
        }
    }
    #endregion
}