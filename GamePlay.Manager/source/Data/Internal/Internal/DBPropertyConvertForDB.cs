using System;

namespace GamePlay.Internal
{ 
    interface IDBPropertyConvertForDB 
    {
        DBPropertyConvertInfo FindBaseInfo(Type _type);
    }

    class DBPropertyConvertForDB : DBPropertyConvert, IDBPropertyConvertForDB
    {
        public DBPropertyConvertInfo FindBaseInfo(Type _type)
        {
            var result = Infos.Find((v) => { return v.BaseType.Equals(_type); });

            if (result != null)
            {
                return result;
            }
            else if (_type.IsEnum && false == _type.IsArray)
            {
                result = new DBPropertyConvertInfo(_type, null, new DBDataEnum(_type), ObjectTo);
                Infos.Add (result);
                return result;
            }

            return null;
        }

        bool IsArrayEnum(Type _type)
        {
            return _type.IsArray && _type.IsEnum;
        }

        bool IsListEnum(Type _type)
        {
            return _type.Name == "List`1" && (_type.GetGenericArguments()[0]).IsEnum;
        }
    }
}
