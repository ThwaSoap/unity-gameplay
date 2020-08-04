using System;
using System.Collections.Generic;

namespace GamePlay.Internal
{
    interface IDBPropertyConvertForStorage 
    {
        void InitializedCustomEnum ( Type[] _types );
        DBPropertyConvertInfo FindBaseInfo(string _typeName);
    }
    class DBPropertyConvertForStorage : DBPropertyConvert,IDBPropertyConvertForStorage
    {
        List<Type> CustomEnum = new List<Type>();

        void IDBPropertyConvertForStorage.InitializedCustomEnum ( Type[] _types )
        {
            for ( int i = 0, max = _types.Length ; i < max ; i++ )
            {
                if ( _types[i].IsEnum )
                {
                    CustomEnum.Add ( _types[i] );
                }
            }
        }

        public DBPropertyConvertInfo FindBaseInfo ( string _typeName )
        {
            if ( _typeName.Length == 0 ) return null;
            var result = Infos.Find((v) => { return v.IsType(_typeName); });

            if ( result != null )
            {
                return result;
            }
            else
            {
                var type = CustomEnum.Find ((v)=> { return _typeName == v.Name; } );

                if ( type != null )
                {
                    result = new DBPropertyConvertInfo ( type, null, new DBDataEnum ( type ), ObjectTo );
                    Infos.Add ( result );
                    return result;
                }
            }

            return null;
        }
    }
}
