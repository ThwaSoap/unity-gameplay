

using System;
using System.Collections.Generic;

namespace GamePlay 
{
    public abstract class UDBStorage
    {
        internal Internal.ILog Output { get; set; }
        #region 框架接口  
        public abstract void Clear(); 
        public abstract void InitializedCustomEnum(Type[] _types);
        public abstract bool LoadData(string _tableName, string _tableContent); 
        public abstract bool TrySelect(string _tableName, string _command, out DBContent[] _outDatas, bool _cacheData = false);
        public abstract DBContent[] Select(string _tableName, string _command, bool _cacheData = false);
        public abstract List<UDBPropertyTitle> GetTitle(string _tableName);
        public abstract void ClearCacheData(string _tableName);
        public abstract bool CreateContent(string _tableName, out string _outResult); 
        public abstract void UnloadData(string _tableName);
        public static UDBStorage CreateInstance() 
        {
            return new Internal.DBStorage();
        }
        #endregion
    }
}

namespace GamePlay.Internal
{

    class DBStorage : UDBStorage
    {
        #region 数据类
        class Data
        {
            public List<DBPropertyStorage> Storage { get; private set; }
            public List<IProperty> Propertys { get; private set; }
            //public List<string> PropertyNames { get; private set; }
            public DBContent[] DataArray { get; private set; }

            /// <summary>
            /// 快速缓冲区
            /// </summary>
            DataCache Cache;

            public Data(List<DBPropertyStorage> _propertyStorage, DBContent[] _dataArray)
            {
                Storage = _propertyStorage;
                Propertys = new List<IProperty>((IProperty[])(object)Storage.ToArray());
                DataArray = _dataArray;
                Cache = new DataCache ();
            }

            public bool CheckoutCache ( string _condition, out object _outValue, bool _isCache )
            {
                _outValue = false;
                if ( !_isCache ) return false;
                return Cache.TryGetout ( _condition, out _outValue );
            }

            public void SaveToCache ( string _condition, object _value, bool _isSave )
            {
                if ( _isSave )
                {
                    Cache.SaveToCache ( _condition, _value );
                }
            }

            public void ClearCecheData ()
            {
                Cache.ClearCache ();
            }

            public void Clear()
            {
                Propertys.Clear();
                Cache.ClearCache ();
                //PropertyNames.Clear();
                DataArray = null;
                //PropertyNames = null;
                Propertys = null;
            }
        }
        #endregion

        Dictionary<string, Data> Datas = new Dictionary<string, Data>();
        IDBPropertyConvertForStorage ConvertBase = new DBPropertyConvertForStorage();

        #region 框架接口  
        public override void Clear()
        {
            foreach (var v in Datas)
            {
                v.Value.Clear();
            }
            Datas.Clear();
        }

        public override void  InitializedCustomEnum (Type[] _types)
        {
            ConvertBase.InitializedCustomEnum (_types);
        }
        public override bool LoadData(string _tableName, string _tableContent)
        {
            //> 检查数据类型的有效性
            if (null == _tableName || _tableName.Length == 0)
            {
                Output?.Error("IDBStorage：无效的表名！");
                return false;
            }

            //> 检查填充内容的有效性
            if (null == _tableContent || _tableContent.Length == 0)
            {
                Output?.Error("IDBStorage：数据载入{0}失败!请确保载入数据有效！", _tableName);
                return false;
            }

            char[] splits = new char[] { '\t' };
            string[] lines = _tableContent.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            if (lines.Length < 2)
            {
                Output?.Error("IDBStorage：数据载入{0}失败!请确保表头有效！", _tableName);
                return false;
            }

            //> 检查数据内容的表头格式
            string[] tabPropertysType = lines[0].Split(splits);
            string[] tabPropertysName = lines[1].Split(splits);

            if (tabPropertysName.Length == 0 ||
                tabPropertysType.Length == 0 ||
                tabPropertysType.Length != tabPropertysName.Length)
            {
                Output?.Error("IDBStorage：数据载入{0}失败!请确保表头数据格式正确！", _tableName);
                return false;
            }

            //> 创建属性集合 UTDBPropertyStorage 对象
            List<DBPropertyStorage> validPropertys = new List<DBPropertyStorage>();
            if (MakeValidPropertys(ref validPropertys, tabPropertysType, tabPropertysName))
            {
                Output?.Error("IDBStorage：载入失败！请确保{0}类中存在有效的存储字段！", _tableName);
                return false;
            }

            //> 使属性和解析数据列对齐
            DBPropertyStorage[] linePropertys = new DBPropertyStorage[tabPropertysName.Length];

            var valid = false;
            for (int i = 0, max = tabPropertysName.Length; i < max; i++)
            {
                foreach (var v in validPropertys)
                {
                    if (v.GetName() == tabPropertysName[i])
                    {
                        valid = true;
                        linePropertys[i] = v;
                        break;
                    }
                }
            }

            if (false == valid)
            {
                Output?.Error("IDBStorage：载入失败！{0}表中没有发现与数据匹配的数据类型！", _tableName);
                return false;
            }

            List<string> validPropertyNames = new List<string>();
            for (var i = 0; i < validPropertys.Count; i++)
            {
                validPropertyNames.Add(validPropertys[i].GetName());
            }

            //> 最后一行数据的索引值
            int endLength = lines.Length - 1;

            //> 写入行数去掉末尾空行(wps导出的转义符文件总是多出一个空行)
            int maxCount = string.IsNullOrEmpty(lines[endLength]) ? endLength - 1 : endLength;

            //> 存储数据的数组
            DBContent[] datas = new DBContent[maxCount];

            //> 实际填充的个数
            int itemCount = 0;

            //> 当前操作的数据实例
            DBContent itemCur = null;

            //> 遍历写入所有内容
            for (int i = 2, max = lines.Length; i < max; i++)
            {
                string[] column = lines[i].Split(splits);

                //> 跳过不完整的数据行
                if (column.Length != tabPropertysName.Length) continue;

                //> 创建并向目标身上写入数据
                itemCur = new DBContent(validPropertyNames);

                for (int n = 0, n_max = column.Length; n < n_max; n++)
                {
                    //> 跳过无效的属性字段
                    if (linePropertys[n] != null)
                    {
                        linePropertys[n].SetValue(itemCur, column[n]);
                    }
                }

                datas[itemCount++] = itemCur;
            }

            //> 矫正数组个数
            DBContent[] final = datas;
            if (itemCount > 0)
            {
                if (itemCount != maxCount)
                {
                    final = new DBContent[itemCount];
                    Array.Copy(datas, final, final.Length);
                }
            }
            else
            {
                final = new DBContent[0];
            }

            //> 将列表对象填充到对应实例中
            //IProperty[] iPropertys = (IProperty[])(object)validPropertys.ToArray();

            //> 将数据存储图表
            Datas[_tableName] = new Data(validPropertys, final);

            Output?.Log("IDBStorage：[{0}] 解析完成！ 共计[{1}]行数据！", _tableName,final.Length); 
            return true;
        }

        public override bool TrySelect(string _tableName, string _command, out DBContent[] _outDatas, bool _cacheData=false)
        {
            Data outDataValue;
            if (Datas.TryGetValue(_tableName, out outDataValue))
            {
				if (string.IsNullOrEmpty (_command)) {
					_outDatas = outDataValue.DataArray;
					return true;
				}

                object outCacheData;
                if ( outDataValue.CheckoutCache ( _command, out outCacheData, _cacheData ) )
                {
                    _outDatas = ( (List<DBContent>) outCacheData ).ToArray ();
                    return true;
                }

                DBCommand cmd = new DBCommand(outDataValue.Propertys);

                if (cmd.SetCommand(_command))
                {
                    List<DBContent> list = cmd.Search<DBContent>(outDataValue.DataArray);
                    outDataValue.SaveToCache ( _command, list, _cacheData );
                    _outDatas = (DBContent[])(object)list.ToArray();
                    return _outDatas.Length > 0;
                }
            }
            _outDatas = new DBContent[0];
            return false;
        }

        public override DBContent[] Select(string _tableName, string _command, bool _cacheData = false)
        {
            Data outDataValue;
            if (Datas.TryGetValue(_tableName, out outDataValue))
            {
				if (string.IsNullOrEmpty (_command)) {
					return outDataValue.DataArray;
				}
				
                object outCacheData;
                if ( outDataValue.CheckoutCache ( _command, out outCacheData, _cacheData ) )
                {
                    return( (List<DBContent>) outCacheData ).ToArray ();
                }

                DBCommand cmd = new DBCommand(outDataValue.Propertys);

                if (cmd.SetCommand(_command))
                {
                    List<DBContent> list = cmd.Search<DBContent> ( outDataValue.DataArray );
                    outDataValue.SaveToCache ( _command, list, _cacheData );
                    return list.ToArray();
                }
            }
            return new DBContent[0];
        }

        public override List<UDBPropertyTitle> GetTitle(string _tableName)
        {
            List<UDBPropertyTitle> result = new List<UDBPropertyTitle>();
            Data outDataValue;
            if (Datas.TryGetValue(_tableName, out outDataValue))
            {
                for (int i = 0; i < outDataValue.Propertys.Count; i++) 
                {
                    IProperty p = outDataValue.Propertys[i];
                    result.Add(new UDBPropertyTitle(p.PropertyName, p.PropertyTypeName));
                }
            }
            return result;
        }

        public override void ClearCacheData ( string _tableName )
        {
            Data outDataValue;
            if ( Datas.TryGetValue ( _tableName, out outDataValue ) )
            {
                outDataValue.ClearCecheData();
            }
        }

        public override bool CreateContent(string _tableName, out string _outResult)
        {
            //> 这里我们只打包已经载入过的数据集
            Data outDataValue;
            if (Datas.TryGetValue(_tableName, out outDataValue))
            {
                return MakeTableContent(out _outResult, outDataValue.Propertys, outDataValue.Storage, outDataValue.DataArray);
            }
            else
            {
                _outResult = "";
                return false;
            }
        }

        public override void UnloadData(string _tableName)
        {
            //> 卸载数据时，将数据集从列表中移除，不建议再这里调用GC。
            Data outData;
            if (Datas.TryGetValue(_tableName, out outData))
            {
                outData.Clear();
                Datas.Remove(_tableName);
            }
        }
        #endregion

        #region 内部维护函数
        bool MakeValidPropertys(ref List<DBPropertyStorage> _propertys, string[] _types, string[] _propertysName)
        {
            for (int i = 0, index = 0; i < _types.Length; i++)
            {
                var convertInfo = ConvertBase.FindBaseInfo(_types[i]);

                if (convertInfo != null)
                {
                    _propertys.Add(new DBPropertyStorage(_propertysName[i], index++, convertInfo));
                }
            }

            return 0 == _propertys.Count;
        }

        bool MakeTableContent(out string _outResult, List<IProperty> _propertys, List<DBPropertyStorage> _propertysStorage, DBContent[] _datas)
        {
            _outResult = "";

            #region 写入表头
            //> 写入字段类型
            _outResult = _propertys[0].PropertyTypeName;
            for (int i = 1, max = _propertys.Count; i < max; ++i)
            {
                _outResult += '\t' + _propertys[i].PropertyTypeName;
            }
            _outResult += "\r\n";

            //> 写入字段名称
            _outResult += _propertys[0].PropertyName;
            for (int i = 1, max = _propertys.Count; i < max; ++i)
            {
                _outResult += '\t' + _propertys[i].PropertyName;
            }
            _outResult += "\r\n";
            #endregion

            //> 写入数据流
            foreach (var v in _datas)
            {
                if (null == v) continue;

                _outResult += _propertysStorage[0].GetValue(v);
                for (int i = 1, max = _propertysStorage.Count; i < max; ++i)
                {
                    _outResult += '\t' + _propertysStorage[i].GetValue(v);
                }
                _outResult += "\r\n";
            }
            return true;
        }
        #endregion
    }
}
