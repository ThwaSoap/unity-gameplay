using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlay
{
    public struct UUpdateInformation
    {
        public bool IsUpdate { get; private set; }
        public int Count { get; private set; }
        public ulong Bytes { get; private set; }

        internal List<UFileVersionRemote> FinalFiles;
        internal string AssetPath;

        internal static UUpdateInformation GetInstance(string _assetPath,List<UFileVersionRemote> _versionList) 
        {
            UUpdateInformation instance = new UUpdateInformation();
            instance.AssetPath = _assetPath;
            instance.FinalFiles = _versionList;
            instance.Count = _versionList.Count;
            instance.IsUpdate = _versionList.Count > 0;
            ulong allBytes = 0;
            foreach (var v in _versionList) 
            {
                allBytes += (ulong)v.Size;
            }
            instance.Bytes = allBytes;
            return instance;
        }
    }
}