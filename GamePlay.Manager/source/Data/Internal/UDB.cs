using System;
using System.Collections.Generic;
using System.Reflection;

namespace GamePlay 
{
    public abstract class UDB
    {  
        internal Internal.ILog Output { get; set; }
        public abstract void Clear();

        #region IDB接口实现
        public abstract bool LoadData(Type _mainClass, string _tableContent);
        public abstract bool TrySelect(string _className, string _condition, out object[] _outDataSet, bool _tryCache = false);
        public abstract object[] Select(string _className, string _condition, bool _tryCache = false);
        public abstract bool TrySelect<T>(string _condition, out T[] _outDataSet, bool _tryCache = false) where T : class;
        public abstract T[] Select<T>(string _condition, bool _tryCache = false) where T : class; 
        public abstract List<UDBPropertyTitleDB> GetTitle(string _className); 
        public abstract void ClearCacheData(string _calssName); 
        public abstract bool CreateContent(Type _mainClass, out string _outResult); 
        public abstract void UnloadData(Type _mainClass);

        public static UDB CreateInstance() { return new Internal.DB(); }
        #endregion
    }
}

namespace GamePlay.Internal 
{
    class DB : UDB
    {

        class Data
        {
            /// <summary>
            /// 数据结构的字段属性信息
            /// </summary>
            public List<IProperty> Propertys { get; private set; }

            /// <summary>
            /// 存储实例数据的队列:类型为:xxx[]
            /// </summary>
            public Object DataArray { get; private set; }

            public Type MainType { get; private set; }

            /// <summary>
            /// 快速缓冲区
            /// </summary>
            DataCache Cache;

            public Data ( List<IProperty> _propertys, Object _dataArray, Type _mainType )
            {
                MainType = _mainType;
                Propertys = _propertys;
                DataArray = _dataArray;
                Cache = new DataCache ();
            }

            public bool CheckoutCache ( string _condition, out object _outValue,bool _isCache)
            {
                _outValue = false;
                if ( !_isCache ) return false;
                return Cache.TryGetout ( _condition, out _outValue );
            }

            public void SaveToCache ( string _condition, object _value,bool _isSave)
            {
                if ( _isSave )
                {
                    Cache.SaveToCache ( _condition, _value );
                }
            }

            public void ClearCacheData ()
            {
                Cache.ClearCache ();
            }

            public void Clear()
            {
                Cache.ClearCache ();
                Propertys.Clear();
                DataArray = null;
            }
        } 

        /// <summary>
        /// 存储所有加载过的数据结构
        /// </summary>
        Dictionary<string, Data> Datas = new Dictionary<string, Data>();

        /// <summary>
        /// 有效应用类型集合的管理者
        /// </summary>
        IDBPropertyConvertForDB ConvertBase = new DBPropertyConvertForDB();
         
        public override void Clear()
        {
            foreach (var v in Datas)
            {
                v.Value.Clear();
                ClearData(v.Value.MainType);
            }

            Datas.Clear();
        }  

        #region IDB接口实现
        public override bool LoadData(Type _mainClass, string _tableContent)
        {
            //> 检查数据类型的有效性
            if (null == _mainClass || false == _mainClass.IsClass || _mainClass.IsAbstract)
            {
                Output.Error("IDB：尝试载入无效的数据结构！");
                return false;
            }

            //> 检查填充内容的有效性
            if (null == _tableContent || _tableContent.Length == 0)
            {
                Output.Error("IDB：数据载入{0}失败!请确保载入数据有效！",_mainClass.Name);
                return false;
            }

            //> 检查有效的存储字段
            List<DBProperty> propertys = new List<DBProperty>();
            if (MakeValidPropertys(ref propertys, _mainClass))
            {
                Output.Error("IDB：载入失败！请确保{0}类中存在有效的存储字段！", _mainClass.Name);
                return false;
            }

            string[] lines = _tableContent.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string[] tabPropertys = lines[0].Split(new char[] { '\t' });
            DBProperty[] validProperty = new DBProperty[tabPropertys.Length];

            var valid = false;
            for (int i = 0, max = tabPropertys.Length; i < max; i++)
            {
                foreach (var v in propertys)
                {
                    if (v.GetName() == tabPropertys[i])
                    {
                        valid = true;
                        validProperty[i] = v;
                        break;
                    }
                }
            }

            if (false == valid)
            {
                Output.Error("IDB：载入失败！{0}类中没有发现与数据匹配的字段或变量！",_mainClass.Name);
                return false;
            }

            //> 最后一行数据的索引值
            int endLength = lines.Length - 1;

            //> 写入行数去掉末尾空行(wps导出的转义符文件总是多出一个空行)
            int maxCount = string.IsNullOrEmpty(lines[endLength]) ? endLength - 1 : endLength;

            //> 用来分割的转义符
            char[] splits = new char[] { '\t' };

            //> 表类的数组类型
            Type tabArray = _mainClass.MakeArrayType();

            //> 创建一个临时的数组存储器，大小由maxCount指定
            object[] datas = (object[])System.Activator.CreateInstance(tabArray, maxCount);

            //> 实际填充的个数
            int itemCount = 0;

            //> 当前操作的数据实例
            object itemCur = null;

            //> 遍历写入所有内容
            for (int i = 1, max = lines.Length; i < max; i++)
            {
                string[] column = lines[i].Split(splits);

                //> 跳过不完整的数据行
                if (column.Length != tabPropertys.Length) continue;

                //> 创建并向目标身上写入数据
                itemCur = System.Activator.CreateInstance(_mainClass);
                for (int n = 0, n_max = column.Length; n < n_max; n++)
                {
                    //> 跳过无效的属性字段
                    if (validProperty[n] != null)
                    {
                        validProperty[n].SetValue(itemCur, column[n]);
                    }
                }
                datas[itemCount++] = itemCur;
            }

            //> 矫正数组个数
            object[] final = datas;
            if (itemCount > 0)
            {
                if (itemCount != maxCount)
                {
                    final = (object[])System.Activator.CreateInstance(tabArray, itemCount);

                    Array.Copy(datas, final, final.Length);
                }
            }
            else
            {
                final = (object[])System.Activator.CreateInstance(tabArray, 0);
            }

            //> 创建该类型的列表对象
            //object arrayTable = System.Activator.CreateInstance(tabArray, (object)final);

            //> 检查有效的静态存储变量
            FieldInfo storageField = GetStorageField(_mainClass);
            if (null != storageField)
            {
                storageField.SetValue(null, final);
                Output.Log("IDB：已将数据写入 {0}[] {1};中！",_mainClass.Name,storageField.Name); 
            }

            //> 将列表对象填充到对应实例中
            IProperty[] iPropertys = (IProperty[])(object)propertys.ToArray();

            //> 将数据存储图表
            Datas[_mainClass.Name] = new Data(new List<IProperty>(iPropertys), final, _mainClass);

            Output.Log("IDB：[{0}] 解析完成！ 共计[{1}]行数据！",_mainClass.Name,final.Length); 
            return true;
        }

        public override bool TrySelect(string _className, string _condition, out object[] _outDataSet, bool _tryCache = false )
        {
            Data outDataValue;
            if (Datas.TryGetValue(_className, out outDataValue))
            {
                if (string.IsNullOrEmpty(_condition))
                {
                    _outDataSet =(object[])outDataValue.DataArray;
                    return true;
                }

                object outCacheData;
                if ( outDataValue.CheckoutCache ( _condition, out outCacheData, _tryCache ) )
                {
                    _outDataSet = ( (List<object>) outCacheData ).ToArray ();
                    return true;
                }

                DBCommand cmd = new DBCommand(outDataValue.Propertys);

                if (cmd.SetCommand(_condition))
                {
                    List<object> list = cmd.Search<object>((object[])outDataValue.DataArray);
                    outDataValue.SaveToCache ( _condition, list, _tryCache );
                    _outDataSet = list.ToArray();
                    return _outDataSet.Length > 0;
                }
            }
            _outDataSet = new object[0];
            return false;
        }

        public override object[] Select(string _className, string _condition, bool _tryCache = false)
        {
            Data outDataValue;
            if (Datas.TryGetValue(_className, out outDataValue))
            {
                if (string.IsNullOrEmpty(_condition))
                {
                    return (object[])outDataValue.DataArray;
                }

                object outCacheData;
                if ( outDataValue.CheckoutCache ( _condition, out outCacheData, _tryCache ) )
                {
                    return ( (List<object>) outCacheData ).ToArray ();
                }

                DBCommand cmd = new DBCommand(outDataValue.Propertys);

                if (cmd.SetCommand(_condition))
                {
                    var list = cmd.Search<object> ( (object[]) outDataValue.DataArray );
                    outDataValue.SaveToCache ( _condition, list, _tryCache );
                    return list.ToArray();
                }
            }
            return new object[0];
        }

        public override bool TrySelect<T>(string _condition, out T[] _outDataSet, bool _tryCache = false)
        {
            Data outDataValue;
            if (Datas.TryGetValue(typeof(T).Name, out outDataValue))
            {
                if (string.IsNullOrEmpty(_condition))
                {
                    _outDataSet = (T[])outDataValue.DataArray;
                    return true;
                }

                object outCacheData;
                if ( outDataValue.CheckoutCache ( _condition, out outCacheData, _tryCache ) )
                {
                    _outDataSet = ((List<T>) outCacheData ).ToArray();
                    return true;
                }

                DBCommand cmd = new DBCommand(outDataValue.Propertys);
                if (cmd.SetCommand(_condition))
                {
                    List<T> list = cmd.Search<T>((T[])outDataValue.DataArray);
                    outDataValue.SaveToCache ( _condition, list, _tryCache );
                    _outDataSet = list.ToArray();
                    return _outDataSet.Length > 0;
                }
            }
            _outDataSet = new T[0];
            return false;
        }

        public override T[] Select<T>( string _condition, bool _tryCache = false)
        {
            Data outDataValue;
            if (Datas.TryGetValue(typeof(T).Name, out outDataValue))
            {
                if (string.IsNullOrEmpty(_condition))
                {
                    return (T[])outDataValue.DataArray; 
                }

                object outCacheData;
                if ( outDataValue.CheckoutCache ( _condition, out outCacheData, _tryCache ) )
                {
                    return ( (List<T>) outCacheData ).ToArray ();
                }

                DBCommand cmd = new DBCommand(outDataValue.Propertys);

                if (cmd.SetCommand(_condition))
                {
                    List<T> list = cmd.Search<T>((T[])outDataValue.DataArray);
                    outDataValue.SaveToCache( _condition, list, _tryCache );
                    return list.ToArray ();
                }
            }
            return new T[0];
        }

        public override List<UDBPropertyTitleDB> GetTitle(string _className) 
        {
            List<UDBPropertyTitleDB> result = new List<UDBPropertyTitleDB>();
            Data outDataValue;
            if (Datas.TryGetValue(_className, out outDataValue))
            {
                foreach (var v in outDataValue.Propertys) 
                {
                    result.Add(new UDBPropertyTitleDB(v.PropertyName,v.PropertyTypeName,v.PropertyValue));
                }
            }

            return result;
        }

        public override void ClearCacheData ( string _calssName )
        {
            Data outDataValue;
            if ( Datas.TryGetValue ( _calssName, out outDataValue ) )
            {
                outDataValue.ClearCacheData ();
            }
        }

        public override bool CreateContent(Type _mainClass,out string _outResult)
        {
            Data outDataValue;
            if (Datas.TryGetValue(_mainClass.Name, out outDataValue))
            {
                return MakeTableContent(out _outResult, _mainClass, (object[])outDataValue.DataArray);
            }
            else
            {
                _outResult = "";
                FieldInfo info = GetStorageField (_mainClass);
                if ( info == null ) return false;

                object array = info.GetValue (null);

                if ( array == null ) return false;

                return MakeTableContent ( out _outResult, _mainClass, (object[]) array );
            }
        }

        public override void UnloadData(Type _mainClass)
        {
            //> 移除记录
            Data outData;
            if (Datas.TryGetValue(_mainClass.Name, out outData))
            {
                outData.Clear();
                ClearData(_mainClass);
                Datas.Remove(_mainClass.Name);
            }
        }
        #endregion

        #region 内部维护函数
        bool MakeTableContent(out string _outContent, Type _mainClass, object[] _datas)
        {
            _outContent = "";

            if (_mainClass == null || _datas == null)
                return false;

            List<DBProperty> propertys = new List<DBProperty>();
            if (MakeValidPropertys(ref propertys, _mainClass))
                return false;

            //> 写入表头
            string line = propertys[0].GetName();
            for (int i = 1, max = propertys.Count; i < max; ++i)
            {
                line += '\t' + propertys[i].GetName();
            }
            line += "\r\n";

            //> 写入数据流
            foreach (var v in _datas)
            {
                if (null == v) continue;

                line += propertys[0].GetValue(v);
                for (int i = 1, max = propertys.Count; i < max; ++i)
                {
                    line += '\t' + propertys[i].GetValue(v);
                }
                line += "\r\n";
            }

            _outContent = line;
            return true;
        }

        bool MakeValidPropertys(ref List<DBProperty> _propertys, Type _mainClass)
        {
            FieldInfo[] fieldInfos = _mainClass.GetFields();
            PropertyInfo[] propertyInfos = _mainClass.GetProperties();

            DBPropertyConvertInfo addItem;
            foreach (var v in fieldInfos)
            {
                if (v.IsStatic) continue;
                if ((addItem = ConvertBase.FindBaseInfo(v.FieldType)) != null)
                    _propertys.Add(new ValueField(addItem, v));
            }

            foreach (var v in propertyInfos)
            {
                if ((addItem = ConvertBase.FindBaseInfo(v.PropertyType)) != null)
                    _propertys.Add(new ValueProperty(addItem, v));
            }

            return 0 == _propertys.Count;
        }

        //object[] GetArrayData(Type _mainClass)
        //{

        //    FieldInfo fieldStorage = GetStorageField(_mainClass);

        //    if (null == fieldStorage)
        //        return null;

        //    object listData = fieldStorage.GetValue(null);

        //    if (null == listData)
        //        return null;

        //    MethodInfo toArray = listData.GetType().GetMethod("ToArray");

        //    return (object[])toArray.Invoke(listData, null);
        //}

        void ClearData(Type _storageType)
        {
            FieldInfo fieldStorage = GetStorageField(_storageType);

            if (fieldStorage != null)
            {
                fieldStorage.SetValue(null, null);
            }
        }

        //> for array
        FieldInfo GetStorageField(Type _mainClass)
        {
            Type fieldType = _mainClass.MakeArrayType();
            FieldInfo[] infos = _mainClass.GetFields();

            foreach (var v in infos)
            {
                if (v.IsStatic && v.FieldType == fieldType)
                {
                    return v;
                }
            }

            return null;
        }

        //> for list


        bool CheckoutValidStorage(FieldInfo _info, Type _mainClass)
        {
            //return _info.FieldType.Name == "List`1" && (_info.FieldType.GetGenericArguments()[0]) == _mainClass;
            return _info.FieldType.Name == _mainClass.Name && _info.FieldType.IsArray;
        }
        #endregion
    }
}