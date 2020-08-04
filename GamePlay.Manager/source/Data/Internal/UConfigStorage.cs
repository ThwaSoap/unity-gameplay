using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    public abstract class UConfigStorage 
    {
        internal Internal.ILog Output { get; set; }
        public abstract void Clear();

        public abstract void InitializedCustomEnum(Type[] _types);

        public abstract bool LoadData(string _content, string _key);

        public abstract bool TryGet(string _mainClass, out DBContent _outObject, string _key);

        public abstract DBContent Get(string _mainClass, string _key);

        public abstract bool TryGetValue(string _mainClass, string _property, out DBValue _outValue, string _key);

        public abstract DBValue GetValue(string _mainClass, string _property, string _key);

        public abstract bool CreateContent(out string _outContent, string _key);

        public abstract void UnloadData(string _key);
    }
}

namespace GamePlay.Internal
{ 
    class ConfigStorage : UConfigStorage
    {
        Dictionary<string, List<DBContentDynamic>> Datas = new Dictionary<string, List<DBContentDynamic>>();
        IDBPropertyConvertForStorage ConvertBase = new DBPropertyConvertForStorage();

        public override void Clear()
        {
            Datas.Clear();
        }

        #region 系统接口
        public override void InitializedCustomEnum (Type[] _types)
        {
            ConvertBase.InitializedCustomEnum (_types);
        }

        public override bool LoadData(string _content, string _key)
        {
            if (string.IsNullOrEmpty(_key))
            {
                Output.Error("IConfigStorage：指定密钥无效！载入数据失败！");
                return false;
            }

            if (_content == null || _content.Length == 0)
            {
                Output.Error("IConfigStorage：数据载入 {0} 失败！请确保数据内容有效！", _key);
                return false;
            }

            string[] lines = _content.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            string temp;
            string stringTypeName = typeof(string).Name;
            DBContentDynamic currentObject = null;
            List<DBContentDynamic> validObject = new List<DBContentDynamic>();
            for (int i = 0, max = lines.Length; i < max; i++)
            {
                temp = lines[i].Trim();

                //> 解析对象
                if (temp.Length > 2 && temp[0] == '[' && temp[temp.Length - 1] == ']')
                {
                    //> 清理上一个对象的数据
                    currentObject = null;

                    //> 获得名称，这个名称应该与配置类型配对
                    temp = temp.Remove(0, 1);
                    temp = temp.Remove(temp.Length - 1, 1);

                    //> 查找历史对象，如果没有就创建一个新对象
                    currentObject = validObject.Find((v) => { return v.ClassName == temp; });

                    if (null == currentObject)
                    {
                        currentObject = new DBContentDynamic(temp);

                        validObject.Add(currentObject);
                    }
                }
                else if (currentObject != null)
                {
                    var index = temp.IndexOf('=');

                    if (index == -1) continue;

                    //> 获取'='号前面的值
                    var property = temp.Substring(0, index).Trim();

                    if (property.Length == 0) continue;

                    //> 解析类型和变量名
                    var idxPoint = property.IndexOf('.');
                    string propertyType = "";
                    string propertyName = "";
                    if (idxPoint != -1)
                    {
                        propertyType = property.Substring(0, idxPoint).Trim();
                        propertyName = property.Remove(0, idxPoint + 1).Trim();
                    }
                    else
                    {
                        propertyName = property;
                    }

                    //> 获取'='号尾部的值
                    string value = temp.Remove(0, index + 1).Trim();
                    bool isUseStringToken = false;
                    if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
                    {
                        isUseStringToken = true;
                        value = value.Substring(1, value.Length - 2);
                    }

                    //> 解析数据的转换类型
                    DBPropertyConvertInfo convertInfo;
                    //Type applyType = null;

                    if (propertyType.Length == 0)
                    {
                        //> 没有明确定义类型，默认成字符串类型 
                        convertInfo = ConvertBase.FindBaseInfo(stringTypeName);
                    }
                    else
                    {
                        convertInfo = ConvertBase.FindBaseInfo(propertyType);

                        if (convertInfo == null)
                        {
                            //> 有类型，但类型解析错误，默认成字符串类型
                            convertInfo = ConvertBase.FindBaseInfo(stringTypeName);
                        }
                        /*else
                        {
                            //> 有类型，类型解析成功
                            applyType = convertInfo.BaseType;
                        }*/
                    }

                    var obj = convertInfo.OnConvertToObject(value);

                    currentObject.AddProperty(propertyName, new DBValueDynamic(convertInfo, propertyName, isUseStringToken, obj));
                }
            }

            Datas[_key] = validObject;
            Output.Log("IConfigStorage：{0}解析完毕！共计[{1}]个配置对象！", _key, validObject.Count);
            return true;
        }

        public override bool TryGet(string _mainClass, out DBContent _outObject, string _key)
        {
			_outObject = Get(_mainClass, _key);
            return _outObject != null;
        }

        public override DBContent Get(string _mainClass, string _key)
        {
            List<DBContentDynamic> outList;
            if (Datas.TryGetValue(_key, out outList))
            {
                return outList.Find((v) => { return v.ClassName == _mainClass; });
            }
            return null;
        }

        public override bool TryGetValue(string _mainClass, string _property, out DBValue _outValue, string _key)
        {
            _outValue = GetValue(_mainClass, _property, _key);
            return _outValue != null;
        }

        public override DBValue GetValue(string _mainClass, string _property, string _key)
        {
            DBContent outContent;
            if (false == TryGet(_mainClass, out outContent, _key))
                return null;

            return outContent[_property];
        }

        public override bool CreateContent(out string _outContent, string _key)
        {
            List<DBContentDynamic> outList; _outContent = "";
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
            List<DBContentDynamic> outList;
            if (Datas.TryGetValue(_key, out outList))
            {
                outList.Clear();
                Datas.Remove(_key);
            }
        }
        #endregion

        #region 内维函数
        string GetObjectContent(DBContentDynamic _object)
        {
            var result = "[" + _object.ClassName + "]";
            DBValueDynamic valueDynamic;
            for (int i = 0, max = _object.Length; i < max; i++)
            {
                valueDynamic = _object[i] as DBValueDynamic;

                if (valueDynamic.ValueType != null)
                {
                    result += "\r\n" + valueDynamic.ValueType.TypeName + "." + valueDynamic.FieldName + "=" + valueDynamic.StringValue;
                }
                else
                {
                    result += "\r\n" + valueDynamic.FieldName + "=" + valueDynamic.StringValue;
                }
            }
            return result;
        }
        #endregion
    }
}


