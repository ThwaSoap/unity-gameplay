using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GamePlay 
{
    public interface ILevelLoader 
    {
        UAsyncOperation LoadLevel(string _levelName, ELoadLevelMode _mode, Action _finish);
        UAsyncOperation LoadLevelAsync(string _levelName, ELoadLevelMode _mode, Action _finish);
        void UnloadAsset(bool _isDispose);
    }
}
