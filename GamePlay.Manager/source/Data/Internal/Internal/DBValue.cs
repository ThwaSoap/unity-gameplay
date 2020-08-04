using System;

namespace GamePlay
{
    public class DBValue
    {
        public object DefaultValue { get; set; }

        public string String
        {
            get
            {
                return GetStringByObject() as string;
            }
            set
            {
                SetStringValue(value);
            }
        }

        public double Double
        {
            get
            {
                return GetDoubleByObject();
            }
            set
            {
                SetDoubleValue(value);
            }
        }

        public float Float
        {
            get
            {
                return GetFloatByObject();
            }
            set
            {
                SetFloatValue(value);
            }
        }
        public long Long
        {
            get
            {
                return GetLongByObject();
            }
            set
            {
                SetLongValue(value);
            }
        }
        public int Int
        {
            get
            {
                return GetIntByObject();
            }
            set
            {
                SetIntValue(value);
            }
        }

        public short Short
        {
            get
            {
                return GetShortByObject();
            }
            set
            {
                SetShortValue(value);
            }
        }

        public bool Bool
        {
            get
            {
                return GetBoolByObject();
            }
            set
            {
                SetBoolValue(value);
            }
        }

        public T GetEnum<T>()
        {
            if (DefaultValue is T)
            {
                return (T)DefaultValue;
            }
            return default(T);
        }

        public void SetEnum<T>(T _value)
        {
            if (typeof(T).IsEnum)
            {
                DefaultValue = _value;
            }
        }

        object GetStringByObject()
        {
            if (DefaultValue is string)
            {
                return DefaultValue;
            }
            else
            {
                return DefaultValue.ToString();
            }
        }


        /*T GetNumberByObject<T>(object _target,Func<string,T> _strToTarget)
        {
            if (_target is T)
            {
                return (T)_target;
            }
            else if (_target is string)
            { 
                return _strToTarget(_target as string);
            }
            else if (_target is bool)
            {
                return ((bool)_target) ? (T)(object)1 : (T)(object)0;
            }
            else
            {
                return (T)(_target);
            }
        }*/


        double GetDoubleByObject()
        {
            if (DefaultValue is double)
                return (double)DefaultValue;
            else if (DefaultValue is float)
                return (double)(float)DefaultValue;
            else if (DefaultValue is int)
                return (double)(int)DefaultValue;
            else if (DefaultValue is short)
                return (double)(short)DefaultValue;
            else if (DefaultValue is long)
                return (double)(long)DefaultValue;
            else if (DefaultValue is string)
                return SToD(DefaultValue as string);
            else if (DefaultValue is bool)
                return (bool)DefaultValue ? (double)1 : (double)0;
            else
                return 0;
        }

        float GetFloatByObject()
        {
            if (DefaultValue is float)
                return (float)DefaultValue;
            else if (DefaultValue is double)
                return (float)(double)DefaultValue;
            else if (DefaultValue is int)
                return (float)(int)DefaultValue;
            else if (DefaultValue is short)
                return (float)(short)DefaultValue;
            else if (DefaultValue is long)
                return (float)(long)DefaultValue;
            else if (DefaultValue is string)
                return SToF(DefaultValue as string);
            else if (DefaultValue is bool)
                return (bool)DefaultValue ? (float)1 : (float)0;
            else
                return 0;
        }

        long GetLongByObject()
        {
            if (DefaultValue is long)
                return (long)DefaultValue;
            else if (DefaultValue is int)
                return (long)(int)DefaultValue;
            else if (DefaultValue is short)
                return (long)(short)DefaultValue;
            else if (DefaultValue is double)
                return (long)(double)DefaultValue;
            else if (DefaultValue is float)
                return (long)(float)DefaultValue;
            else if (DefaultValue is string)
                return SToL(DefaultValue as string);
            else if (DefaultValue is bool)
                return (bool)DefaultValue ? (long)1 : (long)0;
            else
                return 0;
        }

        int GetIntByObject()
        {
            if (DefaultValue is int)
                return (int)DefaultValue;
            else if (DefaultValue is long)
                return (int)(long)DefaultValue;
            else if (DefaultValue is short)
                return (int)(short)DefaultValue;
            else if (DefaultValue is double)
                return (int)(double)DefaultValue;
            else if (DefaultValue is float)
                return (int)(float)DefaultValue;
            else if (DefaultValue is string)
                return SToI(DefaultValue as string);
            else if (DefaultValue is bool)
                return (bool)DefaultValue ? (int)1 : (int)0;
            else
                return 0;
        }

        short GetShortByObject()
        {
            if (DefaultValue is short)
                return (short)DefaultValue;
            else if (DefaultValue is long)
                return (short)(long)DefaultValue;
            else if (DefaultValue is int)
                return (short)(int)DefaultValue;
            else if (DefaultValue is double)
                return (short)(double)DefaultValue;
            else if (DefaultValue is float)
                return (short)(float)DefaultValue;
            else if (DefaultValue is string)
                return SToSh(DefaultValue as string);
            else if (DefaultValue is bool)
                return (bool)DefaultValue ? (short)1 : (short)0;
            else
                return 0;
        }


        bool GetBoolByObject()
        {
            if (DefaultValue is bool)
                return (bool)DefaultValue;
            else if (DefaultValue is string)
                return SToB(DefaultValue as string);
            else if (DefaultValue is long)
                return ((long)DefaultValue) > 0 ? true : false;
            else if (DefaultValue is int)
                return ((int)DefaultValue) > 0 ? true : false;
            else if (DefaultValue is short)
                return ((short)DefaultValue) > 0 ? true : false;
            else if (DefaultValue is double)
                return ((double)DefaultValue) > 0.0 ? true : false;
            else if (DefaultValue is float)
                return ((float)DefaultValue) > 0.0f ? true : false;
            else
                return false;
        }

        double SToD(string _text)
        {
            double outValue;
            double.TryParse(_text, out outValue);
            return outValue;
        }
        float SToF(string _text)
        {
            float outValue;
            float.TryParse(_text, out outValue);
            return outValue;
        }
        long SToL(string _text)
        {
            long outValue;
            long.TryParse(_text, out outValue);
            return outValue;
        }

        int SToI(string _text)
        {
            int outValue;
            int.TryParse(_text, out outValue);
            return outValue;
        }


        short SToSh(string _text)
        {
            short outValue;
            short.TryParse(_text, out outValue);
            return outValue;
        }
        bool SToB(string _text)
        {
            bool outValue;
            bool.TryParse(_text, out outValue);
            return outValue;
        }

        void SetStringValue(string _value)
        {
            if (DefaultValue is string)
                DefaultValue = _value;
            else if (DefaultValue is bool)
                DefaultValue = SToB(_value);
            else if (DefaultValue is double)
                DefaultValue = SToD(_value);
            else if (DefaultValue is float)
                DefaultValue = SToF(_value);
            else if (DefaultValue is long)
                DefaultValue = SToL(_value);
            else if (DefaultValue is int)
                DefaultValue = SToI(_value);
            else if (DefaultValue is short)
                DefaultValue = SToSh(_value);

        }

        void SetDoubleValue(double _value)
        {
            if (DefaultValue is double)
                DefaultValue = _value;
            else if (DefaultValue is string)
                DefaultValue = _value.ToString();
            else if (DefaultValue is bool)
                DefaultValue = _value > 0.0 ? true : false;
            else if (DefaultValue is float)
                DefaultValue = (float)_value;
            else if (DefaultValue is long)
                DefaultValue = (long)_value;
            else if (DefaultValue is int)
                DefaultValue = (int)_value;
            else if (DefaultValue is short)
                DefaultValue = (short)_value;
        }

        void SetFloatValue(float _value)
        {
            if (DefaultValue is float)
                DefaultValue = _value;
            else if (DefaultValue is string)
                DefaultValue = _value.ToString();
            else if (DefaultValue is bool)
                DefaultValue = _value > 0 ? true : false;
            else if (DefaultValue is double)
                DefaultValue = (double)_value;
            else if (DefaultValue is long)
                DefaultValue = (long)_value;
            else if (DefaultValue is int)
                DefaultValue = (int)_value;
            else if (DefaultValue is short)
                DefaultValue = (short)_value;
        }

        void SetLongValue(long _value)
        {
            if (DefaultValue is long)
                DefaultValue = _value;
            else if (DefaultValue is int)
                DefaultValue = (int)_value;
            else if (DefaultValue is short)
                DefaultValue = (short)_value;
            else if (DefaultValue is double)
                DefaultValue = (double)_value;
            else if (DefaultValue is float)
                DefaultValue = (float)_value;
            else if (DefaultValue is string)
                DefaultValue = _value.ToString();
            else if (DefaultValue is bool)
                DefaultValue = _value > 0 ? true : false;
        }

        void SetIntValue(int _value)
        {
            if (DefaultValue is int)
                DefaultValue = _value;
            else if (DefaultValue is long)
                DefaultValue = (long)_value;
            else if (DefaultValue is short)
                DefaultValue = (short)_value;
            else if (DefaultValue is double)
                DefaultValue = (double)_value;
            else if (DefaultValue is float)
                DefaultValue = (float)_value;
            else if (DefaultValue is string)
                DefaultValue = _value.ToString();
            else if (DefaultValue is bool)
                DefaultValue = _value > 0 ? true : false;
        }

        void SetShortValue(int _value)
        {
            if (DefaultValue is short)
                DefaultValue = _value;
            else if (DefaultValue is long)
                DefaultValue = (long)_value;
            else if (DefaultValue is int)
                DefaultValue = (int)_value;
            else if (DefaultValue is double)
                DefaultValue = (double)_value;
            else if (DefaultValue is float)
                DefaultValue = (float)_value;
            else if (DefaultValue is string)
                DefaultValue = _value.ToString();
            else if (DefaultValue is bool)
                DefaultValue = _value > 0 ? true : false;
        }

        void SetBoolValue(bool _value)
        {
            if (DefaultValue is bool)
                DefaultValue = _value;
            else if (DefaultValue is long)
                DefaultValue = _value ? (long)1 : (long)0;
            else if (DefaultValue is int)
                DefaultValue = _value ? (int)1 : (int)0;
            else if (DefaultValue is short)
                DefaultValue = _value ? (short)1 : (short)0;
            else if (DefaultValue is double)
                DefaultValue = _value ? (double)1 : (double)0;
            else if (DefaultValue is float)
                DefaultValue = _value ? (float)1 : (float)0;
            else if (DefaultValue is string)
                DefaultValue = _value.ToString();
        }
    }
}
namespace GamePlay.Internal
{
    class DBValueDynamic : DBValue 
    {
        /// <summary>
        /// 如果该值为Null,则表明默认使用字符串类型，
        /// 否则，它对应当前值的实际数据类型，并影响
        /// 存储时，是否增加类型前缀(如果为Null则在，
        /// 存储时不增加类型前缀)。
        /// </summary>
        public DBPropertyConvertInfo ValueType { get; private set; }

        /// <summary>
        /// 变量的名称
        /// </summary>
        public string FieldName { get; private set; }

        //public IDBValue Interface { get { return this; } }

        bool IsUseStringToken;

        public string StringValue 
        {
            get 
            {
                if (IsUseStringToken)
                    return "\"" + this.String + "\"";
                else
                    return this.String;
            }
        }

        public DBValueDynamic(DBPropertyConvertInfo _valueType, string _fieldName, bool _isUseStringToken, object _defaultValue) 
        {
            ValueType = _valueType;
            FieldName = _fieldName;
            IsUseStringToken = _isUseStringToken;
            DefaultValue = _defaultValue;
        } 
    }
}
