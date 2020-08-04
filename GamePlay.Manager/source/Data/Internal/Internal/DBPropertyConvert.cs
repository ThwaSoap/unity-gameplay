using System;
using System.Collections.Generic;

namespace GamePlay.Internal
{ 
    class DBPropertyConvertInfo 
    {
        public Type BaseType { get; private set; }
        public IDataType DataType { get; private set; }
        public Func<string, object> OnConvertToObject { get; private set; }
        public Func<object, string> OnConvertToStirng { get; private set; }
        string SubName;
        
        public DBPropertyConvertInfo(Type _baseType,string _subName,IDataType _dataType, Func<string, object> _onConverToObject, Func<object, string> _onConverToStirng) 
        {
            BaseType = _baseType;
            DataType = _dataType;
            OnConvertToObject = _onConverToObject;
            OnConvertToStirng = _onConverToStirng;
            if (_subName != null)
                SubName = _subName.ToLower();
        }

        public DBPropertyConvertInfo(Type _baseType,string _subName,IDataType _dataType, Func<object, string> _onConverToStirng) 
        {
            BaseType = _baseType;
            DataType = _dataType;
            OnConvertToObject = ForEnum;
            OnConvertToStirng = _onConverToStirng;
            if ( _subName != null )
                SubName = _subName.ToLower();
        }

        public bool IsType(string _name)
        {
            if (SubName == null)
                return BaseType.Name.ToLower() == _name.ToLower();
            else
                return SubName == _name.ToLower();
        }

        public string TypeName
        {
            get
            {
                if (SubName == null)
                    return BaseType.Name;
                else
                    return SubName;
            }
        }

        object ForEnum(string _value) 
        {
            return Enum.Parse(BaseType,_value,true);
        }
    } 

    abstract class DBPropertyConvert
    {
        protected List<DBPropertyConvertInfo> Infos = new List<DBPropertyConvertInfo>(); 

        public DBPropertyConvert() 
        {
            Infos.Add(new DBPropertyConvertInfo(typeof(bool),"bool",new DBDataBool(),ToBool, ObjectTo));
            Infos.Add(new DBPropertyConvertInfo(typeof(short),"short", new DBDataShort(), ToShort, ObjectTo));
            Infos.Add(new DBPropertyConvertInfo(typeof(int),"int",new DBDataInt(), ToInt, ObjectTo));
            Infos.Add(new DBPropertyConvertInfo(typeof(long),"long",new DBDataLong(), ToLong, ObjectTo));
            Infos.Add(new DBPropertyConvertInfo(typeof(float),"float",new DBDataFloat(), ToFloat, ObjectTo));
            Infos.Add(new DBPropertyConvertInfo(typeof(double),"double",new DBDataDouble(), ToDouble, ObjectTo));
            Infos.Add(new DBPropertyConvertInfo(typeof(string),"string",new DBDataString(),(v) => { return v; }, (v) => { return (string)v; }));
        }

        #region Convert
        protected string ObjectTo(object _object) 
        {
            return _object.ToString();
        }
        protected object ToEnum(Type _type, string _value) 
        {
            return Enum.Parse(_type,_value,true);
        }
        protected object ToBool(string _value) 
        {
            bool _outValue;
            bool.TryParse(_value,out _outValue);
            return _outValue;
        }

        protected object ToShort(string _value) 
        {
            short _outValue;
            short.TryParse(_value, out _outValue);
            return _outValue;
        }

        protected object ToInt(string _value) 
        {
            int _outValue;
            int.TryParse(_value, out _outValue);
            return _outValue;
        }

        protected object ToLong(string _value)
        {
            long _outValue;
            long.TryParse(_value, out _outValue);
            return _outValue;
        }

        protected object ToFloat(string _value)
        {
            float _outValue;
            float.TryParse(_value, out _outValue);
            return _outValue;
        }

        protected object ToDouble(string _value)
        {
            double _outValue;
            double.TryParse(_value, out _outValue);
            return _outValue;
        }
        #endregion
    }
}
