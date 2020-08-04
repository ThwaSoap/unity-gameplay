using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
namespace GamePlay 
{
    public abstract class UConfig
    {
        internal Internal.ILog Output { get; set; } 
        public abstract void Clear();

        public abstract void Initialized(Type[] _configTypes);

        public abstract bool LoadData(string _content, string _key);

        public abstract bool TryGet<T>(out T _outObject, string _key) where T : class;

        public abstract T Get<T>(string _key) where T : class;

        public abstract bool CreateContent(out string _outContent, string _key);

        public abstract void UnloadData(string _key);
    }
}

namespace GamePlay.Internal
{
    using Internal;
    class Config : UConfig
    {
        Dictionary<string, List<Object>> Datas = new Dictionary<string, List<object>>();
        DBPropertyConvertForDB ConvertBase = new DBPropertyConvertForDB();
        List<Type> ConfigTypes = new List<Type>();



        #region 配置接口
        public override void Clear()
        {
            Datas.Clear();
            ConfigTypes.Clear();
        }

        public override void Initialized(Type[] _configTypes)
        {
            foreach (var v in _configTypes)
            {
                if (!v.IsClass || v.IsAbstract) continue;

                ConfigTypes.Add(v);
            }
        }

        public override bool LoadData(string _content, string _key)
        {
            if (string.IsNullOrEmpty(_key))
            {
                Output.Error("IConfig：指定密钥无效！载入数据失败！");
                return false;
            }

            if (_content == null || _content.Length == 0)
            {
                Output.Error("IConfig：数据载入 {0} 失败！请确保数据内容有效！", _key);
                return false;
            }

            string[] lines = _content.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            string temp;
            Object currentObject = null;
            List<DBProperty> validPropertys = new List<DBProperty>();
            List<Object> validObject = new List<Object>();
            for (int i = 0, max = lines.Length; i < max; i++)
            {
                temp = lines[i].Trim();

                //> 解析对象
                if (temp.Length > 2 && temp[0] == '[' && temp[temp.Length - 1] == ']')
                {
                    //> 清理上一个对象的数据
                    currentObject = null;
                    validPropertys.Clear();

                    //> 获得名称，这个名称应该与配置类型配对
                    temp = temp.Remove(0, 1);
                    temp = temp.Remove(temp.Length - 1, 1);

                    if (temp.Length == 0) continue;

                    //> 查找对应的配置类型，如果没有，跳过
                    var type = ConfigTypes.Find((v) => { return v.Name == temp; });

                    if (type == null) continue;

                    //> 获取类的有效字段属性,如果没有有效的字段成员，跳过

                    if (MakeValidPropertys(ref validPropertys, type))
                    {
                        //> 新的对象的数据覆盖掉历史对象的数据，没有历史对象就创建新对象

                        currentObject = validObject.Find((v) => { return v.GetType().Equals(type); });

                        if (null == currentObject)
                        {
                            currentObject = Activator.CreateInstance(type);

                            //> 找到这个类设置的对应存储变量，将他填充进去
                            #region 将数据存入用户自定义的存储单位
                            var fieldStorage = GetStorageField(type, _key + type.Name);

                            if (fieldStorage != null)
                            {
                                fieldStorage.SetValue(null, currentObject);
                            }
                            #endregion

                            validObject.Add(currentObject);
                        }
                    }
                }
                else if (currentObject != null)
                {
                    var index = temp.IndexOf('=');

                    if (index == -1) continue;

                    //> 获取'='号前面的值
                    var property = temp.Substring(0, index).Trim();

                    if (property.Length == 0) continue;

                    DBProperty fieldInterface = validPropertys.Find((v) => { return v.GetName() == property; });

                    if (null == fieldInterface) continue;

                    //> 获取'='号尾部的值
                    var value = temp.Remove(0, index + 1).Trim();

                    if (value.Length == 0) continue;

                    //> 为对象填充数据
                    fieldInterface.SetValue(currentObject, value);
                }
            }

            Datas[_key] = validObject;

            Output.Log("IConfig：{0}解析完毕！共计[{1}]个配置对象！", _key, validObject.Count);
            return true;
        }

        public override bool TryGet<T>(out T _outObject, string _key)
        {
            _outObject = Get<T>(_key);
            return _outObject != null;
        }

        public override T Get<T>(string _key)
        {
            List<Object> outList;
            if (Datas.TryGetValue(_key, out outList))
            {
                Object result = outList.Find((v) => { return v is T; });

                if (result != null)
                {
                    return result as T;
                }
            }
            return default(T);
        }

        public override bool CreateContent(out string _outContent, string _key)
        {
            List<Object> outList; _outContent = "";
            if (Datas.TryGetValue(_key, out outList))
            {
                if (outList.Count == 0) return false;

                _outContent += GetObjectContent(outList[0]);

                for (int i = 1, max = outList.Count; i < max; i++)
                {
                    _outContent += "\r\n\r\n" + GetObjectContent(outList[i]);
                }
                return true;
            }
            return false;
        }

        public override void UnloadData(string _key)
        {
            List<Object> outList;
            if (Datas.TryGetValue(_key, out outList))
            {
                outList.Clear();
                Datas.Remove(_key);
            }
        }
        #endregion

        #region 内维函数
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

            return _propertys.Count > 0;
        }

        FieldInfo GetStorageField(Type _mainClass, string _propertyName)
        {
            FieldInfo[] infos = _mainClass.GetFields();

            foreach (var v in infos)
            {
                if (v.IsStatic && v.FieldType == _mainClass && v.Name == _propertyName)
                {
                    return v;
                }
            }

            return null;
        }

        string GetObjectContent(Object _object)
        {
            Type mainClass = _object.GetType();
            List<DBProperty> validPropertys = new List<DBProperty>();
            if (MakeValidPropertys(ref validPropertys, mainClass))
            {
                var result = "[" + mainClass.Name + "]";

                foreach (var v in validPropertys)
                {
                    result += "\r\n" + v.GetName() + "=" + v.GetValue(_object);
                }
                return result;
            }
            return "";
        } 
        #endregion
    }
}


