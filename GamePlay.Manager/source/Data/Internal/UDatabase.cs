using System;
using System.Collections;
using System.Collections.Generic;

namespace GamePlay.Internal
{
    class DataCache
    {
        /// <summary>
        /// 一批数据
        /// </summary>
        class DataBatch
        {
            public object DataSet;
        }

        /// <summary>
        ///  查询历史
        /// </summary>
        Dictionary<string,DataBatch> Cache = new Dictionary<string, DataBatch>();

        public bool TryGetout (string _condition,out object _outObject)
        {
            _outObject = null;
            DataBatch outValue;
            if ( Cache.TryGetValue ( _condition, out outValue ) )
            {
                _outObject = outValue.DataSet;
                return true;
            }
            return false;
        }

        public void SaveToCache ( string _command,object _dataList )
        {
            DataBatch outValue;
            if ( !Cache.TryGetValue ( _command, out outValue ) )
            {
                outValue = new DataBatch ();
                Cache.Add ( _command, outValue );
            }
            outValue.DataSet = _dataList;
        }

        public void ClearCache()
        {
            Cache.Clear ();
        }
    }
}


