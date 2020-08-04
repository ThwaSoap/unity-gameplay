using System;
using System.Collections.Generic;
using System.Reflection;

namespace GamePlay.Internal
{
    abstract class DBProperty : IProperty
    { 
        /// <summary>
        /// 匹配到的表对象基本数据信息.
        /// </summary>
        protected DBPropertyConvertInfo ConvertInfo;

        public DBProperty(DBPropertyConvertInfo _convertInfo)
        {
            ConvertInfo = _convertInfo;
        }

        /// <summary>
        /// 这个返回字段属性的名称.如: int number,返回number字样.
        /// </summary>
        /// <returns>字段属性的名称.</returns>
        public abstract string GetName();

        /// <summary>
        /// 为目标丢向填充字符串数据.
        /// </summary>
        /// <param name="_target">被填充的目标对象.</param>
        /// <param name="_value">填充的源数据.</param>
        public void SetValue(object _target, string _value)
        {
            WriteValue(_target, ConvertInfo.OnConvertToObject(_value));
        }

        /// <summary>
        /// 获取目标对象的字符串数据.
        /// </summary>
        /// <returns>返回的源数据.</returns>
        /// <param name="_target">获取的目标对象.</param>
        public string GetValue(object _target)
        {
            return ConvertInfo.OnConvertToStirng(ReadValue(_target));
        }

        /// <summary>
        /// 该函数将_value数据写入到_target中.
        /// </summary>
        /// <param name="_target">写入目标.</param>
        /// <param name="_value">写入数据.</param>
        protected abstract void WriteValue(object _target, object _value);

        protected abstract object ReadValue(object _target);

        #region 实现字段属性接口
        object IProperty.PropertyValue(object _target)
        {
            return ReadValue(_target);
        }
        string IProperty.PropertyName 
        {
            get{ return GetName(); } 
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
            return ConvertInfo.DataType.GetValue(_content,out _result);
        } 
        
        bool IProperty.TryGetCompare(string _sign, out Func<object, object, bool> _outCompare) 
        {
            return ConvertInfo.DataType.GetCompare(_sign,out _outCompare);
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

    class ValueField : DBProperty
    {
        private FieldInfo Info;

        public ValueField(DBPropertyConvertInfo _convertInfo, FieldInfo _info)
            : base(_convertInfo)
        {
            Info = _info;
        }

        #region 实现抽象接口
        public override string GetName()
        {
            return Info.Name;
        }

        protected override void WriteValue(object _target, object _value)
        {
            Info.SetValue(_target, _value);
        }
        protected override object ReadValue(object _target) 
        {
            return Info.GetValue(_target);
        }

        #endregion
    }

    class ValueProperty : DBProperty
    {
        private PropertyInfo Info;

        public ValueProperty(DBPropertyConvertInfo _convertInfo, PropertyInfo _info)
            : base(_convertInfo)
        {
            Info = _info;
        }

        #region 实现抽象接口
        public override string GetName()
        {
            return Info.Name;
        }

        protected override void WriteValue(object _target, object _value)
        {
            Info.SetValue(_target, _value,null);
        }

        protected override object ReadValue(object _target)
        {
            return Info.GetValue(_target,null);
        }

        #endregion
    }
}
