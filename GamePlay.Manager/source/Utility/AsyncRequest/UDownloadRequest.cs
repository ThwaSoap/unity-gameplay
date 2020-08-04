using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GamePlay
{
    using Internal;
    public sealed class UDownloadRequest : UAsyncOperation
    {
        #region body
        public string Url { get; private set; }
        public bool IsSuccess { get { return !Failed; } }
        public string Error { get; private set; } 
        public ulong DownloadBytes { get; private set; } 
        public byte[] Bytes { get; private set; }
        public string Text { get; private set; }

        public override float Progress
        {
            get
            {
                return Failed ? 1f : (FirstProgress + LastProgress) / 1.1f;
            }
        }

        public event Action<UDownloadRequest> Completed;

        bool Failed = false;
        float LastProgress = 0;
        float FirstProgress = 0; 

        private UDownloadRequest(UnityWebRequest _request) 
        { 
            this.StartCoroutine(Start(_request));
        }

        internal IEnumerator Start(UnityWebRequest _request)
        {
            Url = _request.url;
            _request.SendWebRequest();

            while (!_request.isDone) 
            {
                yield return new WaitForFixedUpdate();
                FirstProgress = _request.downloadProgress;
                DownloadBytes = _request.downloadedBytes;
            }

            Bytes = _request.downloadHandler.data;
            Text = _request.downloadHandler.text;

            if (_request.isHttpError || _request.isNetworkError)
            {
                Failed = true;
            } 
            FirstProgress = 1f;
            LastProgress = 0.1f;
            Completed?.Invoke(this);
            _request.Dispose();
            yield break;
        }
        #endregion

        public static UDownloadRequest Send(string _url,string _postData=null) 
        {
            if(string.IsNullOrEmpty(_postData))
                return new UDownloadRequest(UnityWebRequest.Get(_url));
            else       
                return new UDownloadRequest(UnityWebRequest.Post(_url, _postData));
        }

        public static UDownloadRequest Send(string _url, Dictionary<string, string> _postData) 
        {
            if (null == _postData)
                return new UDownloadRequest(UnityWebRequest.Get(_url));
            else
                return new UDownloadRequest(UnityWebRequest.Post(_url, _postData));
        }
    }

    public class UUploadFile 
    {
        public string FieldName;
        public byte[] Bytes;
        public string FileName;
        public string MiniType;
    }

    public sealed class UUploadRequest : UAsyncOperation 
    {
        #region body
        public string Url { get; private set; }
        public bool IsSuccess { get { return !Failed; } }
        public string Error { get; private set; }
        public ulong UploadBytes { get; private set; } 
        public ulong AllBytes { get; private set; }

        public override float Progress
        {
            get
            {
                return Failed ? 1f : (FirstProgress + LastProgress) / 1.1f;
            }
        }

        public event Action<UUploadRequest> Completed;

        bool Failed = false;
        float LastProgress = 0;
        float FirstProgress = 0;

        private UUploadRequest(UnityWebRequest _request,ulong _allBytes) 
        {
            this.AllBytes = _allBytes;
            this.StartCoroutine(Start(_request));
        }

        IEnumerator Start(UnityWebRequest _request) 
        {
            Url = _request.url;
            _request.SendWebRequest();
            while (!_request.isDone)
            {
                yield return new WaitForFixedUpdate();
                FirstProgress = _request.uploadProgress;
                UploadBytes   = _request.uploadedBytes;
            }

            if (_request.isHttpError || _request.isNetworkError)
            {
                Failed = true;
            }

            FirstProgress = 1f;
            LastProgress = 0.1f;
            Completed?.Invoke(this);
            _request.Dispose();
            yield break; 
        }
        #endregion

        public static UUploadRequest Send(string _url,Dictionary<string,string> _fields,List<UUploadFile> _files=null) 
        {
            ulong allBytes = 0;
            WWWForm form = new WWWForm();
            if (_fields != null && _fields.Count > 0) 
            {
                foreach (var v in _fields)
                {
                    form.AddField(v.Key, v.Value);
                }
            }

            if (_files != null && _files.Count > 0)
            {
                foreach (var v in _files)
                {
                    form.AddBinaryData(v.FieldName,v.Bytes,v.FileName,v.MiniType);
                    allBytes += (ulong)v.Bytes.Length;
                }
            }

            return new UUploadRequest(UnityWebRequest.Post(_url, form), allBytes);
        }
    }
}