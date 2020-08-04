using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    public class UDataManager
    {
        public UDB Data = new Internal.DB();
        public UDBStorage DataStorage = new Internal.DBStorage();
        public UConfig Config = new Internal.Config();
        public UConfigStorage ConfigStorage = new Internal.ConfigStorage(); 
        internal void SetLog(Internal.ILog _output) 
        { 
            Config.Output = _output;
            ConfigStorage.Output = _output;
            Data.Output = _output;
            DataStorage.Output = _output;
        }

        public void Dispose() 
        {
            Data.Clear();
            DataStorage.Clear();
            Config.Clear();
            ConfigStorage.Clear();
        }
    }
}