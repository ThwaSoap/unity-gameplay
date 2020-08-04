using System;
using System.Collections.Generic;

namespace GamePlay.Internal
{
    class DBPropertyStorage : IProperty
    {
        DBPropertyConvertInfo ConvertInfo;
        string FieldName;
        int FieldIndex;
        public DBPropertyStorage(string _fieldName,int _fieldIndex,DBPropertyConvertInfo _convertInfo)
        {
            FieldName = _fieldName;
            FieldIndex = _fieldIndex;
            ConvertInfo = _convertInfo;
        }

        /// <summary>
        /// 这个返回字段属性的名称.如: int number,返回number字样.
        /// </summary>
        /// <returns>字段属性的名称.</returns>
        public string GetName()
        {
            return FieldName;
        }

        /// <summary>
        /// 为目标丢向填充字符串数据.
        /// </summary>
        /// <param name="_target">被填充的目标对象.</param>
        /// <param name="_value">填充的源数据.</param>
        public void SetValue(object _target, string _value)
        {
            (_target as DBContent)[FieldIndex].DefaultValue = ConvertInfo.OnConvertToObject(_value);
        }

        /// <summary>
        /// 获取目标对象的字符串数据.
        /// </summary>
        /// <returns>返回的源数据.</returns>
        /// <param name="_target">获取的目标对象.</param>
        public string GetValue(object _target)
        {
            return ConvertInfo.OnConvertToStirng((_target as DBContent)[FieldIndex].DefaultValue);
        }

        #region 属性接口
        object IProperty.PropertyValue(object _target)
        {
            return (_target as DBContent)[FieldIndex].DefaultValue;
        }

        string IProperty.PropertyName
        {
            get { return FieldName; }
        }

        Type IProperty.PropertyType
        {
            get { return ConvertInfo.BaseType; }
        }
        string IProperty.PropertyTypeName
        {
            get { return ConvertInfo.TypeName; }
        }

        bool IProperty.TryGetValue(string _content, out object _result)
        {
            return ConvertInfo.DataType.GetValue(_content, out _result);
        }

        bool IProperty.TryGetCompare(string _sign, out Func<object, object, bool> _outCompare)
        {
            return ConvertInfo.DataType.GetCompare(_sign, out _outCompare);
        }

        int IProperty.CheckoutContentLength(string _content)
        {
            return ConvertInfo.DataType.CheckoutContentLength(_content);
        }

        List<string> IProperty.Signs
        {
            get { return ConvertInfo.DataType.Signs; }
        }
        #endregion
    }
}
