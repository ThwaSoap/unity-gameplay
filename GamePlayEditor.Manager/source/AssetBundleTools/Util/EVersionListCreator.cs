using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace GamePlayEditor.AssetBundleTools
{
    //RelativePath(相对于Version.List的路径),文件名称(文件名称),MD5(唯一标识),Size(文件大小),Module(模块名称)
    class FileVersionInfo
    {
        public string RelativePath;
        public string FileName;
        public string MD5;
        public long Size;
        public string ModuleName;
    }

    static class TiyVersionListCreator
    {
        public static List<FileVersionInfo> GetVersionListOfModule(string _parentPath,string _inputPath,string _moduleName, string _searchPattern)
        {
            List<FileVersionInfo> result = new List<FileVersionInfo>();
            var files = Directory.GetFiles(_inputPath, _searchPattern, SearchOption.AllDirectories);
            int parentDirLen = _parentPath.Length;
            MD5 md5 = new MD5CryptoServiceProvider();
            foreach (var v in  files)
            {
                FileVersionInfo info = new FileVersionInfo();
                FileInfo finfo = new FileInfo(v);
                long outLength;
                var suburl = v.Substring(parentDirLen); 
                var strMd5 = EAssetBundleToolUtil.GetMD5(md5, v, out outLength);
                info.Size = outLength;
                info.MD5 = strMd5;
                info.FileName = finfo.Name;
                info.RelativePath = suburl;
                info.ModuleName = _moduleName;
                result.Add(info);
            }
            return result;
        }

        public static FileVersionInfo GetVersionInfoByFile(string _parentPath, string _filePath, string _moduleName)
        {
            FileVersionInfo info = new FileVersionInfo();
            int parentDirLen = _parentPath.Length;
            FileInfo finfo = new FileInfo(_filePath);
            long outLength;
            var suburl = _filePath.Substring(parentDirLen);
            MD5 md5 = new MD5CryptoServiceProvider();
            var strMd5 = EAssetBundleToolUtil.GetMD5(md5, _filePath, out outLength);
            info.Size = outLength;
            info.MD5 = strMd5;
            info.FileName = finfo.Name;
            info.RelativePath = suburl;
            info.ModuleName = _moduleName;
            //md5.Dispose();
            return info;
        }

        public static void CreateVersionListByTemplate(List<FileVersionInfo> _allInfo,string _templateFilePath,string _outputPath)
        {
            if (File.Exists(_templateFilePath)==false) return;

            string templateContent = File.ReadAllText(_templateFilePath);

            string[] contents = templateContent.Split(new string[] {":::" },2,System.StringSplitOptions.None);
             
            if (contents.Length != 2)
            {
                Debug.Log("Invalid Template Becuse It need a value define.");
                return;
            }

            string[] rows = contents[0].Split(new string[] { "\r\n" },2,System.StringSplitOptions.None);

            if (rows.Length != 2)
            {
                Debug.Log("Invalid Template about the row of define.");
                return;
            }

            string row1 = rows[0];
            string row2 = rows[1];
            string body = contents[1].TrimStart('\r').TrimStart('\n'); 

            string content = "";

            for ( int i = 0; i < _allInfo.Count; i++ )
            {
                var v = _allInfo[i];
                string row = i < (_allInfo.Count - 1) ? row1 : row2; 

                row = row.Replace("#RelativePath", v.RelativePath);
                row = row.Replace("#FileName", v.FileName);
                row = row.Replace("#ModuleName", v.ModuleName);
                row = row.Replace("#MD5", v.MD5);
                row = row.Replace("#Size", v.Size.ToString()); 

                if (i > 0)
                    content += "\r\n" + row;
                else
                    content += row;
            }
            
            File.WriteAllText(_outputPath, body.Replace("$Dataset", content));
        }
    }
}
